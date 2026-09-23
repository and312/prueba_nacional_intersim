using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Set<Usuario>()
            .Include(u => u.Area)
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permisos)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByCorreoAsync(string correo)
    {
        return await _context.Set<Usuario>()
            .Include(u => u.Area)
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permisos)
            .FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task AddAsync(Usuario usuario)
    {
        await _context.Set<Usuario>().AddAsync(usuario);
    }

    public void Update(Usuario usuario)
    {
        _context.Set<Usuario>().Update(usuario);
    }

    public async Task<(IEnumerable<Usuario> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? search,
        int? areaId = null,
        int? rolId = null,
        string? estado = null,
        string? cargo = null,
        string? gerencia = null)
    {
        IQueryable<Usuario> query = _context.Set<Usuario>()
            .Include(u => u.Area)
            .Include(u => u.Roles);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.Nombre.Contains(search) || u.Correo.Contains(search) || (u.Cargo != null && u.Cargo.Contains(search)));
        }

        if (areaId.HasValue)
        {
            query = query.Where(u => u.AreaId == areaId.Value);
        }

        if (rolId.HasValue)
        {
            query = query.Where(u => u.Roles.Any(r => r.Id == rolId.Value));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (System.Enum.TryParse<NacionalSeguros.Domain.Enums.UsuarioEstado>(estado, true, out var parsedEstado))
            {
                query = query.Where(u => u.Estado == parsedEstado);
            }
        }

        if (!string.IsNullOrWhiteSpace(cargo))
        {
            query = query.Where(u => u.Cargo != null && u.Cargo == cargo);
        }

        if (!string.IsNullOrWhiteSpace(gerencia))
        {
            query = query.Where(u => u.Gerencia != null && u.Gerencia == gerencia);
        }

        int totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
