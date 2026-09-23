using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Catalogos.Queries.ObtenerDetalles;

public class ObtenerDetallesQueryHandler : IRequestHandler<ObtenerDetallesQuery, Result<IEnumerable<ParametroResponse>>>
{
    private readonly IParametroRepository _parametroRepository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly ICatalogoCacheService _cacheService;
    private readonly IMapper _mapper;

    public ObtenerDetallesQueryHandler(
        IParametroRepository parametroRepository,
        ICatalogoRepository catalogoRepository,
        ICatalogoCacheService cacheService,
        IMapper mapper)
    {
        _parametroRepository = parametroRepository ?? throw new ArgumentNullException(nameof(parametroRepository));
        _catalogoRepository = catalogoRepository ?? throw new ArgumentNullException(nameof(catalogoRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<IEnumerable<ParametroResponse>>> Handle(ObtenerDetallesQuery request, CancellationToken cancellationToken)
    {
        if (request.CatalogoId.HasValue)
        {
            var parameters = await _parametroRepository.GetByCatalogoIdAsync(request.CatalogoId.Value);
            var dtos = _mapper.Map<IEnumerable<ParametroResponse>>(parameters.Where(p => !p.IsDeleted));
            return Result.Success(dtos);
        }

        if (!string.IsNullOrWhiteSpace(request.CatalogoCodigo))
        {
            // Consultar a través del servicio de caché
            var cachedParameters = await _cacheService.GetCachedParametersAsync(request.CatalogoCodigo);
            return Result.Success(cachedParameters);
        }

        return Result.Failure<IEnumerable<ParametroResponse>>(
            new Error("Parametro.InvalidQuery", "Debe proporcionar CatalogoId o CatalogoCodigo para consultar los detalles."));
    }
}
