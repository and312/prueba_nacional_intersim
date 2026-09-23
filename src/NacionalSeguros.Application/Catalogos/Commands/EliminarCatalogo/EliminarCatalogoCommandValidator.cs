using FluentValidation;

namespace NacionalSeguros.Application.Catalogos.Commands.EliminarCatalogo;

public class EliminarCatalogoCommandValidator : AbstractValidator<EliminarCatalogoCommand>
{
    public EliminarCatalogoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del catálogo debe ser mayor a cero.");

        RuleFor(x => x.DeletedBy)
            .NotEmpty().WithMessage("El usuario eliminador es requerido.");
    }
}
