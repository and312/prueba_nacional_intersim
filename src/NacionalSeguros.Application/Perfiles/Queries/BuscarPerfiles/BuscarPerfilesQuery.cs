using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries.BuscarPerfiles;

public record BuscarPerfilesQuery(string? Term) : IRequest<Result<IEnumerable<PerfilResponseDto>>>;
