using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.ListSolicitudesKanban;

public record ListSolicitudesKanbanQuery(
    int PageNumber,
    int PageSize,
    int? EstadoId,
    string? Search,
    string UserEmail,
    bool SoloMisSolicitudes = false,
    int? SolicitanteIdExplicit = null) : IRequest<Result<PagedSolicitudesResponseDto>>;
