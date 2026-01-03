using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class CambiarContrasenaDto
{
    [Required(ErrorMessage = "El token JWT es obligatorio.")]
    public string tokenJwt { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de la nueva contraseña es obigatoria.")]
    [Compare("NuevaContrasena", ErrorMessage = "La confirmación de la nueva contraseña no coincide con la nueva contraseña.")]
    public string ConfirmarNuevaContrasena { get; set; } = string.Empty;
}