using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IHistorialApiKeyRepository
{
    Task AddAsync(HistorialApiKey historial);
    Task<IEnumerable<HistorialApiKey>> GetByApiKeyIdAsync(long apiKeyId);
}
