using FluentValidation;

namespace NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;

public class CrearPerfilCargoCommandValidator : AbstractValidator<CrearPerfilCargoCommand>
{
    public CrearPerfilCargoCommandValidator()
    {
        RuleFor(x => x.Dto.SolicitudId)
            .GreaterThan(0).WithMessage("El ID de solicitud debe ser mayor a 0.");

        RuleFor(x => x.Dto.PerfilEstructurado)
            .NotNull().WithMessage("El perfil estructurado no puede ser nulo.");

        RuleFor(x => x.Dto.EstadoGeneracion)
            .NotEmpty().WithMessage("El estado de generación es obligatorio.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El usuario creador es obligatorio.");
    }
}
