using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.GenerarPerfil;

public record GenerarPerfilCommand(int SolicitudId, string CreatedBy) : IRequest<Result>;
