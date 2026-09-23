using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Vacantes.Queries.BuscarVacantes;

public class BuscarVacantesQueryHandler : IRequestHandler<BuscarVacantesQuery, Result<PagedVacantesResponseDto>>
{
    private readonly IVacanteRepository _vacanteRepository;
    private readonly IMapper _mapper;

    public BuscarVacantesQueryHandler(IVacanteRepository vacanteRepository, IMapper mapper)
    {
        _vacanteRepository = vacanteRepository ?? throw new ArgumentNullException(nameof(vacanteRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PagedVacantesResponseDto>> Handle(BuscarVacantesQuery request, CancellationToken cancellationToken)
    {
        var vacantes = await _vacanteRepository.ListAsync();

        int totalCount = vacantes.Count();
        
        // Aplicar paginación básica en memoria
        var paginated = vacantes
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<VacanteResponseDto>>(paginated);

        var pagedResult = new PagedVacantesResponseDto(dtos, totalCount);
        return Result.Success(pagedResult);
    }
}
