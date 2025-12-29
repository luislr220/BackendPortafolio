using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendPortafolio.Models;

[Table("proyecto_imagenes")]
public class ProyectoImagen
{
    [Key]
    [Column("id")]
    public int Id {get; set;}

    [Required]
    [Column("url")]
    public string Url {get; set;} = string.Empty;

    [Required]
    [Column("public_id")]
    public string PublicId {get; set;} = string.Empty;

    [Column("proyecto_id")]
    public int ProyectoId {get; set;}

    [ForeignKey("ProyectoId")]
    public Proyecto? Proyecto {get; set;}
}