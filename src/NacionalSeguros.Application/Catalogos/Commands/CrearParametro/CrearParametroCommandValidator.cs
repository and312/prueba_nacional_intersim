using FluentValidation;

namespace NacionalSeguros.Application.Catalogos.Commands.CrearParametro;

public class CrearParametroCommandValidator : AbstractValidator<CrearParametroCommand>
{
    public CrearParametroCommandValidator()
    {
        RuleFor(x => x.CatalogoId)
            .GreaterThan(0).WithMessage("El ID del catálogo debe ser mayor a cero.");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código del parámetro no puede estar vacío.")
            .MaximumLength(50).WithMessage("El código del parámetro no puede exceder los 50 caracteres.");

        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor del parámetro no puede estar vacío.")
            .MaximumLength(250).WithMessage("El valor del parámetro no puede exceder los 250 caracteres.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El usuario creador es requerido.");
    }
}
