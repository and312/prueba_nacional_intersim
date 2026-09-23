using FluentValidation;

namespace NacionalSeguros.Application.Catalogos.Commands.EliminarParametro;

public class EliminarParametroCommandValidator : AbstractValidator<EliminarParametroCommand>
{
    public EliminarParametroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del parámetro debe ser mayor a cero.");

        RuleFor(x => x.DeletedBy)
            .NotEmpty().WithMessage("El usuario eliminador es requerido.");
    }
}
