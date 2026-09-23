using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class SolicitudResumenRepository : ISolicitudResumenRepository
{
    private readonly ApplicationDbContext _context;

    public SolicitudResumenRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SolicitudResumen?> GetBySolicitudIdAsync(int solicitudId)
    {
        return await _context.Set<SolicitudResumen>()
            .FirstOrDefaultAsync(sr => sr.SolicitudId == solicitudId);
    }

    public async Task AddAsync(SolicitudResumen resumen)
    {
        await _context.Set<SolicitudResumen>().AddAsync(resumen);
    }

    public void Update(SolicitudResumen resumen)
    {
        _context.Set<SolicitudResumen>().Update(resumen);
    }
}
