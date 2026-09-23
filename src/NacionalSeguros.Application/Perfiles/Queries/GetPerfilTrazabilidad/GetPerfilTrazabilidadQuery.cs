using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.GetPerfilTrazabilidad;

public record GetPerfilTrazabilidadQuery(int PerfilCargoId) : IRequest<Result<List<StateHistoryResponseDto>>>;
