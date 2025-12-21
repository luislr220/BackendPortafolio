namespace BackendPortafolio.DTOs;

public class UsuarioLeerDto
{

    
    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;
    public List<ProyectoReadDto> Proyectos {get; set;} = new();

}