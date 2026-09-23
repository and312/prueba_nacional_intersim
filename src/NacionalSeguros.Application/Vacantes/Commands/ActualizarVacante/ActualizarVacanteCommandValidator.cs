using FluentValidation;

namespace NacionalSeguros.Application.Vacantes.Commands.ActualizarVacante;

public class ActualizarVacanteCommandValidator : AbstractValidator<ActualizarVacanteCommand>
{
    public ActualizarVacanteCommandValidator()
    {
        RuleFor(x => x.VacanteId)
            .GreaterThan(0).WithMessage("El ID de la vacante debe ser mayor a 0.");

        RuleFor(x => x.BandaSalarialMin)
            .GreaterThanOrEqualTo(0).WithMessage("La banda salarial mínima no puede ser negativa.");

        RuleFor(x => x.BandaSalarialMax)
            .GreaterThanOrEqualTo(x => x.BandaSalarialMin)
            .WithMessage("La banda salarial máxima no puede ser menor que la mínima.");
    }
}
