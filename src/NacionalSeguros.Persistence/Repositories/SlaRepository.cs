using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class SlaRepository : ISlaRepository
{
    private readonly ApplicationDbContext _context;

    public SlaRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Sla?> GetByIdAsync(int id)
    {
        return await _context.Slas.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Sla>> GetAllAsync()
    {
        return await _context.Slas.ToListAsync();
    }
}
