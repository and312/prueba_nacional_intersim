using FluentValidation;

namespace NacionalSeguros.Application.Catalogos.Commands.ActualizarParametro;

public class ActualizarParametroCommandValidator : AbstractValidator<ActualizarParametroCommand>
{
    public ActualizarParametroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del parámetro debe ser mayor a cero.");

        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor del parámetro no puede estar vacío.")
            .MaximumLength(250).WithMessage("El valor del parámetro no puede exceder los 250 caracteres.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es requerido.");
    }
}
