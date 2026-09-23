using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class TipoObservacionRepository : ITipoObservacionRepository
{
    private readonly ApplicationDbContext _context;

    public TipoObservacionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TipoObservacion?> GetByIdAsync(int id)
    {
        return await _context.Set<TipoObservacion>().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TipoObservacion?> GetByCodigoAsync(string codigo)
    {
        return await _context.Set<TipoObservacion>().FirstOrDefaultAsync(t => t.Codigo == codigo);
    }

    public async Task<IEnumerable<TipoObservacion>> GetAllAsync()
    {
        return await _context.Set<TipoObservacion>().ToListAsync();
    }

    public async Task AddAsync(TipoObservacion entity)
    {
        await _context.Set<TipoObservacion>().AddAsync(entity);
    }

    public void Update(TipoObservacion entity)
    {
        _context.Set<TipoObservacion>().Update(entity);
    }
}
