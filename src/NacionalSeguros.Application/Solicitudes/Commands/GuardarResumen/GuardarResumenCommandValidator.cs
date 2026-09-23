using FluentValidation;

namespace NacionalSeguros.Application.Solicitudes.Commands.GuardarResumen;

public class GuardarResumenCommandValidator : AbstractValidator<GuardarResumenCommand>
{
    public GuardarResumenCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la solicitud debe ser mayor a 0.");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("El cuerpo de la petición no puede ser nulo.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.ProfileSummary)
                .NotEmpty().WithMessage("El resumen narrativo del perfil (profileSummary) es obligatorio.");

            RuleFor(x => x.Dto.CaptureState)
                .NotEmpty().WithMessage("El estado de captura (captureState) es obligatorio.")
                .Must(state => state == "PARCIAL" || state == "COMPLETO")
                .WithMessage("El estado de captura debe ser 'PARCIAL' o 'COMPLETO'.");

            RuleFor(x => x.Dto.CaptureConfidence)
                .NotNull().WithMessage("La confianza de captura (captureConfidence) es obligatoria.")
                .Must(conf => conf >= 0.0m && conf <= 1.0m).WithMessage("La confianza de captura debe estar entre 0.0 y 1.0.");

            RuleFor(x => x.Dto.Recommendation)
                .NotEmpty().WithMessage("La recomendación (recommendation) es obligatoria.")
                .Must(rec => rec == "APROBAR_REVISION" || rec == "OBSERVAR" || rec == "RECHAZAR_DATOS_INSUFICIENTES")
                .WithMessage("La recomendación debe ser 'APROBAR_REVISION', 'OBSERVAR' o 'RECHAZAR_DATOS_INSUFICIENTES'.");

            RuleFor(x => x.Dto.AgentName)
                .NotEmpty().WithMessage("El nombre del agente (agenteName) es obligatorio.");

            RuleFor(x => x.Dto.AgentVersion)
                .NotEmpty().WithMessage("La versión del agente (agenteVersion) es obligatoria.");

        });
    }
}
