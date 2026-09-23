using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IPerfilCargoRepository
{
    Task<PerfilCargo?> GetByIdAsync(int id);
    Task<PerfilCargo?> GetBySolicitudIdAndVersionAsync(int solicitudId, int version);
    Task<PerfilCargo?> GetLatestBySolicitudIdAsync(int solicitudId);
    Task AddAsync(PerfilCargo perfilCargo);
    void Update(PerfilCargo perfilCargo);
    Task<IEnumerable<PerfilCargo>> ListAsync();
    Task<Estado?> GetEstadoByCodigoAsync(string codigo);
    Task<IEnumerable<StateHistory>> GetStateHistoryForProfilesAsync(List<int> perfilIds);
    Task AddAuditoriaAsync(PerfilAuditoria perfilAuditoria);
    Task<IEnumerable<PerfilAuditoria>> GetAuditoriasByPerfilIdAsync(int perfilId);

    // New methods for Perfiles Sprint
    Task<PerfilCargo?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<PerfilCargo>> ListBySolicitanteIdAsync(int solicitanteId);
    Task<IEnumerable<PerfilObservacion>> GetObservacionesByPerfilIdAsync(int perfilId);
    Task AddObservacionAsync(PerfilObservacion observacion);
    Task<int> GetMaxObservacionIteracionAsync(int perfilId);
    Task AddStateHistoryAsync(StateHistory history);
    Task<IEnumerable<PerfilObservacion>> GetPendientesByPerfilIdAsync(int perfilId);

    // Resumen Ejecutivo methods
    Task<ResumenEjecutivo?> GetResumenByPerfilCargoIdAsync(int perfilCargoId);
    Task AddResumenAsync(ResumenEjecutivo resumen);
    void UpdateResumen(ResumenEjecutivo resumen);
    Task<bool> WasApprovedByAreaAsync(int perfilId);

    // Perfil Estructurado methods
    Task<PerfilEstructurado?> GetEstructuradoBySolicitudIdAsync(int solicitudId, System.Threading.CancellationToken cancellationToken = default);
    Task<PerfilEstructurado?> GetEstructuradoByIdAsync(int id, System.Threading.CancellationToken cancellationToken = default);
    Task<IEnumerable<PerfilEstructurado>> ListEstructuradosAsync(System.Threading.CancellationToken cancellationToken = default);
    Task AddEstructuradoAsync(PerfilEstructurado perfilEstructurado, System.Threading.CancellationToken cancellationToken = default);
    void UpdateEstructurado(PerfilEstructurado perfilEstructurado);
}
