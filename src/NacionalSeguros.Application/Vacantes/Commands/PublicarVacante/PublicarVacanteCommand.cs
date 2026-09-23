using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.PublicarVacante;

public record PublicarVacanteCommand(
    int VacanteId,
    IEnumerable<int> CanalIds,
    string ModifiedBy) : IRequest<Result>;
