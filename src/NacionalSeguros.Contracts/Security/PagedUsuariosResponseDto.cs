using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Security;

public class PagedUsuariosResponseDto
{
    public List<UsuarioResponseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}
