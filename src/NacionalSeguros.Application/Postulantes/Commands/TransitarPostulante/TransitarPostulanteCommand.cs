using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Commands.TransitarPostulante;

public record TransitarPostulanteCommand(
    int PostulanteId,
    int VacanteId,
    int NuevoEstadoId,
    string? MotivoDescarteCodigo,
    string? JustificacionText,
    string Changer) : IRequest<Result>;
