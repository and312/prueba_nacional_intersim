using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByCorreoAsync(string correo);
    Task AddAsync(Usuario usuario);
    void Update(Usuario usuario);
    Task<(IEnumerable<Usuario> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? search,
        int? areaId = null,
        int? rolId = null,
        string? estado = null,
        string? cargo = null,
        string? gerencia = null);
}
