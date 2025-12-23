using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es el esperado. e.j: example@example.com")]
    public string Correo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = string.Empty;
}