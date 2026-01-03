using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class ConfirmarRecuperacionDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    public string Correo { get; set; } = string.Empty;
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;
}