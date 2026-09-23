using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Usuarios;

public record ListarUsuariosQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    int? AreaId = null,
    int? RolId = null,
    string? Estado = null,
    string? Cargo = null,
    string? Gerencia = null) : IRequest<Result<PagedUsuariosResponseDto>>;
