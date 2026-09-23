using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class SolicitudRepository : ISolicitudRepository
{
    private readonly ApplicationDbContext _context;

    public SolicitudRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Solicitud?> GetByIdAsync(int id)
    {
        return await _context.Set<Solicitud>()
            .Include(s => s.Estado)
            .Include(s => s.Solicitante)
                .ThenInclude(u => u.Area)
            .Include(s => s.Decisor)
            .Include(s => s.Regional)
            .Include(s => s.TipoSolicitud)
            .Include(s => s.ModalidadTrabajo)
            .Include(s => s.Comentarios)
                .ThenInclude(c => c.Usuario)
                    .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Solicitud solicitud)
    {
        await _context.Set<Solicitud>().AddAsync(solicitud);
    }

    public void Update(Solicitud solicitud)
    {
        _context.Set<Solicitud>().Update(solicitud);
    }

    public async Task<(IEnumerable<Solicitud> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? estadoId,
        string? search,
        int? solicitanteId = null,
        bool excludePending = false)
    {
        IQueryable<Solicitud> query = _context.Set<Solicitud>()
            .Include(s => s.Estado)
            .Include(s => s.Solicitante)
                .ThenInclude(u => u.Area)
            .Include(s => s.Decisor)
            .Include(s => s.Regional)
            .Include(s => s.TipoSolicitud)
            .Include(s => s.ModalidadTrabajo)
            .Include(s => s.Comentarios)
                .ThenInclude(c => c.Usuario)
                    .ThenInclude(u => u.Roles)
            .AsNoTracking();

        if (estadoId.HasValue)
        {
            query = query.Where(s => s.EstadoId == estadoId.Value);
        }

        if (solicitanteId.HasValue)
        {
            query = query.Where(s => s.SolicitanteId == solicitanteId.Value);
        }

        if (excludePending)
        {
            query = query.Where(s => s.Estado.Codigo != "SOL-PEN");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Cargo.Contains(search) || 
                (s.Solicitante != null && s.Solicitante.Area != null && s.Solicitante.Area.Nombre.Contains(search)));
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.ModifiedDate ?? s.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<StateHistory>> GetStateHistoryAsync(int solicitudId)
    {
        return await _context.Set<StateHistory>()
            .Include(sh => sh.EstadoAnterior)
            .Include(sh => sh.EstadoNuevo)
            .Include(sh => sh.Usuario!).ThenInclude(u => u.Roles)
            .Where(sh => sh.Entidad == "Solicitud" && sh.EntidadId == solicitudId)
            .OrderBy(sh => sh.Fecha)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estado?> GetEstadoByCodigoAsync(string codigo)
    {
        return await _context.Set<Estado>()
            .FirstOrDefaultAsync(e => e.Codigo == codigo && e.Entidad == "Solicitud");
    }

    public async Task<DateTime?> GetFechaUltimaInteraccionSolicitanteAsync(int solicitudId)
    {
        var lastUpdate = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.Entidad.StartsWith("Solicitud") && a.EntidadId == solicitudId && a.Rol == "Solicitante")
            .OrderByDescending(a => a.FechaHoraUTC)
            .Select(a => (DateTime?)a.FechaHoraUTC)
            .FirstOrDefaultAsync();

        if (lastUpdate.HasValue)
        {
            return lastUpdate.Value;
        }

        var solicitud = await _context.Set<Solicitud>()
            .AsNoTracking()
            .Where(s => s.Id == solicitudId)
            .Select(s => (DateTime?)s.CreatedDate)
            .FirstOrDefaultAsync();

        return solicitud;
    }
}
