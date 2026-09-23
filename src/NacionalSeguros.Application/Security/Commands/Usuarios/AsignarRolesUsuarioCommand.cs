using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Usuarios;

public record AsignarRolesUsuarioCommand(
    int UsuarioId,
    List<int> RolIds) : IRequest<Result>;
