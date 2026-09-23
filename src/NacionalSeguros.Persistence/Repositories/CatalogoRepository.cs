using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Repositories;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly ApplicationDbContext _context;

    public CatalogoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Catalogo?> GetByIdAsync(int id)
    {
        return await _context.Catalogos
            .Include(c => c.Parametros)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Catalogo?> GetByCodigoAsync(string codigo)
    {
        return await _context.Catalogos
            .Include(c => c.Parametros)
            .FirstOrDefaultAsync(c => c.Codigo == codigo);
    }

    public async Task<IEnumerable<Catalogo>> GetAllAsync()
    {
        return await _context.Catalogos
            .Include(c => c.Parametros)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Catalogo>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        return await _context.Catalogos
            .Include(c => c.Parametros)
            .Where(c => c.Nombre.Contains(searchTerm) || c.Codigo.Contains(searchTerm))
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task AddAsync(Catalogo catalogo)
    {
        await _context.Catalogos.AddAsync(catalogo);
    }

    public void Update(Catalogo catalogo)
    {
        _context.Catalogos.Update(catalogo);
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        var parametros = await _context.Parametros
            .Where(p => p.CatalogoId == id && !p.IsDeleted)
            .ToListAsync();

        if (!parametros.Any())
        {
            return false;
        }

        var parametroRepository = new ParametroRepository(_context);
        foreach (var parametro in parametros)
        {
            if (await parametroRepository.IsInUseAsync(parametro.Id))
            {
                return true;
            }
        }

        return false;
    }
}
