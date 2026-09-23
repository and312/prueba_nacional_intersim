using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface ISolicitudDocumentoRepository
{
    Task<IEnumerable<SolicitudDocumento>> GetBySolicitudIdAsync(int solicitudId, string? tipoDocumento = null);
    Task AddAsync(SolicitudDocumento documento);
}
