using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class RecuperarCuentaDto
{
    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;
}