using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class UsuarioActualizarDto
{

    [MaxLength(50, ErrorMessage = "El nombre no debe contener más de 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s]+$",
    ErrorMessage = @"El campo contiene caracteres no permitidos. Evite usar símbolos como < > / \ & % $")]
    public string? NombreUsuario { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El formato de correo no es el esperado. e.j: example@example.com")]
    public string? Correo { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string? Contrasena { get; set; } = string.Empty;
}