using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class GerenciaRepository : IGerenciaRepository
{
    private readonly ApplicationDbContext _context;

    public GerenciaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Gerencia?> GetByIdAsync(int id)
    {
        return await _context.Set<Gerencia>()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Gerencia>> GetAllActiveAsync()
    {
        return await _context.Set<Gerencia>()
            .ToListAsync();
    }

    public async Task AddAsync(Gerencia gerencia)
    {
        await _context.Set<Gerencia>().AddAsync(gerencia);
    }
}
