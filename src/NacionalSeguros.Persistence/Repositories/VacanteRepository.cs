using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class VacanteRepository : IVacanteRepository
{
    private readonly ApplicationDbContext _context;

    public VacanteRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Vacante?> GetByIdAsync(int id)
    {
        return await _context.Set<Vacante>()
            .Include(v => v.Estado)
            .Include(v => v.Solicitud)
            .Include(v => v.PerfilCargo)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Estado?> GetEstadoByCodigoAsync(string codigo)
    {
        return await _context.Set<Estado>()
            .FirstOrDefaultAsync(e => e.Codigo == codigo && e.Entidad == "Vacante");
    }

    public async Task AddAsync(Vacante vacante)
    {
        await _context.Set<Vacante>().AddAsync(vacante);
    }

    public void Update(Vacante vacante)
    {
        _context.Set<Vacante>().Update(vacante);
    }

    public async Task<IEnumerable<Vacante>> ListAsync()
    {
        return await _context.Set<Vacante>()
            .Include(v => v.Estado)
            .Include(v => v.Solicitud)
            .Include(v => v.PerfilCargo)
            .AsNoTracking()
            .ToListAsync();
    }
}
