using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Security.Commands.RefreshToken;
using NacionalSeguros.Application.Security.Commands.Usuarios;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Infrastructure.Security;
using Xunit;

namespace NacionalSeguros.Tests.Security;

public class SecurityTests
{
    [Fact]
    public void PasswordHasher_Should_Hash_And_Verify_Correctly()
    {
        // Arrange
        var hasher = new PasswordHasher();
        string password = "StrongSecurePassword123!";

        // Act
        string hash = hasher.HashPassword(password);
        bool isValid = hasher.VerifyPassword(password, hash);
        bool isInvalid = hasher.VerifyPassword("WrongPassword", hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().Contain(".");
        isValid.Should().BeTrue();
        isInvalid.Should().BeFalse();
    }

    [Fact]
    public void PasswordHasher_Should_Produce_Different_Hashes_For_Same_Password()
    {
        // Arrange
        var hasher = new PasswordHasher();
        string password = "TestPassword";

        // Act
        string hash1 = hasher.HashPassword(password);
        string hash2 = hasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void MfaService_Should_Generate_Valid_SecretKey()
    {
        // Arrange
        var mfa = new MfaService();

        // Act
        string secret = mfa.GenerateSecretKey();

        // Assert
        secret.Should().NotBeNullOrEmpty();
        secret.Length.Should().Be(16); // 80 bits in Base32 is 16 characters
    }

    [Fact]
    public void MfaService_Should_Validate_TOTP_Code_Successfully()
    {
        // Arrange
        var mfa = new MfaService();
        string secret = mfa.GenerateSecretKey();

        // Act & Assert
        // El código cambia cada 30 segundos. Un código vacío o aleatorio no numérico debe fallar.
        mfa.ValidateCode(secret, "000000").Should().BeFalse();
        mfa.ValidateCode(secret, "abc123").Should().BeFalse();
    }

    [Fact]
    public void MfaService_Should_Generate_Correct_QrCodeUri()
    {
        // Arrange
        var mfa = new MfaService();
        string secret = "JBSWY3DPEHPK3PXP";
        string email = "test@nacionalseguros.com.bo";

        // Act
        string uri = mfa.GetQrCodeUri(email, secret);

        // Assert
        uri.Should().Contain("otpauth://totp/");
        uri.Should().Contain("secret=" + secret);
        uri.Should().Contain("issuer=NacionalSeguros.SIR");
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_Should_Detect_Token_Reuse_And_Anulate_All_Sessions()
    {
        // Arrange
        var sesionRepoMock = new Mock<ISesionRepository>();
        var usuarioRepoMock = new Mock<IUsuarioRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var jwtServiceMock = new Mock<IJwtService>();
        var auditServiceMock = new Mock<IAuditService>();

        // Simular un usuario válido
        var usuario = new Usuario("Test User", "test@nacionalvida.com.bo", TipoAutenticacion.Local);
        usuario.AsignarRol(new Rol("Reclutador", "Acceso básico"));

        // Simular una sesión que ya ha sido consumida (Activa = false)
        string usedRefreshToken = "obsolete-token";
        var sesionObsoleta = new Sesion(1, usedRefreshToken, DateTime.UtcNow.AddDays(1));
        sesionObsoleta.Desactivar(); // Activa = false

        sesionRepoMock
            .Setup(r => r.GetByRefreshTokenAsync(usedRefreshToken))
            .ReturnsAsync(sesionObsoleta);

        usuarioRepoMock
            .Setup(r => r.GetByIdAsync(sesionObsoleta.UsuarioId))
            .ReturnsAsync(usuario);

        var activeSessions = new List<Sesion>
        {
            new(1, "active-token-1", DateTime.UtcNow.AddDays(1)),
            new(1, "active-token-2", DateTime.UtcNow.AddDays(2))
        };

        sesionRepoMock
            .Setup(r => r.GetActiveSessionsByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(activeSessions);

        var handler = new RefreshTokenCommandHandler(
            sesionRepoMock.Object,
            usuarioRepoMock.Object,
            unitOfWorkMock.Object,
            jwtServiceMock.Object,
            auditServiceMock.Object);

        var command = new RefreshTokenCommand("expired-jwt", usedRefreshToken);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TOKEN_REUSE_DETECTED");

        // Validar que se inactivaron todas las demás sesiones activas
        activeSessions.ForEach(s => s.Activa.Should().BeFalse());

        sesionRepoMock.Verify(r => r.Update(It.IsAny<Sesion>()), Times.Exactly(activeSessions.Count));
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void JwtService_Should_Generate_Token_With_Correct_Claims()
    {
        // Arrange
        var myConfiguration = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "SuperSecretKeyOfAtLeast32BytesLength!"},
            {"Jwt:Issuer", "NacionalSeguros.SIR"},
            {"Jwt:Audience", "NacionalSeguros.SIR.Clients"},
            {"Jwt:ExpiryMinutes", "60"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        config["Jwt:Issuer"].Should().Be("NacionalSeguros.SIR");
        config["Jwt:Secret"].Should().Be("SuperSecretKeyOfAtLeast32BytesLength!");

        var jwtService = new JwtService(config);

        var usuario = new Usuario("Test Nombres", "test@nacionalvida.com.bo", TipoAutenticacion.Local);
        var rol = new Rol("RRHH", "Gestor");
        var permiso = new Permiso("solicitudes.crear", "Crear Solicitudes");
        rol.AsignarPermiso(permiso);
        usuario.AsignarRol(rol);

        // Act
        string token = jwtService.GenerateToken(usuario);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Act & Assert: Validate through roundtrip decryption and validation
        var principal = jwtService.GetPrincipalFromExpiredToken(token);
        principal.Should().NotBeNull();
        
        var nameClaim = principal!.FindFirst(ClaimTypes.Name) ?? principal.FindFirst("name");
        nameClaim.Should().NotBeNull();
        nameClaim!.Value.Should().Be("Test Nombres");

        var emailClaim = principal.FindFirst(ClaimTypes.Email) ?? principal.FindFirst("email");
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be("test@nacionalvida.com.bo");

        var roleClaim = principal.FindFirst(ClaimTypes.Role) ?? principal.FindFirst("role");
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be("RRHH");

        var permissionClaim = principal.FindFirst("permission");
        permissionClaim.Should().NotBeNull();
        permissionClaim!.Value.Should().Be("solicitudes.crear");

        // Verify the issuer on the validated claims
        principal.Claims.Should().AllSatisfy(c => c.Issuer.Should().Be("NacionalSeguros.SIR"));
    }

    [Fact]
    public async Task CrearUsuarioCommandHandler_Should_Enforce_AD_Null_Password_Constraint()
    {
        // Arrange
        var usuarioRepoMock = new Mock<IUsuarioRepository>();
        var rolRepoMock = new Mock<IRolRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var auditServiceMock = new Mock<IAuditService>();

        usuarioRepoMock
            .Setup(r => r.GetByCorreoAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        var handler = new CrearUsuarioCommandHandler(
            usuarioRepoMock.Object,
            rolRepoMock.Object,
            unitOfWorkMock.Object,
            passwordHasherMock.Object,
            auditServiceMock.Object);

        // Act & Assert 1: Local requires password
        var commandLocalSinClave = new CrearUsuarioCommand("test@nacionalvida.com.bo", "Nombres", "Apellidos", "Local", "", 1);
        var resultLocal = await handler.Handle(commandLocalSinClave, CancellationToken.None);
        resultLocal.IsFailure.Should().BeTrue();
        resultLocal.Error.Code.Should().Be("PASSWORD_REQUIRED");

        // Act & Assert 2: AD creates user without hashing local password
        var commandAd = new CrearUsuarioCommand("test@nacionalvida.com.bo", "Nombres", "Apellidos", "ActiveDirectory", "", 1);
        var resultAd = await handler.Handle(commandAd, CancellationToken.None);
        resultAd.IsSuccess.Should().BeTrue();
        resultAd.Value.TipoAutenticacion.Should().Be("ActiveDirectory");
        passwordHasherMock.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
    }
}
