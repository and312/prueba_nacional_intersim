using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfil;

public record ObtenerPerfilQuery(int SolicitudId) : IRequest<Result<PerfilResponseDto>>;
