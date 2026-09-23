using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public record RestablecerPasswordCommand(int UsuarioId, string NuevaClave) : IRequest<Result>;
