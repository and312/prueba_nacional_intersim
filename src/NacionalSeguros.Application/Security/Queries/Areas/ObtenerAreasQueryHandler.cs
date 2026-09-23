using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Security.Queries.Areas;

public class ObtenerAreasQueryHandler : IRequestHandler<ObtenerAreasQuery, Result<List<AreaResponseDto>>>
{
    private readonly IAreaRepository _areaRepository;
    private readonly IMapper _mapper;

    public ObtenerAreasQueryHandler(IAreaRepository areaRepository, IMapper mapper)
    {
        _areaRepository = areaRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<AreaResponseDto>>> Handle(ObtenerAreasQuery request, CancellationToken cancellationToken)
    {
        var areas = await _areaRepository.GetAllAsync();
        var dtos = _mapper.Map<List<AreaResponseDto>>(areas);
        return Result.Success(dtos);
    }
}
