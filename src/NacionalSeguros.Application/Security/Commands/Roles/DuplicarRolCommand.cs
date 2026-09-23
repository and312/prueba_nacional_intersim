using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Commands.Roles;

public record DuplicarRolCommand(int RolId, string NuevoNombre) : IRequest<Result<RolResponseDto>>;
