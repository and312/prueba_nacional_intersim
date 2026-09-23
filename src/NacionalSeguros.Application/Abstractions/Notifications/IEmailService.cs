using System.Threading.Tasks;

namespace NacionalSeguros.Application.Abstractions.Notifications;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
