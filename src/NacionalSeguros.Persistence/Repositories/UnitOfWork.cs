using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string? TransitionComment { get; set; }
    public bool SkipWhatsAppNotification { get; set; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(TransitionComment))
        {
            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync(cancellationToken);
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_set_session_context 'TransitionComment', @Comment;";
            var p = cmd.CreateParameter();
            p.ParameterName = "@Comment";
            p.Value = TransitionComment;
            cmd.Parameters.Add(p);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        int result = await _dbContext.SaveChangesAsync(cancellationToken);

        // Reset context to avoid carry-over
        TransitionComment = null;
        SkipWhatsAppNotification = false;

        return result;
    }
}
