using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ISolicitudRepository
{
    Task<Solicitud?> GetByIdAsync(int id);
    Task AddAsync(Solicitud solicitud);
    void Update(Solicitud solicitud);
    Task<(IEnumerable<Solicitud> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, int? estadoId, string? search, int? solicitanteId = null, bool excludePending = false);
    Task<IEnumerable<StateHistory>> GetStateHistoryAsync(int solicitudId);
    Task<Estado?> GetEstadoByCodigoAsync(string codigo);
    Task<DateTime?> GetFechaUltimaInteraccionSolicitanteAsync(int solicitudId);
}
