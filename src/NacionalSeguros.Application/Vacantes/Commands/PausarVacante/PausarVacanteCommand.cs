using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.PausarVacante;

public record PausarVacanteCommand(
    int VacanteId,
    string Justificacion,
    string ModifiedBy) : IRequest<Result>;
