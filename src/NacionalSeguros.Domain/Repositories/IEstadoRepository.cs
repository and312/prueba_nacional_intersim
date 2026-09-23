using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IEstadoRepository
{
    Task<Estado?> GetByIdAsync(int id);
    Task<Estado?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Estado>> GetByEntidadAsync(string entidad);
    Task<IEnumerable<Estado>> GetAllAsync();
}
