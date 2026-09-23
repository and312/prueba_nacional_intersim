using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Commands.RegistrarPostulante;

public record RegistrarPostulanteCommand(
    int VacanteId,
    string Correo,
    byte[] CvContent,
    string CvFileName,
    string CreatedBy) : IRequest<Result<int>>;
