using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ISesionRepository
{
    Task<Sesion?> GetByRefreshTokenAsync(string refreshToken);
    Task AddAsync(Sesion sesion);
    void Update(Sesion sesion);
    Task<List<Sesion>> GetActiveSessionsByUsuarioIdAsync(int usuarioId);
}
