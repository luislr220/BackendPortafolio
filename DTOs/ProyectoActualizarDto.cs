using System.ComponentModel.DataAnnotations;

namespace BackendPortafolio.DTOs;

public class ProyectoActualizarDto
{
    [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,:()""'!-]+$",
        ErrorMessage = @"El campo contiene caracteres no permitidos. Evite usar símbolos como < > / \ & % $")]
    public string? Titulo { get; set; } = string.Empty;
    [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,;()""'!?¡¿\n\r-]+$",
        ErrorMessage = @"El campo contiene caracteres no permitidos. Evite usar símbolos como < > / \ & % $")]
    public string? Descripcion { get; set; }
    [Url(ErrorMessage = "La dirección URL no es válida. Debe incluir http:// o https://")]
    [RegularExpression(@"^[a-zA-Z0-9.:/?#\[\]@!$&'()*+,;=%-]+$",
        ErrorMessage = @"Ingresa un url valida.")]
    public string? UrlRepositorio { get; set; }
    [Url(ErrorMessage = "La dirección URL no es válida. Debe incluir http:// o https://")]
    [RegularExpression(@"^[a-zA-Z0-9.:/?#\[\]@!$&'()*+,;=%-]+$",
        ErrorMessage = @"Ingresa un url valida.")]
    public string? UrlDemo { get; set; }
    public int UsuarioId { get; set; }

    //Lista de Ids de Tecnologias
    public List<int> TecnologiasIds { get; set; } = new();
}