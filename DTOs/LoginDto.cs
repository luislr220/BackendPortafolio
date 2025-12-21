using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = string.Empty;
}