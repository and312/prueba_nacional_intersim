using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IAreaRepository
{
    Task<Area?> GetByIdAsync(int id);
    Task<Area?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Area>> GetAllAsync();
    Task AddAsync(Area area);
    void Update(Area area);
    Task<bool> HasUsersAsync(int areaId);
}
