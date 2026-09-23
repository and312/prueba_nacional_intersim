using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly ApplicationDbContext _context;

    public ApiKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKey?> GetByIdAsync(long id)
    {
        return await _context.Set<ApiKey>()
            .Include(a => a.Integracion)
            .Include(a => a.ApiKeyPermisos)
                .ThenInclude(ap => ap.Permiso)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ApiKey?> GetByHashAsync(string apiKeyHash)
    {
        return await _context.Set<ApiKey>()
            .Include(a => a.Integracion)
            .Include(a => a.ApiKeyPermisos)
                .ThenInclude(ap => ap.Permiso)
            .FirstOrDefaultAsync(a => a.ApiKeyHash == apiKeyHash);
    }

    public async Task AddAsync(ApiKey apiKey)
    {
        await _context.Set<ApiKey>().AddAsync(apiKey);
    }

    public void Update(ApiKey apiKey)
    {
        _context.Set<ApiKey>().Update(apiKey);
    }

    public async Task<IEnumerable<ApiKey>> ListAsync()
    {
        return await _context.Set<ApiKey>()
            .Include(a => a.Integracion)
            .Include(a => a.ApiKeyPermisos)
                .ThenInclude(ap => ap.Permiso)
            .ToListAsync();
    }
}
