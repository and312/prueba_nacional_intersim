using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IApiKeyAuditoriaRepository
{
    Task AddAsync(ApiKeyAuditoria auditoria);
    Task<IEnumerable<ApiKeyAuditoria>> ListAsync();
}
