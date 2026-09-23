using FluentValidation;

namespace NacionalSeguros.Application.PerfilesEstructurados.Commands.CrearPerfilEstructurado;

public class CrearPerfilEstructuradoCommandValidator : AbstractValidator<CrearPerfilEstructuradoCommand>
{
    public CrearPerfilEstructuradoCommandValidator()
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
