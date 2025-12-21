using BackendPortafolio.Data;
using BackendPortafolio.DTOs;
using BackendPortafolio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendPortafolio.Controllers;

[ApiController]
[Route("api/[controller]")] //api/proyectos
public class ProyectosController : ControllerBase
{
    private readonly AppDbContext _context;
    public ProyectosController(AppDbContext context)
    {
        _context = context;
    }

    //GET: api/proyectos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProyectoReadDto>>> GetProyectos()
    {
        var proyectos = await _context.Proyectos.
            Include(p => p.ProyectoTecnologias).
            ThenInclude(pt => pt.Tecnologia).
            Select(p => new ProyectoReadDto
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                UrlRepositorio = p.UrlRepositorio,
                UrlDemo = p.UrlDemo,
                Tecnologias = p.ProyectoTecnologias.Select(pt => pt.Tecnologia!.Nombre).ToList()
            }).ToListAsync();
        return Ok(proyectos);
    }

    //POST: api/proyectos
    [HttpPost]
    public async Task<ActionResult<ProyectoReadDto>> PostProyecto(ProyectoCreacionDto proyectoDto)
    {
        // 1. Mapear del DTO de creación al Modelo real de la BD
        var proyecto = new Proyecto
        {
            Titulo = proyectoDto.Titulo,
            Descripcion = proyectoDto.Descripcion,
            UrlRepositorio = proyectoDto.UrlRepositorio,
            UrlDemo = proyectoDto.UrlDemo,
            UsuarioId = proyectoDto.UsuarioId
        };

        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync();

        // 2. Lógica para las tecnologías (si las hay)
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

        return CreatedAtAction(nameof(GetProyectos), new { id = proyecto.Id }, respuesta);
    }

    //DELETE: api/proyectos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProyecto(int id)
    {
        var proyecto = await _context.Proyectos.FindAsync(id);

        if (proyecto == null) return NotFound();

        var tituloProyecto = proyecto.Titulo;

        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync();

        return Ok(
            new
            {
                mensaje = $"El proyecto '{tituloProyecto}' se ha eiminado exitosamente."
            }
        );
    }

    //PUT: api/proyectos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProyecto(int id, ProyectoActualizarDto proyectoDto)
    {
        var proyectoEnDb = await _context.Proyectos
            .Include(p => p.ProyectoTecnologias)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proyectoEnDb == null) return NotFound("El proyecto no existe.");

        if (!string.IsNullOrWhiteSpace(proyectoDto.Titulo))
        {
            proyectoEnDb.Titulo = proyectoDto.Titulo;
        }

        if (!string.IsNullOrWhiteSpace(proyectoDto.Descripcion))
        {
            proyectoEnDb.Descripcion = proyectoDto.Descripcion;
        }

        if (!string.IsNullOrWhiteSpace(proyectoDto.UrlRepositorio))
        {
            proyectoEnDb.UrlRepositorio = proyectoDto.UrlRepositorio;
        }

        if (!string.IsNullOrWhiteSpace(proyectoDto.UrlDemo))
        {
            proyectoEnDb.UrlDemo = proyectoDto.UrlDemo;
        }

        if (proyectoDto.TecnologiasIds != null && proyectoDto.TecnologiasIds.Any())
        {
            var tecnologiasActuales = _context.ProyectoTecnologias.Where(pt => pt.ProyectoId == id);
            _context.ProyectoTecnologias.RemoveRange(tecnologiasActuales);

            foreach (var techId in proyectoDto.TecnologiasIds)
            {
                _context.ProyectoTecnologias.Add(new ProyectoTecnologia
                {
                    ProyectoId = id,
                    TecnologiaId = techId
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Proyecto actualizado correctamente." });
    }

}