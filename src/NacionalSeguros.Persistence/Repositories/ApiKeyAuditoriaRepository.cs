using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class ApiKeyAuditoriaRepository : IApiKeyAuditoriaRepository
{
    private readonly ApplicationDbContext _context;

    public ApiKeyAuditoriaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ApiKeyAuditoria auditoria)
    {
        await _context.Set<ApiKeyAuditoria>().AddAsync(auditoria);
    }

    public async Task<IEnumerable<ApiKeyAuditoria>> ListAsync()
    {
        return await _context.Set<ApiKeyAuditoria>()
            .OrderByDescending(a => a.FechaHora)
            .ToListAsync();
    }
}
