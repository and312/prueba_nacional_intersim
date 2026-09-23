using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class PermisoRepository : IPermisoRepository
{
    private readonly ApplicationDbContext _context;

    public PermisoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permiso>> ListAsync()
    {
        return await _context.Set<Permiso>().ToListAsync();
    }

    public async Task<List<Permiso>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _context.Set<Usuario>()
            .Where(u => u.Id == usuarioId && !u.IsDeleted)
            .SelectMany(u => u.Roles)
            .Where(r => !r.IsDeleted)
            .SelectMany(r => r.Permisos)
            .Where(p => !p.IsDeleted)
            .Distinct()
            .ToListAsync();
    }
}
