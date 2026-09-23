using FluentValidation;

namespace NacionalSeguros.Application.Perfiles.Commands.ActualizarPerfilCargo;

public class ActualizarPerfilCargoCommandValidator : AbstractValidator<ActualizarPerfilCargoCommand>
{
    public ActualizarPerfilCargoCommandValidator()
    {
        RuleFor(x => x.PerfilCargoId)
            .GreaterThan(0).WithMessage("El ID de perfil de cargo debe ser mayor a 0.");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El nombre del cargo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del cargo no debe exceder los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción del perfil es obligatoria.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es obligatorio.");
    }
}
