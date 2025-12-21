using System.ComponentModel.DataAnnotations.Schema;

namespace BackendPortafolio.Models;

[Table("proyecto_tecnologias")]
public class ProyectoTecnologia
{
    [Column("proyecto_id")]
    public int ProyectoId { get; set; }

    [ForeignKey("ProyectoId")]
    public Proyecto? Proyecto { get; set; }

    [Column("tecnologia_id")]
    public int TecnologiaId { get; set; }

    [ForeignKey("TecnologiaId")]
    public Tecnologia? Tecnologia { get; set; }
}