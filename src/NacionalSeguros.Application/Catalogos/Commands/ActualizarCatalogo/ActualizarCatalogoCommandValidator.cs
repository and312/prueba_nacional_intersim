using FluentValidation;

namespace NacionalSeguros.Application.Catalogos.Commands.ActualizarCatalogo;

public class ActualizarCatalogoCommandValidator : AbstractValidator<ActualizarCatalogoCommand>
{
    public ActualizarCatalogoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del catálogo debe ser mayor a cero.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del catálogo no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre del catálogo no puede exceder los 100 caracteres.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es requerido.");
    }
}
