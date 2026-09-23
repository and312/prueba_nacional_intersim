using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class RolRepository : IRolRepository
{
    private readonly ApplicationDbContext _context;

    public RolRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Rol?> GetByIdAsync(int id)
    {
        return await _context.Set<Rol>()
            .Include(r => r.Permisos)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Rol>> ListAsync()
    {
        return await _context.Set<Rol>()
            .Include(r => r.Permisos)
            .ToListAsync();
    }

    public async Task AddAsync(Rol rol)
    {
        await _context.Set<Rol>().AddAsync(rol);
    }
}
