using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ITipoObservacionRepository
{
    Task<TipoObservacion?> GetByIdAsync(int id);
    Task<TipoObservacion?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<TipoObservacion>> GetAllAsync();
    Task AddAsync(TipoObservacion entity);
    void Update(TipoObservacion entity);
}
