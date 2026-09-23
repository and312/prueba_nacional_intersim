using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.CerrarVacante;

public record CerrarVacanteCommand(
    int VacanteId,
    int PostulanteContratadoId,
    string ModifiedBy) : IRequest<Result>;
