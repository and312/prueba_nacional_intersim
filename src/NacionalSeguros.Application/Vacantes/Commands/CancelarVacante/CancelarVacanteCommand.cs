using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.CancelarVacante;

public record CancelarVacanteCommand(
    int VacanteId,
    string MotivoCancelacionCodigo,
    string ModifiedBy) : IRequest<Result>;
