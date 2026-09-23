using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Security;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ForgotPassword;

public class ResetPasswordConfirmCommandHandler : IRequestHandler<ResetPasswordConfirmCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public ResetPasswordConfirmCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(ResetPasswordConfirmCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure(new Error("TOKEN_REQUIRED", "El token es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(request.NuevaClave))
        {
            return Result.Failure(new Error("PASSWORD_REQUIRED", "La nueva contraseña es obligatoria."));
        }

        var usuario = await _usuarioRepository.GetByIdAsync(request.UserId);
        if (usuario == null || usuario.IsDeleted)
        {
            return Result.Failure(new Error("USER_NOT_FOUND", "Usuario no encontrado."));
        }

        if (string.IsNullOrEmpty(usuario.ResetPasswordToken) || usuario.ResetPasswordToken != request.Token)
        {
            return Result.Failure(new Error("INVALID_TOKEN", "El enlace de recuperación es inválido."));
        }

        if (usuario.ResetPasswordTokenExpiration == null || usuario.ResetPasswordTokenExpiration < DateTime.UtcNow)
        {
            return Result.Failure(new Error("EXPIRED_TOKEN", "El enlace de recuperación ha expirado. Por favor, solicite uno nuevo."));
        }

        // Hashear y restablecer clave
        var claveHash = _passwordHasher.HashPassword(request.NuevaClave);
        usuario.ResetPassword(claveHash);

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auditar el restablecimiento de contraseña
        await _auditService.LogActionAsync(
            usuarioId: usuario.Id,
            usuarioNombre: usuario.Correo,
            rol: "Usuario",
            modulo: "Seguridad",
            entidad: "Usuarios",
            entidadId: usuario.Id,
            accion: "Restablecer contraseña por flujo de olvido",
            estadoAnterior: "Token activo",
            estadoNuevo: "Contraseña cambiada, token removido",
            canal: "API",
            correlationId: null);

        return Result.Success();
    }
}
