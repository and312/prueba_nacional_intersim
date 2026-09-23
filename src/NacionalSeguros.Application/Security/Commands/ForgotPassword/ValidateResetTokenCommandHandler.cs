using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.ForgotPassword;

public class ValidateResetTokenCommandHandler : IRequestHandler<ValidateResetTokenCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ValidateResetTokenCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result> Handle(ValidateResetTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure(new Error("TOKEN_REQUIRED", "El token es obligatorio."));
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

        return Result.Success();
    }
}
