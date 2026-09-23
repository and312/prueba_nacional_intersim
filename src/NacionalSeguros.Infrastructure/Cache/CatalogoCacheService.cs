using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NacionalSeguros.Application.Abstractions.Cache;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Infrastructure.Cache;

public class CatalogoCacheService : ICatalogoCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IParametroRepository _parametroRepository;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    public CatalogoCacheService(IMemoryCache cache, IParametroRepository parametroRepository)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _parametroRepository = parametroRepository ?? throw new ArgumentNullException(nameof(parametroRepository));
    }

    public async Task<IEnumerable<ParametroResponse>> GetCachedParametersAsync(string catalogoCodigo)
    {
        var cacheKey = $"catalog:{catalogoCodigo}";
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<ParametroResponse>? parameters) || parameters == null)
        {
            var dbParameters = await _parametroRepository.GetByCatalogoCodigoAsync(catalogoCodigo);
            parameters = dbParameters.Select(p => new ParametroResponse(
                p.Id,
                p.CatalogoId,
                p.Codigo,
                p.Valor,
                p.ParametroIdPadre,
                p.CreatedBy,
                p.CreatedDate
            )).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration);

            _cache.Set(cacheKey, parameters, cacheOptions);
        }

        return parameters;
    }

    public Task InvalidateCacheAsync(string catalogoCodigo)
    {
        var cacheKey = $"catalog:{catalogoCodigo}";
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }
}
