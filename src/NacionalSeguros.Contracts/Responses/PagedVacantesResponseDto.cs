using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public record PagedVacantesResponseDto(
    IEnumerable<VacanteResponseDto> Items,
    int TotalCount);
