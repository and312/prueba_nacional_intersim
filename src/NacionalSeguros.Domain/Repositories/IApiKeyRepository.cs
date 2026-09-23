using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByIdAsync(long id);
    Task<ApiKey?> GetByHashAsync(string apiKeyHash);
    Task AddAsync(ApiKey apiKey);
    void Update(ApiKey apiKey);
    Task<IEnumerable<ApiKey>> ListAsync();
}
