using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ICatalogoRepository
{
    Task<Catalogo?> GetByIdAsync(int id);
    Task<Catalogo?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Catalogo>> GetAllAsync();
    Task<IEnumerable<Catalogo>> SearchAsync(string searchTerm);
    Task AddAsync(Catalogo catalogo);
    void Update(Catalogo catalogo);
    Task<bool> IsInUseAsync(int id);
}
