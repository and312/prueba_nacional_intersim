using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IParametroRepository
{
    Task<Parametro?> GetByIdAsync(int id);
    Task<Parametro?> GetByCodigoAsync(int catalogoId, string codigo);
    Task<IEnumerable<Parametro>> GetByCatalogoIdAsync(int catalogoId);
    Task<IEnumerable<Parametro>> GetByCatalogoCodigoAsync(string catalogoCodigo);
    Task AddAsync(Parametro parametro);
    void Update(Parametro parametro);
    Task<bool> HasCircularDependencyAsync(int selfId, int? parentId);
    Task<bool> IsInUseAsync(int id);
}
