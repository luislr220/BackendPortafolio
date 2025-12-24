using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es el esperado. e.j: example@example.com")]
    public string Correo { get; set; } = string.Empty;


    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", 
        ErrorMessage = "La contraseña no puede contener caracteres especiales como < > & % $ /")]
    public string Contrasena { get; set; } = string.Empty;
}