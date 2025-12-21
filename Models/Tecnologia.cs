using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendPortafolio.Models;

[Table("tecnologias")]
public class Tecnologia
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nombre")]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public List<ProyectoTecnologia> ProyectoTecnologias { get; set; } = new();
}