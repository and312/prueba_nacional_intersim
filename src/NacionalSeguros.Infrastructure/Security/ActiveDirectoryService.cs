using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NacionalSeguros.Application.Abstractions.Security;

namespace NacionalSeguros.Infrastructure.Security;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public ActiveDirectoryService(IConfiguration configuration, ILogger<ActiveDirectoryService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> AuthenticateAsync(string correo, string password)
    {
        _logger.LogInformation("Iniciando intento de autenticación en Active Directory para: {Correo}", correo);

        var simulate = _configuration.GetValue<bool>("ActiveDirectory:Simulate", false);

        if (simulate)
        {
            _logger.LogWarning("Autenticación Active Directory en modo SIMULACIÓN. Aceptando credenciales.");
            // En simulación local, se aceptará cualquier clave que tenga al menos 8 caracteres
            bool isValid = !string.IsNullOrEmpty(password) && password.Length >= 8;
            return Task.FromResult(isValid);
        }

        try
        {
            // TODO(security): Configurar LdapConnection contra Microsoft Entra ID en producción
            _logger.LogError("Autenticación LDAP corporativa real no configurada.");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción ocurrida durante autenticación LDAP.");
            return Task.FromResult(false);
        }
    }
}
