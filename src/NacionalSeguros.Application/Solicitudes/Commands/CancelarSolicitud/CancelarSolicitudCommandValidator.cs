using FluentValidation;

namespace NacionalSeguros.Application.Solicitudes.Commands.CancelarSolicitud;

public class CancelarSolicitudCommandValidator : AbstractValidator<CancelarSolicitudCommand>
{
    public CancelarSolicitudCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la solicitud debe ser mayor a 0.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo de la cancelación es obligatorio.")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo del modificador es inválido.");
    }
}
