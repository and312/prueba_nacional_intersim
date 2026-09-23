using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.ObtenerUsuarioActual;

public record ObtenerUsuarioActualQuery(int UsuarioId) : IRequest<Result<UsuarioResponseDto>>;
