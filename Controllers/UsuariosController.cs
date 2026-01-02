using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendPortafolio.Data;
using BackendPortafolio.Models;
using BackendPortafolio.DTOs;
using BackendPortafolio.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BackendPortafolio.Services;
using System.Diagnostics;

namespace BackendPortafolio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    public UsuariosController(AppDbContext context, IConfiguration config, IEmailService emailService)
    {
        _context = context;
        _config = config;
        _emailService = emailService;
    }

    //POST: api/Usuarios/registro
    [HttpPost("registro")]
    public async Task<ActionResult> Registrar(UsuarioRegistroDto registroDto)
    {
        // 1. Verificar si el correo ya existe
        if (await _context.Usuarios.AnyAsync(u => u.Correo == registroDto.Correo))
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "El correo ya está registrado." });
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

        return Ok(new ApiResponse<object> { Exito = true, Mensaje = "Usuario creado con éxito", Datos = new { id = usuario.Id } });
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

        //Generar código para el 2FA
        string codigoGenerado = SeguridadesHelper.GenerarCodigo2Fa();

        //Crear el objeto de verificación
        var verificacion = new Verificacion2Fa
        {
            UsuarioId = usuario.Id,
            Codigo = codigoGenerado,
            Expiracion = DateTime.UtcNow.AddMinutes(5),
            Usado = false
        };

        var codigosViejos = _context.Verificacion2Fas.Where(v => v.UsuarioId == usuario.Id);
        _context.Verificacion2Fas.RemoveRange(codigosViejos);

        //Guardar en la db
        _context.Verificacion2Fas.Add(verificacion);
        await _context.SaveChangesAsync();

        var asunto = "Código de verificaión para el Portafolio Dev";
        var datosCorreo = new Dictionary<string, string>
        {
            {"Usuario", usuario.NombreUsuario ?? "Usuario"},
            {"Codigo", codigoGenerado}
        };

        await _emailService.EnviarCorreoAsync(loginDto.Correo, asunto, "Email2fa", datosCorreo);

        return Ok(new ApiResponse<object>
        {
            Exito = true,
            Mensaje = "Código de verificación enviado al correo. Por favor revisa tu correo.",
            Datos = new
            {
                usuarioId = usuario.Id,
            }
        });
    }

    [HttpPost("confirmar2fa")]
    public async Task<ActionResult> Confirmar2fa(Confirmar2FaDto confirmar2FaDto)
    {
        //Buascar la verificación mas reciente que no haya sido usada
        var verificacion = await _context.Verificacion2Fas
            .Where(v => v.UsuarioId == confirmar2FaDto.UsuarioId && !v.Usado)
            .OrderByDescending(v => v.Expiracion)
            .FirstOrDefaultAsync();


        if (verificacion == null)
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "No hay un código pendiente" });
        }

        if (verificacion.Codigo != confirmar2FaDto.Codigo)
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Código de verificación incorrecto." });
        }
        if (verificacion.Expiracion < DateTime.Now)
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Código de verificación expirado." });
        }

        verificacion.Usado = true;
        await _context.SaveChangesAsync();

        //Generar el token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

        var usuario = await _context.Usuarios.FindAsync(confirmar2FaDto.UsuarioId);
        if (usuario == null) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Usuario no encontrado." });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo)
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(
            new ApiResponse<object>
            {
                Exito = true,
                Mensaje = "Autenticación exitosa.",
                Datos = new
                {
                    token = tokenString,
                    Nombre = usuario.NombreUsuario,
                    IdUsuario = usuario.Id
                }
            }
        );
    }

    //PUT: api/usuarios/perfil
    [HttpPut("perfil")]
    [Authorize]
    public async Task<IActionResult> ActualizarUsuario(UsuarioActualizarDto actualizarDto)
    {
        var idToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var usuarioEnDB = await _context.Usuarios.FindAsync(idToken);
        if (usuarioEnDB == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "No se encontro el usuario" });

        //Validar el nombre del usuario
        if (!string.IsNullOrWhiteSpace(actualizarDto.NombreUsuario))
        {
            usuarioEnDB.NombreUsuario = actualizarDto.NombreUsuario;
        }

        //Validar el correo y que no este dupicado por otro usuario
        if (!string.IsNullOrWhiteSpace(actualizarDto.Correo))
        {
            var correoExiste = await _context.Usuarios.
                AnyAsync(u => u.Correo == actualizarDto.Correo && u.Id != idToken);

            if (correoExiste) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = $"El correo '{actualizarDto.Correo}' ya esta en uso por otra cuenta." });

            usuarioEnDB.Correo = actualizarDto.Correo;
        }

        //validar y encriptar nueva contraseña
        if (!string.IsNullOrWhiteSpace(actualizarDto.Contrasena))
        {
            if (actualizarDto.Contrasena.Length < 6)
                return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "La contraseña debe tener almenos 6 caracteres." });

            usuarioEnDB.Contrasena = BCrypt.Net.BCrypt.HashPassword(actualizarDto.Contrasena);
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string> { Exito = true, Mensaje = $"Usuario '{usuarioEnDB.NombreUsuario}' actualizado correctamente." });
    }


    //DELETE: api/usuarios/perfil
    [HttpDelete("perfil")]
    [Authorize]
    public async Task<IActionResult> EliminarUsuario()
    {
        var idToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var usuarioEnDb = await _context.Usuarios.FindAsync(idToken);
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
                    Tecnologias = p.ProyectoTecnologias.Select(pt => pt.Tecnologia!.Nombre).ToList(),
                    Imagenes = p.ProyectoImagenes.Select(pi => new ProyectoImagenDto
                    {
                        Id = pi.Id,
                        Url = pi.Url
                    }).ToList()
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