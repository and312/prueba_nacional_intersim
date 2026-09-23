using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class PostulanteRepository : IPostulanteRepository
{
    private readonly ApplicationDbContext _context;

    public PostulanteRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Postulante?> GetByIdAsync(int id)
    {
        return await _context.Set<Postulante>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Postulante?> GetByCorreoAsync(string correo)
    {
        return await _context.Set<Postulante>()
            .FirstOrDefaultAsync(p => p.Correo.ToLower() == correo.ToLower());
    }

    public async Task<Postulacion?> GetPostulacionByIdsAsync(int postulanteId, int vacanteId)
    {
        return await _context.Set<Postulacion>()
            .Include(p => p.Postulante)
            .Include(p => p.EstadoPipeline)
            .FirstOrDefaultAsync(p => p.PostulanteId == postulanteId && p.VacanteId == vacanteId);
    }

    public async Task<Estado?> GetEstadoByCodigoAsync(string codigo)
    {
        return await _context.Set<Estado>()
            .FirstOrDefaultAsync(e => e.Codigo == codigo && e.Entidad == "Postulante");
    }

    public async Task<Estado?> GetEstadoByIdAsync(int id)
    {
        return await _context.Set<Estado>()
            .FirstOrDefaultAsync(e => e.Id == id && e.Entidad == "Postulante");
    }

    public async Task AddPostulanteAsync(Postulante postulante)
    {
        await _context.Set<Postulante>().AddAsync(postulante);
    }

    public async Task AddPostulacionAsync(Postulacion postulacion)
    {
        await _context.Set<Postulacion>().AddAsync(postulacion);
    }

    public void UpdatePostulante(Postulante postulante)
    {
        _context.Set<Postulante>().Update(postulante);
    }

    public void UpdatePostulacion(Postulacion postulacion)
    {
        _context.Set<Postulacion>().Update(postulacion);
    }

    public async Task<IEnumerable<Postulacion>> ListPostulacionesAsync(int? vacanteId)
    {
        IQueryable<Postulacion> query = _context.Set<Postulacion>()
            .Include(p => p.Postulante)
            .Include(p => p.EstadoPipeline);

        if (vacanteId.HasValue && vacanteId.Value > 0)
        {
            query = query.Where(p => p.VacanteId == vacanteId.Value);
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Postulacion?> GetExpedienteDetailsAsync(int postulanteId)
    {
        // Obtener la postulación más reciente del candidato
        return await _context.Set<Postulacion>()
            .Include(p => p.Postulante)
            .Include(p => p.EstadoPipeline)
            .OrderByDescending(p => p.FechaPostulacion)
            .FirstOrDefaultAsync(p => p.PostulanteId == postulanteId);
    }
}
