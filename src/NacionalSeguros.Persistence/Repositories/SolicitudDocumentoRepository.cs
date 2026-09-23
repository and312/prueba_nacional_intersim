using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class SolicitudDocumentoRepository : ISolicitudDocumentoRepository
{
    private readonly ApplicationDbContext _context;

    public SolicitudDocumentoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<SolicitudDocumento>> GetBySolicitudIdAsync(int solicitudId, string? tipoDocumento = null)
    {
        var query = _context.Set<SolicitudDocumento>()
            .Where(sd => sd.SolicitudId == solicitudId);

        if (!string.IsNullOrWhiteSpace(tipoDocumento))
        {
            query = query.Where(sd => sd.TipoDocumento == tipoDocumento);
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(SolicitudDocumento documento)
    {
        await _context.Set<SolicitudDocumento>().AddAsync(documento);
    }
}
