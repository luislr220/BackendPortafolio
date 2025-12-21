namespace BackendPortafolio.DTOs;

public class ProyectoReadDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? UrlRepositorio { get; set; }
    public string? UrlDemo { get; set; }
    public List<string> Tecnologias { get; set; } = new();
}