using System.Collections.Generic;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerSlas;

public record ObtenerSlasQuery() : IRequest<Result<IEnumerable<SlaResponse>>>;
