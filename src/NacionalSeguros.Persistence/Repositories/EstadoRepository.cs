using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class EstadoRepository : IEstadoRepository
{
    private readonly ApplicationDbContext _context;

    public EstadoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Estado?> GetByIdAsync(int id)
    {
        return await _context.Estados.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Estado?> GetByCodigoAsync(string codigo)
    {
        return await _context.Estados.FirstOrDefaultAsync(e => e.Codigo == codigo);
    }

    public async Task<IEnumerable<Estado>> GetByEntidadAsync(string entidad)
    {
        return await _context.Estados
            .Where(e => e.Entidad == entidad)
            .ToListAsync();
    }

    public async Task<IEnumerable<Estado>> GetAllAsync()
    {
        return await _context.Estados.ToListAsync();
    }
}
