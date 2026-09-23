using FluentValidation;

namespace NacionalSeguros.Application.Solicitudes.Commands.ActualizarSolicitud;

public class ActualizarSolicitudCommandValidator : AbstractValidator<ActualizarSolicitudCommand>
{
    public ActualizarSolicitudCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la solicitud debe ser mayor a 0.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo del modificador es inválido.");

        RuleFor(x => x.Dto.Cargo)
            .NotEmpty().WithMessage("El cargo es obligatorio.");

        RuleFor(x => x.Dto.Seniority)
            .NotEmpty().WithMessage("El seniority del cargo es obligatorio.");

        RuleFor(x => x.Dto.Prioridad)
            .NotEmpty().WithMessage("La prioridad es obligatoria.");

        RuleFor(x => x.Dto.Funciones)
            .NotEmpty().WithMessage("Las funciones del cargo son obligatorias.");

        RuleFor(x => x.Dto.CantidadVacantes)
            .Must(v => !v.HasValue || v.Value >= 1)
            .WithMessage("La cantidad de vacantes debe ser mayor o igual a 1.");
    }
}
