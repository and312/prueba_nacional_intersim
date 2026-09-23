using FluentValidation;

namespace NacionalSeguros.Application.PerfilesEstructurados.Commands.ActualizarPerfilEstructurado;

public class ActualizarPerfilEstructuradoCommandValidator : AbstractValidator<ActualizarPerfilEstructuradoCommand>
{
    public ActualizarPerfilEstructuradoCommandValidator()
    {
        RuleFor(x => x.Dto.PerfilEstructurado)
            .NotNull().WithMessage("El perfil estructurado no puede ser nulo.");

        RuleFor(x => x.Dto.EstadoGeneracion)
            .NotEmpty().WithMessage("El estado de generación es obligatorio.");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El usuario modificador es obligatorio.");
    }
}
