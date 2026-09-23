using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudById;

public record GetSolicitudByIdQuery(int Id, string UserEmail) : IRequest<Result<SolicitudResponseDto>>;
