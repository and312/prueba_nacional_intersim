using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class PerfilCargoRepository : IPerfilCargoRepository
{
    private readonly ApplicationDbContext _context;

    public PerfilCargoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PerfilCargo?> GetByIdAsync(int id)
    {
        var perfil = await _context.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
            .Include(p => p.Secciones)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (perfil != null && perfil.Estado == null)
        {
            await _context.Entry(perfil).Reference(p => p.Estado).LoadAsync();
        }

        return perfil;
    }

    public async Task<PerfilCargo?> GetBySolicitudIdAndVersionAsync(int solicitudId, int version)
    {
        var perfil = await _context.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
            .Include(p => p.Secciones)
            .FirstOrDefaultAsync(p => p.SolicitudId == solicitudId && p.Version == version);

        if (perfil != null && perfil.Estado == null)
        {
            await _context.Entry(perfil).Reference(p => p.Estado).LoadAsync();
        }

        return perfil;
    }

    public async Task<PerfilCargo?> GetLatestBySolicitudIdAsync(int solicitudId)
    {
        var perfil = await _context.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
            .Include(p => p.Secciones)
            .Where(p => p.SolicitudId == solicitudId)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync();

        if (perfil != null && perfil.Estado == null)
        {
            await _context.Entry(perfil).Reference(p => p.Estado).LoadAsync();
        }

        return perfil;
    }

    public async Task AddAsync(PerfilCargo perfilCargo)
    {
        await _context.Set<PerfilCargo>().AddAsync(perfilCargo);
    }

    public void Update(PerfilCargo perfilCargo)
    {
        _context.Set<PerfilCargo>().Update(perfilCargo);
    }

    public async Task<IEnumerable<PerfilCargo>> ListAsync()
    {
        return await _context.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
                .ThenInclude(s => s.Solicitante)
                    .ThenInclude(u => u.Area)
            .Where(p => p.Activo)
            .OrderByDescending(p => p.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estado?> GetEstadoByCodigoAsync(string codigo)
    {
        return await _context.Set<Estado>()
            .FirstOrDefaultAsync(e => e.Codigo == codigo && (e.Entidad == "Perfil" || e.Entidad == "Solicitud"));
    }

    public async Task<IEnumerable<StateHistory>> GetStateHistoryForProfilesAsync(List<int> perfilIds)
    {
        return await _context.Set<StateHistory>()
            .Include(sh => sh.EstadoAnterior)
            .Include(sh => sh.EstadoNuevo)
            .Include(sh => sh.Usuario)
            .Where(sh => (sh.Entidad == "Perfil" || sh.Entidad == "PerfilCargo") && perfilIds.Contains(sh.EntidadId))
            .OrderBy(sh => sh.Fecha)
            .ToListAsync();
    }

    public async Task AddAuditoriaAsync(PerfilAuditoria perfilAuditoria)
    {
        await _context.Set<PerfilAuditoria>().AddAsync(perfilAuditoria);
    }

    public async Task<IEnumerable<PerfilAuditoria>> GetAuditoriasByPerfilIdAsync(int perfilId)
    {
        var perfil = await _context.Set<PerfilCargo>().FirstOrDefaultAsync(p => p.Id == perfilId);
        if (perfil == null) return Enumerable.Empty<PerfilAuditoria>();

        var perfilIds = await _context.Set<PerfilCargo>()
            .Where(p => p.SolicitudId == perfil.SolicitudId)
            .Select(p => p.Id)
            .ToListAsync();

        return await _context.Set<PerfilAuditoria>()
            .Include(pa => pa.PerfilSeccion)
            .Where(pa => perfilIds.Contains(pa.PerfilCargoId))
            .OrderByDescending(pa => pa.FechaHora)
            .ToListAsync();
    }

    // New methods for Perfiles Sprint
    public async Task<PerfilCargo?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
                .ThenInclude(s => s.Solicitante)
                    .ThenInclude(u => u.Area)
            .Include(p => p.Secciones)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PerfilCargo>> ListBySolicitanteIdAsync(int solicitanteId)
    {
        return await _context.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
                .ThenInclude(s => s.Solicitante)
                    .ThenInclude(u => u.Area)
            .Where(p => p.Solicitud.SolicitanteId == solicitanteId && p.Activo)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerfilObservacion>> GetObservacionesByPerfilIdAsync(int perfilId)
    {
        var perfil = await _context.Set<PerfilCargo>().FirstOrDefaultAsync(p => p.Id == perfilId);
        if (perfil == null) return Enumerable.Empty<PerfilObservacion>();

        return await _context.Set<PerfilObservacion>()
            .Include(o => o.UsuarioSolicitante)
            .Include(o => o.AtendidaPorUsuario)
            .Include(o => o.TipoObservacion)
            .Where(o => o.PerfilCargo.SolicitudId == perfil.SolicitudId)
            .ToListAsync();
    }

    public async Task AddObservacionAsync(PerfilObservacion observacion)
    {
        await _context.Set<PerfilObservacion>().AddAsync(observacion);
    }

    public async Task<int> GetMaxObservacionIteracionAsync(int perfilId)
    {
        return await _context.Set<PerfilObservacion>()
            .Where(o => o.PerfilCargoId == perfilId)
            .Select(o => o.NumeroIteracion)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync();
    }

    public async Task AddStateHistoryAsync(StateHistory history)
    {
        await _context.Set<StateHistory>().AddAsync(history);
    }

    public async Task<IEnumerable<PerfilObservacion>> GetPendientesByPerfilIdAsync(int perfilId)
    {
        var perfil = await _context.Set<PerfilCargo>().FirstOrDefaultAsync(p => p.Id == perfilId);
        if (perfil == null) return Enumerable.Empty<PerfilObservacion>();

        return await _context.Set<PerfilObservacion>()
            .Include(o => o.TipoObservacion)
            .Where(o => o.PerfilCargo.SolicitudId == perfil.SolicitudId && o.EstadoObservacion == "Pendiente")
            .ToListAsync();
    }

    public async Task<ResumenEjecutivo?> GetResumenByPerfilCargoIdAsync(int perfilCargoId)
    {
        var resumen = await _context.Set<ResumenEjecutivo>()
            .FirstOrDefaultAsync(r => r.PerfilCargoId == perfilCargoId);

        if (resumen == null)
        {
            var perfil = await _context.Set<PerfilCargo>()
                .FirstOrDefaultAsync(p => p.Id == perfilCargoId);

            if (perfil != null)
            {
                resumen = await _context.Set<ResumenEjecutivo>()
                    .Include(r => r.PerfilCargo)
                    .Where(r => r.PerfilCargo.SolicitudId == perfil.SolicitudId)
                    .OrderByDescending(r => r.PerfilCargo.Version)
                    .FirstOrDefaultAsync();
            }
        }

        return resumen;
    }

    public async Task AddResumenAsync(ResumenEjecutivo resumen)
    {
        await _context.Set<ResumenEjecutivo>().AddAsync(resumen);
    }

    public void UpdateResumen(ResumenEjecutivo resumen)
    {
        _context.Set<ResumenEjecutivo>().Update(resumen);
    }

    public async Task<PerfilEstructurado?> GetEstructuradoBySolicitudIdAsync(int solicitudId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PerfilEstructurado>()
            .FirstOrDefaultAsync(pe => pe.SolicitudId == solicitudId, cancellationToken);
    }

    public async Task<PerfilEstructurado?> GetEstructuradoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PerfilEstructurado>()
            .FirstOrDefaultAsync(pe => pe.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PerfilEstructurado>> ListEstructuradosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<PerfilEstructurado>()
            .ToListAsync(cancellationToken);
    }

    public async Task AddEstructuradoAsync(PerfilEstructurado perfilEstructurado, CancellationToken cancellationToken = default)
    {
        await _context.Set<PerfilEstructurado>().AddAsync(perfilEstructurado, cancellationToken);
    }

    public void UpdateEstructurado(PerfilEstructurado perfilEstructurado)
    {
        _context.Set<PerfilEstructurado>().Update(perfilEstructurado);
    }

    public async Task<bool> WasApprovedByAreaAsync(int perfilId)
    {
        var targetApr = await _context.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-APR-AREA");
        var targetObs = await _context.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-OBS-AREA");
        if (targetApr == null || targetObs == null) return false;

        var lastTransition = await _context.Set<StateHistory>()
            .Where(h => (h.Entidad == "Perfil" || h.Entidad == "PerfilCargo") && h.EntidadId == perfilId && (h.EstadoNuevoId == targetApr.Id || h.EstadoNuevoId == targetObs.Id))
            .OrderByDescending(h => h.Fecha)
            .FirstOrDefaultAsync();

        return lastTransition != null && lastTransition.EstadoNuevoId == targetApr.Id;
    }
}
