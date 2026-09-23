using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Gerencias;

public record ObtenerGerenciasQuery : IRequest<Result<List<GerenciaResponseDto>>>;
