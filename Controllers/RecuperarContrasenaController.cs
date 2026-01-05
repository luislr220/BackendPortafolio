using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendPortafolio.Data;
using BackendPortafolio.DTOs;
using BackendPortafolio.Helpers;
using BackendPortafolio.Models;
using BackendPortafolio.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendPortafolio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecuperarContrasenaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public RecuperarContrasenaController(AppDbContext context, IEmailService emailService, IConfiguration config)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
    }

    [HttpPost("recuperar-cuenta")]
    public async Task<IActionResult> RecuperarCuenta(RecuperarCuentaDto recuperarCuentaDto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == recuperarCuentaDto.Correo);

        if (usuario == null) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "El correo no esta registrado." });

        string codigoGenerado = SeguridadesHelper.GenerarCodigo2Fa();

        var verificacion = new Verificacion2Fa
        {
            UsuarioId = usuario.Id,
            Codigo = codigoGenerado,
            Expiracion = DateTime.UtcNow.AddMinutes(5),
            Usado = false
        };

        var codigosViejos = _context.Verificacion2Fas
            .Where(v => v.UsuarioId == usuario.Id);
        _context.Verificacion2Fas.RemoveRange(codigosViejos);

        _context.Verificacion2Fas.Add(verificacion);
        await _context.SaveChangesAsync();

        var asunto = "Recuperación de cuenta - Portafolio Dev";
        var datosCorreo = new Dictionary<string, string>
        {
          {"Usuario", usuario.NombreUsuario ?? "Usuario"},
          {"Codigo", codigoGenerado}
        };

        await _emailService.EnviarCorreoAsync(recuperarCuentaDto.Correo, asunto, "Email2fa", datosCorreo);

        return Ok(new ApiResponse<object>
        {
            Exito = true,
            Mensaje = "Se ha enviado un código de verificación a su correo electrónico."
        });
    }

    [HttpPost("verificar-codigo")]
    public async Task<ActionResult> VerificarCodigo(ConfirmarRecuperacionDto confirmarRecuperacionDto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == confirmarRecuperacionDto.Correo);

        if (usuario == null) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "No se ha encontrado esta cuenta." });

        var verificacion = await _context.Verificacion2Fas
            .Where(v => v.UsuarioId == usuario.Id && v.Codigo == confirmarRecuperacionDto.Codigo && !v.Usado)
            .OrderByDescending(v => v.Expiracion)
            .FirstOrDefaultAsync();

        if (verificacion == null) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "No hay un código pendiente." });

        if (verificacion.Expiracion < DateTime.UtcNow)
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "El código ha expirado." });
        }

        verificacion.Usado = true;
        await _context.SaveChangesAsync();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim("PasswordStamp", usuario.Contrasena.Substring(usuario.Contrasena.Length - 10)),
                new Claim("Purpose", "ResetPassword")
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new ApiResponse<object>
        {
            Exito = true,
            Mensaje = "Código verificado correctamente.",
            Datos = new { TokenReset = tokenString }
        });
    }

    [HttpPost("cambiar-contrasena")]
    public async Task<ActionResult> CambiarContrasena(CambiarContrasenaDto cambiarContrasenaDto)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(cambiarContrasenaDto.tokenJwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true
            }, out _);

            var purposeClaim = principal.FindFirst("Purpose")?.Value;
            if (purposeClaim != "ResetPassword")
            {
                return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Token invalido para esta operación." });
            }

            var usuarioIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdStr)) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Token invalido." });

            int usuarioId = int.Parse(usuarioIdStr);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null) return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Usuario no encontrado." });

            var stampEnToken = principal.FindFirst("PasswordStamp")?.Value;
            var stampActualEnDb = usuario.Contrasena.Substring(usuario.Contrasena.Length - 10);

            if (stampEnToken != stampActualEnDb)
            {
                return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Este enlace de recuperación ya ha sido uttilizado." });
            }

            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(cambiarContrasenaDto.NuevaContrasena);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Exito = true,
                Mensaje = "Contraseña cambiada exitosamente."
            });
        }
        catch (System.Exception)
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "El token ha expirado o es inválido." });
        }
    }
}