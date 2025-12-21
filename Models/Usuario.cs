using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendPortafolio.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("nombre_usuario")]
    [MaxLength(50)]
    public string? NombreUsuario { get; set; }

    [Required]
    [Column("contrasena")]
    public string Contrasena { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Column("correo")]
    [MaxLength(100)]
    public string Correo { get; set; } = string.Empty;

    // Relación de un usuario a mmuchos proyectos
    public List<Proyecto> Proyectos { get; set; } = new();
}