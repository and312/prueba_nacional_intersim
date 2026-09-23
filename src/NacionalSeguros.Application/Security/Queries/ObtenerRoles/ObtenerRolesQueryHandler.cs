using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.ObtenerRoles;

public class ObtenerRolesQueryHandler : IRequestHandler<ObtenerRolesQuery, Result<List<RolResponseDto>>>
{
    private readonly IRolRepository _rolRepository;

    public ObtenerRolesQueryHandler(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
    }

    public async Task<Result<List<RolResponseDto>>> Handle(ObtenerRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _rolRepository.ListAsync();
        var dtos = roles.Select(r => new RolResponseDto
        {
            RolId = r.Id,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion
        }).ToList();

        return Result.Success(dtos);
    }
}
