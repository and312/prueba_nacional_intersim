using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Usuarios;

public record ObtenerUsuarioPorIdQuery(int UsuarioId) : IRequest<Result<UsuarioResponseDto>>;
