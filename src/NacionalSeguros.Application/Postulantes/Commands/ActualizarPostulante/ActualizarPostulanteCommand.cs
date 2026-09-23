using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Commands.ActualizarPostulante;

public record ActualizarPostulanteCommand(
    int PostulanteId,
    string Nombres,
    string Apellidos,
    string DocumentoIdentidad,
    string ModifiedBy) : IRequest<Result>;
