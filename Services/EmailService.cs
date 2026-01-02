using BackendPortafolio.Models;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace BackendPortafolio.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IWebHostEnvironment _env;

    public EmailService(IOptions<EmailSettings> emailSettings, IWebHostEnvironment env)
    {
        _emailSettings = emailSettings.Value;
        _env = env;
    }

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string nombreTemplate, Dictionary<string, string> remplazos)
    {
        Configuration.Default.ApiKey["api-key"] = _emailSettings.ApiKey;

        string rutaArchivo = Path.Combine(_env.ContentRootPath, "Templates", $"{nombreTemplate}.html");
        string contenidoHtml = await File.ReadAllTextAsync(rutaArchivo);
        foreach (var item in remplazos)
        {
            contenidoHtml = contenidoHtml.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        var apiInstance = new TransactionalEmailsApi();

        var email = new SendSmtpEmail(
            sender: new SendSmtpEmailSender(_emailSettings.SenderName, _emailSettings.SenderEmail),
            to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(destinatario) },
            subject: asunto,
            htmlContent: contenidoHtml
        );

        try
        {
            await apiInstance.SendTransacEmailAsync(email);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al enviar con Brevo: " + ex.Message);
            throw;
        }

    }
}