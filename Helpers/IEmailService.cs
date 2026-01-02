namespace BackendPortafolio.Services;

public interface IEmailService
{
    Task EnviarCorreoAsync(string destinatario, string asunto, string nombreTemplate, Dictionary<string,string> remplazos);
}