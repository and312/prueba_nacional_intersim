using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.BuscarCatalogos;

public class BuscarCatalogosQueryHandler : IRequestHandler<BuscarCatalogosQuery, Result<IEnumerable<CatalogoResponse>>>
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IMapper _mapper;

    public BuscarCatalogosQueryHandler(ICatalogoRepository catalogoRepository, IMapper mapper)
    {
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<CatalogoResponse>>> Handle(BuscarCatalogosQuery request, CancellationToken cancellationToken)
    {
        var catalogos = await _catalogoRepository.SearchAsync(request.SearchTerm);
        var dtos = _mapper.Map<IEnumerable<CatalogoResponse>>(catalogos);
        return Result.Success(dtos);
    }
}
