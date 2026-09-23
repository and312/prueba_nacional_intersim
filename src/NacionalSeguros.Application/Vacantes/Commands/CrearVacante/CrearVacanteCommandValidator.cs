using System;
using FluentValidation;

namespace NacionalSeguros.Application.Vacantes.Commands.CrearVacante;

public class CrearVacanteCommandValidator : AbstractValidator<CrearVacanteCommand>
{
    public CrearVacanteCommandValidator()
    {
        RuleFor(x => x.Dto.SolicitudId)
            .GreaterThan(0).WithMessage("El ID de la solicitud debe ser mayor a 0.");

        RuleFor(x => x.Dto.PerfilId)
            .GreaterThan(0).WithMessage("El ID del perfil debe ser mayor a 0.");

        RuleFor(x => x.Dto.FechaLimiteCobertura)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("La fecha límite de cobertura debe ser una fecha futura.");
    }
}
