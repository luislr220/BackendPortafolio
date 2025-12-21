using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendPortafolio.Data;
using BackendPortafolio.Models;
using BackendPortafolio.DTOs;

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
            return BadRequest("El correo ya está registrado.");
        }

        // 2. Mapear el DTO al Modelo real
        var usuario = new Usuario
        {
            NombreUsuario = registroDto.NombreUsuario,
            Correo = registroDto.Correo,
            Contrasena = registroDto.Contrasena
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario creado con éxito", id = usuario.Id });
    }

    //POST: api/Usuarios/login
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        //BUscar al usuario por correo
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == loginDto.Correo);

        //Si no existe o la contraseña no coincide
        if (usuario == null || usuario.Contrasena != loginDto.Contrasena)
        {
            return Unauthorized("Correo o contraseña incorrectos.");
        }

        return Ok(new
        {
            mensaje = "Login exitoso",
            usuarioId = usuario.Id,
            nombre = usuario.NombreUsuario
        });
    }
}