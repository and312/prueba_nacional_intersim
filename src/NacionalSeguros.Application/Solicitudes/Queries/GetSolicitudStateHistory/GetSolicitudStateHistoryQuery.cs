using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudStateHistory;

public record GetSolicitudStateHistoryQuery(int SolicitudId) : IRequest<Result<List<StateHistoryResponseDto>>>;
