using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.ReanudarVacante;

public record ReanudarVacanteCommand(
    int VacanteId,
    string Justificacion,
    string ModifiedBy) : IRequest<Result>;
