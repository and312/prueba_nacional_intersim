using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public record PagedPostulantesResponseDto(
    IEnumerable<PostulanteResponseDto> Items,
    int TotalCount);
