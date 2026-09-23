using FluentValidation;

namespace NacionalSeguros.Application.Security.Commands.CambiarPassword;

public class CambiarPasswordCommandValidator : AbstractValidator<CambiarPasswordCommand>
{
    public CambiarPasswordCommandValidator()
    {
        RuleFor(x => x.ClaveActual)
            .NotEmpty().WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.NuevaClave)
            .NotEmpty().WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.");
    }
}
