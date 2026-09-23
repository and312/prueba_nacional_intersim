using System;
using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public record PerfilEstructuradoResponseDto(
    int PerfilEstructuradoId,
    int SolicitudId,
    string ObjetivoPrincipalCargo,
    string PerfilIdealCandidato,
    string PerfilTipoAltoAjuste,
    string EstadoGeneracion,
    object DatosGeneralesCargo,
    object PerfilRequerido,
    object HerramientasSistemas,
    object FiltrosClaveSeleccion,
    object ConocimientosTecnicosRequeridos,
    object FuncionesPrincipalesCargo,
    object CompetenciasClave,
    object IndicadoresExitoCargo,
    object MatrizPonderacion,
    object FuentesUtilizadas,
    object Alertas,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate,
    int? Version = null
);
