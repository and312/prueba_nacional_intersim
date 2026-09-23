using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int TokenExpiracionSegundos => (int)(_configuration.GetValue<double>("Jwt:ExpiryMinutes", 60) * 60);
    public int RefreshTokenExpiracionDias => _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

    public string GenerateToken(Usuario usuario)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no está configurado.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(JwtRegisteredClaimNames.Name, usuario.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        string? userArea = usuario.Area?.Nombre;
        if (string.IsNullOrEmpty(userArea) && !string.IsNullOrEmpty(usuario.Correo))
        {
            string emailLower = usuario.Correo.ToLower();
            if (emailLower.StartsWith("admin")) userArea = "Tecnología (TI)";
            else if (emailLower.StartsWith("rrhh")) userArea = "Recursos Humanos";
            else if (emailLower.StartsWith("gerente")) userArea = "Gerencia General";
            else if (emailLower.StartsWith("comercial")) userArea = "Comercial";
            else if (emailLower.StartsWith("finanzas") || emailLower.StartsWith("alexander")) userArea = "Finanzas";
        }
        if (!string.IsNullOrEmpty(userArea))
        {
            claimsList.Add(new Claim("area", userArea));
        }
        // Agregar roles
        foreach (var rol in usuario.Roles)
        {
            claimsList.Add(new Claim(ClaimTypes.Role, rol.Nombre));
        }

        // Agregar permisos granulares
        var permissions = usuario.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct();
        foreach (var permission in permissions)
        {
            claimsList.Add(new Claim("permission", permission));
        }

        var expiryMinutes = _configuration.GetValue<double>("Jwt:ExpiryMinutes", 60);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["Jwt:Issuer"] ?? "NacionalSeguros.SIR",
            Audience = _configuration["Jwt:Audience"] ?? "NacionalSeguros.SIR.Clients",
            Subject = new ClaimsIdentity(claimsList),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = creds
        };

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no está configurado.");
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = _configuration["Jwt:Audience"] ?? "NacionalSeguros.SIR.Clients",
            ValidIssuer = _configuration["Jwt:Issuer"] ?? "NacionalSeguros.SIR",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false // Permitir tokens expirados
        };

        var tokenHandler = new JsonWebTokenHandler();
        var result = tokenHandler.ValidateTokenAsync(token, tokenValidationParameters).GetAwaiter().GetResult();

        if (!result.IsValid)
        {
            throw new SecurityTokenException("Token inválido: " + result.Exception?.Message);
        }

        if (result.SecurityToken is not JsonWebToken jsonWebToken || 
            !jsonWebToken.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Token de firma inválido.");
        }

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}
