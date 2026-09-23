using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetWhatsAppCostCheck;

public record GetWhatsAppCostCheckQuery(int Id) : IRequest<Result<WhatsAppCostCheckResponseDto>>;
