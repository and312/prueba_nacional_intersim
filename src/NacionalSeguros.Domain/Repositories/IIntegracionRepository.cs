using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IIntegracionRepository
{
    Task<Integracion?> GetByIdAsync(int id);
    Task<Integracion?> GetByCodigoAsync(string codigo);
    Task AddAsync(Integracion integracion);
    void Update(Integracion integracion);
    Task<IEnumerable<Integracion>> ListAsync();
}
