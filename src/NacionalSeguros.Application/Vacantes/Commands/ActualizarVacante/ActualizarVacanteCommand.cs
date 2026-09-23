using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Commands.ActualizarVacante;

public record ActualizarVacanteCommand(
    int VacanteId,
    decimal BandaSalarialMin,
    decimal BandaSalarialMax,
    string ModifiedBy) : IRequest<Result>;
