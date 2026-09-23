using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ISlaRepository
{
    Task<Sla?> GetByIdAsync(int id);
    Task<IEnumerable<Sla>> GetAllAsync();
}
