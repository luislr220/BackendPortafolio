using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class TecnologiaDto
{
    [Required(ErrorMessage = "El nombre de la tecnología es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El nombre de a tecnología no puede exceder de los 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}