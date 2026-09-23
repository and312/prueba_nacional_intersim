using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class HistorialApiKeyRepository : IHistorialApiKeyRepository
{
    private readonly ApplicationDbContext _context;

    public HistorialApiKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(HistorialApiKey historial)
    {
        await _context.Set<HistorialApiKey>().AddAsync(historial);
    }

    public async Task<IEnumerable<HistorialApiKey>> GetByApiKeyIdAsync(long apiKeyId)
    {
        return await _context.Set<HistorialApiKey>()
            .Where(h => h.ApiKeyId == apiKeyId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();
    }
}
