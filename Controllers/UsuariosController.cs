using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendPortafolio.Data;
using BackendPortafolio.Models;
using BackendPortafolio.DTOs;
using BackendPortafolio.Helpers;

namespace BackendPortafolio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    //POST: api/Usuarios/registro
    [HttpPost("registro")]
    public async Task<ActionResult> Registrar(UsuarioRegistroDto registroDto)
    {
        // 1. Verificar si el correo ya existe
        if (await _context.Usuarios.AnyAsync(u => u.Correo == registroDto.Correo))
        {
            return BadRequest(new ApiResponse<string> {Exito = false, Mensaje ="El correo ya está registrado."} );
        }

        // 2. Mapear el DTO al Modelo real
        var usuario = new Usuario
        {
            NombreUsuario = registroDto.NombreUsuario,
            Correo = registroDto.Correo,
            Contrasena = BCrypt.Net.BCrypt.HashPassword(registroDto.Contrasena)
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>{ Exito = true, Mensaje = "Usuario creado con éxito", Datos = new {id = usuario.Id} });
    }

    //POST: api/Usuarios/login
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        //BUscar al usuario por correo
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == loginDto.Correo);

        //Si no existe o la contraseña no coincide
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contrasena, usuario.Contrasena))
        {
            return Unauthorized(new ApiResponse<string> { Exito = false, Mensaje = "Correo o contraseña incorrectos." });
        }

        return Ok(new ApiResponse<object>
        {
            Exito = true,
            Mensaje = "Login exitoso",
            Datos = new
            {
                usuarioId = usuario.Id,
                nombre = usuario.NombreUsuario
            }
        });
    }

    //PUT: api/usuarios/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarUsuario(int id, UsuarioActualizarDto actualizarDto)
    {
        var usuarioEnDB = await _context.Usuarios.FindAsync(id);
        if (usuarioEnDB == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "No se encontro el usuario" });

        //Validar el nombre del usuario

        if (!string.IsNullOrWhiteSpace(actualizarDto.NombreUsuario))
        {
            usuarioEnDB.NombreUsuario = actualizarDto.NombreUsuario;
        }

        //Validar el correo y que no este dupicado por otro usuario
        if (!string.IsNullOrWhiteSpace(actualizarDto.Correo))
        {
            if (!actualizarDto.Correo.Contains('@')) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "El formato del correo no es valido." });

            var correoExiste = await _context.Usuarios.
                AnyAsync(u => u.Correo == actualizarDto.Correo && u.Id != id);

            if (correoExiste) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = $"El correo '{actualizarDto.Correo}' ya esta en uso por otra cuenta." });

            usuarioEnDB.Correo = actualizarDto.Correo;
        }

        //validar y encriptar nueva contraseña
        if (!string.IsNullOrWhiteSpace(actualizarDto.Contrasena))
        {
            if(actualizarDto.Contrasena.Length < 6)
                return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "La contraseña debe tener almenos 6 caracteres." });

            usuarioEnDB.Contrasena = BCrypt.Net.BCrypt.HashPassword(actualizarDto.Contrasena);
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string> { Exito = true, Mensaje = $"Usuario '{usuarioEnDB.NombreUsuario}' actualizado correctamente." });
    }


    //DELETE: api/usuarios/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        var usuarioEnDb = await _context.Usuarios.FindAsync(id);
        if (usuarioEnDb == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "No se encontro el usuario" });

        _context.Usuarios.Remove(usuarioEnDb);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<string> { Exito = true, Mensaje = $"Usuario '{usuarioEnDb.NombreUsuario}' eliminado correctamente." });
    }

    //GET: api/usuarios/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioLeerDto>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioLeerDto
            {
              NombreUsuario = u.NombreUsuario,
              Correo = u.Correo,
              Proyectos = u.Proyectos.Select(p => new ProyectoReadDto
              {
                  Id = p.Id,
                  Titulo = p.Titulo,
                  Descripcion = p.Descripcion,
                  UrlRepositorio = p.UrlRepositorio,
                  UrlDemo = p.UrlDemo,
                  Tecnologias = p.ProyectoTecnologias.Select(pt => pt.Tecnologia!.Nombre).ToList()
              }).ToList()  
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "Usuario no encontrado." });
        }

        return Ok(new ApiResponse<UsuarioLeerDto>
        {
            Exito = true,
            Mensaje = "Usuario obtenido correctamente.",
            Datos = usuario
        });

    }
}