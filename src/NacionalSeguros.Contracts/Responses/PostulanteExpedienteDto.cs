using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public record PostulanteExpedienteDto(
    int PostulanteId,
    string Correo,
    string ParsedTextCV,
    IEnumerable<string> Skills,
    int MatchingScore,
    string ExplicabilidadIA,
    string EstadoNombre);
