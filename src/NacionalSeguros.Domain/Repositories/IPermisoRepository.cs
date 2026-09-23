using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IPermisoRepository
{
    Task<List<Permiso>> ListAsync();
    Task<List<Permiso>> GetByUsuarioIdAsync(int usuarioId);
}
