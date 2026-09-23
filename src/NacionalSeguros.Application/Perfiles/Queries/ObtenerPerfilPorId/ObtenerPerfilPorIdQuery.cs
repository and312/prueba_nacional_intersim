using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfilPorId;

public record ObtenerPerfilPorIdQuery(int PerfilCargoId) : IRequest<Result<PerfilResponseDto>>;
