using System.Security.Claims;
using BackendPortafolio.Data;
using BackendPortafolio.DTOs;
using BackendPortafolio.Helpers;
using BackendPortafolio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendPortafolio.Controllers;

[ApiController]
[Route("api/[controller]")] //api/proyectos
public class ProyectosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IImagenServicio _imagenServicio;
    public ProyectosController(AppDbContext context, IImagenServicio imagenServicio)
    {
        _context = context;
        _imagenServicio = imagenServicio;
    }

    //GET: api/proyectos
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProyectoReadDto>>>> GetProyectos()
    {
        var proyectos = await _context.Proyectos.
            Include(p => p.ProyectoTecnologias).
                ThenInclude(pt => pt.Tecnologia).
            Include(pi => pi.ProyectoImagenes).
            Select(p => new ProyectoReadDto
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                UrlRepositorio = p.UrlRepositorio,
                UrlDemo = p.UrlDemo,
                Tecnologias = p.ProyectoTecnologias.Select(pt => pt.Tecnologia!.Nombre).ToList(),
                Imagenes = p.ProyectoImagenes.Select(pi => new ProyectoImagenDto
                {
                    Id = pi.Id,
                    Url = pi.Url
                }).ToList()
            }).ToListAsync();
        return Ok(new ApiResponse<IEnumerable<ProyectoReadDto>>
        {
            Exito = true,
            Mensaje = "Proyectos obtenidos correctamente.",
            Datos = proyectos
        });
    }

    //POST: api/proyectos
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProyectoReadDto>>> PostProyecto([FromForm] ProyectoCreacionDto proyectoDto)
    {

        //Extraer el ID directamente del Token
        var idToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Mapear del DTO de creación al Modelo real de la BD
        var proyecto = new Proyecto
        {
            Titulo = proyectoDto.Titulo,
            Descripcion = proyectoDto.Descripcion,
            UrlRepositorio = proyectoDto.UrlRepositorio,
            UrlDemo = proyectoDto.UrlDemo,
            UsuarioId = idToken
        };

        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync();

        //Logica de imagenes
        if (proyectoDto.Imagenes != null && proyectoDto.Imagenes.Any())
        {
            foreach (var archivo in proyectoDto.Imagenes)
            {
                var resultado = await _imagenServicio.SubirImagenAsync(archivo);
                if (resultado.Error == null)
                {
                    _context.ProyectoImagenes.Add(new ProyectoImagen
                    {
                        ProyectoId = proyecto.Id,
                        Url = resultado.SecureUrl.AbsoluteUri,
                        PublicId = resultado.PublicId
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        // Lógica para las tecnologías (si las hay)
        if (proyectoDto.TecnologiasIds.Any())
        {
            foreach (var techId in proyectoDto.TecnologiasIds)
            {
                _context.ProyectoTecnologias.Add(new ProyectoTecnologia
                {
                    ProyectoId = proyecto.Id,
                    TecnologiaId = techId
                });
            }
            await _context.SaveChangesAsync();
        }

        var respuesta = new ProyectoReadDto
        {
            Id = proyecto.Id,
            Titulo = proyecto.Titulo,
            Descripcion = proyecto.Descripcion,
            UrlRepositorio = proyecto.UrlRepositorio,
            UrlDemo = proyecto.UrlDemo
        };

        var apiResponse = new ApiResponse<ProyectoReadDto>
        {
            Exito = true,
            Mensaje = "Proyecto creado con exito",
            Datos = respuesta
        };

        return CreatedAtAction(nameof(GetProyectos), new { id = proyecto.Id }, apiResponse);
    }

    //DELETE: api/proyectos/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> DeleteProyecto(int id)
    {
        var proyecto = await _context.Proyectos.FindAsync(id);

        if (proyecto == null) return NotFound(new ApiResponse<string>
        {
            Exito = false,
            Mensaje = "El proyecto no existe."
        });

        // Extraer ID del token
        var idToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (proyecto.UsuarioId != idToken)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<string>
            {
                Exito = false,
                Mensaje = "No tienes permiso para eliminar un proyecto que no te pertenece."
            });
        }

        var tituloProyecto = proyecto.Titulo;

        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync();

        return Ok(
            new ApiResponse<string>
            {
                Exito = true,
                Mensaje = $"El proyecto '{tituloProyecto}' se ha eiminado exitosamente."
            }
        );
    }

    //PUT: api/proyectos/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> PutProyecto(int id, [FromForm] ProyectoActualizarDto proyectoDto, List<IFormFile>? nuevasImagenes)
    {
        var proyectoEnDb = await _context.Proyectos
            .Include(p => p.ProyectoTecnologias)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proyectoEnDb == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "El proyecto no existe" });

        // Validar Propiedad
        if (!EsDuenoDelProyecto(proyectoEnDb))
            return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<string> { Exito = false, Mensaje = "Sin permisos." });

        // Actualizar campos básicos
        ActualizarDatosBasicos(proyectoEnDb, proyectoDto);

        // Actualizar Tecnologías
        ActualizarTecnologias(id, proyectoDto.TecnologiasIds);

        // Procesar nuevas imágenes
        await ProcesarNuevasImagenes(id, nuevasImagenes);

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Exito = true,
            Mensaje = "Proyecto actualizado correctamente."
        });
    }

    private bool EsDuenoDelProyecto(Proyecto proyecto)
    {
        //Extraer ID del token
        var idToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return proyecto.UsuarioId == idToken;
    }

    private static void ActualizarDatosBasicos(Proyecto proyecto, ProyectoActualizarDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Titulo)) proyecto.Titulo = dto.Titulo;
        if (!string.IsNullOrWhiteSpace(dto.Descripcion)) proyecto.Descripcion = dto.Descripcion;
        if (!string.IsNullOrWhiteSpace(dto.UrlRepositorio)) proyecto.UrlRepositorio = dto.UrlRepositorio;
        if (!string.IsNullOrWhiteSpace(dto.UrlDemo)) proyecto.UrlDemo = dto.UrlDemo;
    }

    private void ActualizarTecnologias(int proyectoId, List<int> nuevasTecs)
    {
        if (nuevasTecs == null || !nuevasTecs.Any()) return;

        var actuales = _context.ProyectoTecnologias.Where(pt => pt.ProyectoId == proyectoId);
        _context.ProyectoTecnologias.RemoveRange(actuales);

        foreach (var techId in nuevasTecs)
        {
            _context.ProyectoTecnologias.Add(new ProyectoTecnologia { ProyectoId = proyectoId, TecnologiaId = techId });
        }
    }

    private async Task ProcesarNuevasImagenes(int proyectoId, List<IFormFile>? imagenes)
    {
        if (imagenes == null || !imagenes.Any()) return;

        foreach (var archivo in imagenes)
        {
            var resultado = await _imagenServicio.SubirImagenAsync(archivo);
            if (resultado.Error == null)
            {
                _context.ProyectoImagenes.Add(new ProyectoImagen
                {
                    ProyectoId = proyectoId,
                    Url = resultado.SecureUrl.AbsoluteUri,
                    PublicId = resultado.PublicId
                });
            }
        }
    }


    //ENDPOINT PARA ELIMINAR IMAGENES DE PROYECTOS
    [HttpDelete("imagen/{imagenId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> EliminarImagen(int imagenId)
    {
        var imagen = await _context.ProyectoImagenes
            .Include(pi => pi.Proyecto)
            .FirstOrDefaultAsync(i => i.Id == imagenId);

        if (imagen == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "No se encontro una imagen con ese id." });

        //validar que el usuario que borra la imagen sea el dueño
        var idToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (imagen.Proyecto!.UsuarioId != idToken) return StatusCode(StatusCodes.Status403Forbidden,
            new ApiResponse<string>
            {
                Exito = false,
                Mensaje = "No tienes permiso para eliminar esta imagen."
            });

        //Borrar de cloudinary
        await _imagenServicio.EliminarImagenAsync(imagen.PublicId);

        //Borrar de la db
        _context.ProyectoImagenes.Remove(imagen);

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string> { Exito = true, Mensaje = "La imagen se elimino correctamente." });

    }

}