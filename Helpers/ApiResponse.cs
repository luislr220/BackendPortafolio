namespace BackendPortafolio.Helpers;

public class ApiResponse<T>
{
    public bool Exito {get; set;}
    public string Mensaje {get; set;} = string.Empty;
    public T? Datos {get; set;} //DTOs o Listas
}