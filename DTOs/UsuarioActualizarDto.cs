using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class UsuarioActualizarDto
{

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}