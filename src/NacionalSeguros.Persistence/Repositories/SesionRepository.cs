using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class SesionRepository : ISesionRepository
{
    private readonly ApplicationDbContext _context;

    public SesionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Sesion?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Set<Sesion>()
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
    }

    public async Task AddAsync(Sesion sesion)
    {
        await _context.Set<Sesion>().AddAsync(sesion);
    }

    public void Update(Sesion sesion)
    {
        _context.Set<Sesion>().Update(sesion);
    }

    public async Task<List<Sesion>> GetActiveSessionsByUsuarioIdAsync(int usuarioId)
    {
        return await _context.Set<Sesion>()
            .Where(s => s.UsuarioId == usuarioId && s.Activa)
            .ToListAsync();
    }
}
