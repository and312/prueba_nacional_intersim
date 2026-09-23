using FluentValidation;

namespace NacionalSeguros.Application.Catalogos.Commands.CrearCatalogo;

public class CrearCatalogoCommandValidator : AbstractValidator<CrearCatalogoCommand>
{
    public CrearCatalogoCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del catálogo no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre del catálogo no puede exceder los 100 caracteres.");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código del catálogo no puede estar vacío.")
            .MaximumLength(50).WithMessage("El código del catálogo no puede exceder los 50 caracteres.")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("El código del catálogo debe ser alfanumérico en mayúsculas, números, guiones o guiones bajos (ej. CAT-MOD).");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El usuario creador es requerido.");
    }
}
