using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Application.Abstractions.Notifications;

namespace NacionalSeguros.Infrastructure.Notifications;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation(
            "----- ENVIANDO CORREO DE SEGURIDAD -----\n" +
            "Para: {To}\n" +
            "Asunto: {Subject}\n" +
            "Mensaje: {Body}\n" +
            "---------------------------------------",
            to, subject, body);

        return Task.CompletedTask;
    }
}
