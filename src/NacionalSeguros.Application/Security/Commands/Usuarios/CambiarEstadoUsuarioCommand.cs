using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public record CambiarEstadoUsuarioCommand(int UsuarioId, string NuevoEstado) : IRequest<Result>;
