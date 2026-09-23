using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IRolRepository
{
    Task<Rol?> GetByIdAsync(int id);
    Task<List<Rol>> ListAsync();
    Task AddAsync(Rol rol);
}
