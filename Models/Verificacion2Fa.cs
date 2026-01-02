using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendPortafolio.Models;

[Table("verificaciones_2fa")]
public class Verificacion2Fa
{
    [Key]
    [Column("id")]
    public int Id {get; set;}

    [Column("usuario_id")]
    public int UsuarioId {get; set;}

    [Column("codigo")]
    public string Codigo {get; set;} = string.Empty;

    [Column("expiracion")]
    public DateTime Expiracion {get; set;}

    [Column("usado")]
    public bool Usado {get; set;}

    
}