using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerCatalogos;

public class ObtenerCatalogosQueryHandler : IRequestHandler<ObtenerCatalogosQuery, Result<IEnumerable<CatalogoResponse>>>
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IMapper _mapper;

    public ObtenerCatalogosQueryHandler(ICatalogoRepository catalogoRepository, IMapper mapper)
    {
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<CatalogoResponse>>> Handle(ObtenerCatalogosQuery request, CancellationToken cancellationToken)
    {
        var catalogos = await _catalogoRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<CatalogoResponse>>(catalogos);
        return Result.Success(dtos);
    }
}
