using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
}
