using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.ObtenerPermisos;

public class ObtenerPermisosQueryHandler : IRequestHandler<ObtenerPermisosQuery, Result<List<string>>>
{
    private readonly IPermisoRepository _permisoRepository;

    public ObtenerPermisosQueryHandler(IPermisoRepository permisoRepository)
    {
        _permisoRepository = permisoRepository;
    }

    public async Task<Result<List<string>>> Handle(ObtenerPermisosQuery request, CancellationToken cancellationToken)
    {
        var permisos = await _permisoRepository.ListAsync();
        var codigos = permisos.Select(p => p.Codigo).ToList();
        return Result.Success(codigos);
    }
}
