namespace BackendPortafolio.DTOs;

public class UsuarioLeerDto
{

    
    public string? NombreUsuario { get; set; }

    public string? Correo { get; set; }
    public List<ProyectoReadDto> Proyectos {get; set;} = new();

}