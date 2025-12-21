using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class ProyectoCreacionDto
{
    [Required(ErrorMessage = "El Titulo es obligatorio")]
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? UrlRepositorio { get; set; }
    public string? UrlDemo { get; set; }
    [Required]
    public int UsuarioId { get; set; }

    //Lista de Ids de Tecnologias
    public List<int> TecnologiasIds { get; set; } = new();
}