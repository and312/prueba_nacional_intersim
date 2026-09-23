using FluentValidation;

namespace NacionalSeguros.Application.Security.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");

        RuleFor(x => x.Clave)
            .NotEmpty().WithMessage("La contraseña es requerida.");

        RuleFor(x => x.TipoAutenticacion)
            .NotEmpty().WithMessage("El tipo de autenticación es requerido.")
            .Must(x => x == "Local" || x == "ActiveDirectory")
            .WithMessage("El tipo de autenticación debe ser Local o ActiveDirectory.");
    }
}
