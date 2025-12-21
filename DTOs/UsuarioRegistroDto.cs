using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class UsuarioRegistroDto
{
    [Required]
    [MaxLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Contrasena { get; set; } = string.Empty;
}