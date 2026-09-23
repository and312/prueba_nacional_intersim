namespace NacionalSeguros.Contracts.Requests;

public record PostulanteEstadoRequestDto(
    int NuevoEstadoId,
    string? MotivoDescarteCodigo,
    string? JustificacionText);
