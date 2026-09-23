using FluentValidation;

namespace NacionalSeguros.Application.Solicitudes.Commands.TransitarSolicitud;

public class TransitarSolicitudEstadoCommandValidator : AbstractValidator<TransitarSolicitudEstadoCommand>
{
    public TransitarSolicitudEstadoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la solicitud debe ser mayor a 0.");

        RuleFor(x => x.NuevoEstadoCodigo)
            .NotEmpty().WithMessage("El código del nuevo estado es obligatorio.");

        RuleFor(x => x.Comentario)
            .NotEmpty()
            .When(x => x.NuevoEstadoCodigo == "SOL-OBS")
            .WithMessage("La observación de RRHH es obligatoria.");

        RuleFor(x => x.Comentario)
            .NotEmpty()
            .When(x => x.NuevoEstadoCodigo == "SOL-RECH")
            .WithMessage("La justificación de rechazo es obligatoria.")
            .MinimumLength(10)
            .When(x => x.NuevoEstadoCodigo == "SOL-RECH")
            .WithMessage("La justificación de rechazo debe tener al menos 10 caracteres.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo del modificador es inválido.");
    }
}
