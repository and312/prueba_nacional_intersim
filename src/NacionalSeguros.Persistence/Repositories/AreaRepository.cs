using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class AreaRepository : IAreaRepository
{
    private readonly ApplicationDbContext _context;

    public AreaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Area?> GetByIdAsync(int id)
    {
        return await _context.Set<Area>()
            .Include(a => a.Gerencia)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Area?> GetByCodigoAsync(string codigo)
    {
        return await _context.Set<Area>()
            .Include(a => a.Gerencia)
            .FirstOrDefaultAsync(a => a.Codigo == codigo);
    }

    public async Task<IEnumerable<Area>> GetAllAsync()
    {
        return await _context.Set<Area>()
            .Include(a => a.Gerencia)
            .ToListAsync();
    }

    public async Task AddAsync(Area area)
    {
        await _context.Set<Area>().AddAsync(area);
    }

    public void Update(Area area)
    {
        _context.Set<Area>().Update(area);
    }

    public async Task<bool> HasUsersAsync(int areaId)
    {
        return await _context.Set<Usuario>()
            .AnyAsync(u => u.AreaId == areaId && !u.IsDeleted);
    }
}
