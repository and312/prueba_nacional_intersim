using System.Security.Claims;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Application.Abstractions.Security;

public interface IJwtService
{
    string GenerateToken(Usuario usuario);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    int TokenExpiracionSegundos { get; }
    int RefreshTokenExpiracionDias { get; }
}
