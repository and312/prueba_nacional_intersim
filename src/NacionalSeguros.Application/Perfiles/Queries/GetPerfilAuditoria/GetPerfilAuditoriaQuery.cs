using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.GetPerfilAuditoria;

public record GetPerfilAuditoriaQuery(int PerfilCargoId) : IRequest<Result<IEnumerable<PerfilAuditoriaResponseDto>>>;
