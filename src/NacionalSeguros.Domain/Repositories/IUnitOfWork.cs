namespace NacionalSeguros.Domain.Repositories;

public interface IUnitOfWork
{
    string? TransitionComment { get; set; }
    bool SkipWhatsAppNotification { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
