using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public record EliminarUsuarioCommand(int UsuarioId) : IRequest<Result>;
