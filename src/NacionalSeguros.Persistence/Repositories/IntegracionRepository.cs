using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class IntegracionRepository : IIntegracionRepository
{
    private readonly ApplicationDbContext _context;

    public IntegracionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Integracion?> GetByIdAsync(int id)
    {
        return await _context.Set<Integracion>()
            .Include(i => i.ApiKeys)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Integracion?> GetByCodigoAsync(string codigo)
    {
        return await _context.Set<Integracion>()
            .Include(i => i.ApiKeys)
            .FirstOrDefaultAsync(i => i.Codigo == codigo);
    }

    public async Task AddAsync(Integracion integracion)
    {
        await _context.Set<Integracion>().AddAsync(integracion);
    }

    public void Update(Integracion integracion)
    {
        _context.Set<Integracion>().Update(integracion);
    }

    public async Task<IEnumerable<Integracion>> ListAsync()
    {
        return await _context.Set<Integracion>()
            .Include(i => i.ApiKeys)
            .ToListAsync();
    }
}
