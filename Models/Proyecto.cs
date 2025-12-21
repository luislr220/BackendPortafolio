using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendPortafolio.Models;

[Table("proyectos")]
public class Proyecto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("titulo")]
    [MaxLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("url_repositorio")]
    public string? UrlRepositorio { get; set; }

    [Column("url_demo")]
    public string? UrlDemo { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    // Propiedades de navegación
    [ForeignKey("UsuarioId")]
    public Usuario? Usuario { get; set; }

    public List<ProyectoTecnologia> ProyectoTecnologias { get; set; } = new();
}