using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IGerenciaRepository
{
    Task<Gerencia?> GetByIdAsync(int id);
    Task<IEnumerable<Gerencia>> GetAllActiveAsync();
    Task AddAsync(Gerencia gerencia);
}
