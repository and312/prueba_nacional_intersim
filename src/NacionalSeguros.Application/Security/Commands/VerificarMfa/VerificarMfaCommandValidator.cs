using FluentValidation;

namespace NacionalSeguros.Application.Security.Commands.VerificarMfa;

public class VerificarMfaCommandValidator : AbstractValidator<VerificarMfaCommand>
{
    public VerificarMfaCommandValidator()
    {
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");

        RuleFor(x => x.CodigoOtp)
            .NotEmpty().WithMessage("El código OTP es requerido.")
            .Length(6).WithMessage("El código OTP debe tener exactamente 6 caracteres.");
    }
}
