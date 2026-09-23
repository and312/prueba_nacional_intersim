using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class MatchingRepository : IMatchingRepository
{
    private readonly ApplicationDbContext _context;

    public MatchingRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAgentExecutionAsync(AgentExecution execution)
    {
        await _context.Set<AgentExecution>().AddAsync(execution);
    }

    public async Task AddMatchingAsync(Matching matching)
    {
        await _context.Set<Matching>().AddAsync(matching);
    }

    public async Task AddScoringAsync(Scoring scoring)
    {
        await _context.Set<Scoring>().AddAsync(scoring);
    }

    public async Task<Matching?> GetLatestMatchingAsync(int postulanteId, int vacanteId)
    {
        return await _context.Set<Matching>()
            .Include(m => m.Postulante)
            .Include(m => m.Vacante)
            .Where(m => m.PostulanteId == postulanteId && m.VacanteId == vacanteId)
            .OrderByDescending(m => m.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Scoring?> GetLatestScoringAsync(int postulanteId, int vacanteId)
    {
        return await _context.Set<Scoring>()
            .Include(s => s.Postulante)
            .Include(s => s.Vacante)
            .Where(s => s.PostulanteId == postulanteId && s.VacanteId == vacanteId)
            .OrderByDescending(s => s.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<AgentExecution?> GetAgentExecutionByIdAsync(long executionId)
    {
        return await _context.Set<AgentExecution>()
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId);
    }
}
