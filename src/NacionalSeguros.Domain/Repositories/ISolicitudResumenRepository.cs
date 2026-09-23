using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ISolicitudResumenRepository
{
    Task<SolicitudResumen?> GetBySolicitudIdAsync(int solicitudId);
    Task AddAsync(SolicitudResumen resumen);
    void Update(SolicitudResumen resumen);
}
