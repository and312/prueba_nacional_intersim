using FluentValidation;

namespace NacionalSeguros.Application.Security.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.TokenExpirado)
            .NotEmpty().WithMessage("El token de acceso expirado es requerido.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es requerido.");
    }
}
