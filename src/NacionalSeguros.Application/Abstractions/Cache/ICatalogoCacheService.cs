using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Contracts.Catalogos;

namespace NacionalSeguros.Application.Abstractions.Cache;

public interface ICatalogoCacheService
{
    Task<IEnumerable<ParametroResponse>> GetCachedParametersAsync(string catalogoCodigo);
    Task InvalidateCacheAsync(string catalogoCodigo);
}
