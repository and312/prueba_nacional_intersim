using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public class PagedSolicitudesResponseDto
{
    public List<SolicitudResponseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}
