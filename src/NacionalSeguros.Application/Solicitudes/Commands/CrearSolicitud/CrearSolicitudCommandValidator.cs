using FluentValidation;

namespace NacionalSeguros.Application.Solicitudes.Commands.CrearSolicitud;

public class CrearSolicitudCommandValidator : AbstractValidator<CrearSolicitudCommand>
{
    public CrearSolicitudCommandValidator()
    {
        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El correo del solicitante es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo del solicitante es inválido.");

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
