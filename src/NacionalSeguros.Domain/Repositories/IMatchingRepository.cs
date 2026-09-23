using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IMatchingRepository
{
    Task AddAgentExecutionAsync(AgentExecution execution);
    Task AddMatchingAsync(Matching matching);
    Task AddScoringAsync(Scoring scoring);
    Task<Matching?> GetLatestMatchingAsync(int postulanteId, int vacanteId);
    Task<Scoring?> GetLatestScoringAsync(int postulanteId, int vacanteId);
    Task<AgentExecution?> GetAgentExecutionByIdAsync(long executionId);
}
