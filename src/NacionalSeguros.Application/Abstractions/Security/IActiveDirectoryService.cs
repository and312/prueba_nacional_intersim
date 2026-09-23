using System.Threading.Tasks;

namespace NacionalSeguros.Application.Abstractions.Security;

public interface IActiveDirectoryService
{
    Task<bool> AuthenticateAsync(string correo, string password);
}
