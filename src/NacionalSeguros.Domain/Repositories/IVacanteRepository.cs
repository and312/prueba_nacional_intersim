using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IVacanteRepository
{
    Task<Vacante?> GetByIdAsync(int id);
    Task<Estado?> GetEstadoByCodigoAsync(string codigo);
    Task AddAsync(Vacante vacante);
    void Update(Vacante vacante);
    Task<IEnumerable<Vacante>> ListAsync();
}
