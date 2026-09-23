using System;
using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Responses;

public record PerfilListItemDto(
    int PerfilId,
    string CodigoPerfil,
    int SolicitudId,
    string CodigoSolicitud,
    string Cargo,
    string AreaSolicitante,
    string EstadoPerfil,
    string EstadoCodigo,
    DateTime UltimaActualizacion,
    string AccionRequerida
);

public record PerfilListResponseDto(
    int PerfilId,
    string CodigoPerfil,
    string CodigoSolicitud,
    string Cargo,
    string AreaSolicitante,
    string EstadoPerfil,
    string EstadoCodigo,
    DateTime UltimaActualizacion,
    string AccionRequerida,
    string Solicitante,
    int SolicitanteId,
    string Canal,
    string Prioridad,
    DateTime FechaSolicitud
);

public record PerfilSeccionDto(
    int SeccionId,
    int NumeroSeccion,
    string NombreSeccion,
    string Contenido,
    int Orden
);

public record PerfilObservacionResponseDto(
    int Id,
    int PerfilCargoId,
    int TipoObservacionId,
    string TipoObservacionNombre,
    string Comentario,
    int UsuarioSolicitanteId,
    string UsuarioSolicitanteNombre,
    int NumeroIteracion,
    string EstadoObservacion,
    DateTime CreatedDate,
    int? AtendidaPorUsuarioId,
    string? AtendidaPorUsuarioNombre,
    DateTime? FechaAtencion
);

public record ResumenEjecutivoDto(
    string Resumen,
    string ObjetivoCargo,
    List<string> FuncionesPrincipales,
    List<string> RequisitosMinimos,
    string FormacionExperiencia,
    List<string> HardSkills,
    List<string> SoftSkills,
    string Modalidad,
    string Ubicacion,
    string BandaSalarial,
    string CriteriosEvaluacion,
    string CaracteristicasClave,
    string ValoracionPerfil
);

public record PerfilEstructuradoDto(
    DatosGeneralesDto DatosGenerales,
    PerfilRequeridoDto PerfilRequerido,
    CondicionesVacanteDto CondicionesVacante
);

public record DatosGeneralesDto(
    DateTime FechaSolicitud,
    string Area,
    string Solicitante,
    string CargoSolicitante,
    string Cargo,
    string Regional,
    int CantidadVacantes,
    string TipoSolicitud,
    string Motivo
);

public record PerfilRequeridoDto(
    string ObjetivoCargo,
    string FormacionAcademica,
    string ExperienciaMinima,
    string ExperienciaIndispensable,
    string ConocimientosTecnicos,
    string HerramientasSistemas,
    string CompetenciasClave,
    string CriteriosExcluyentes,
    string CriteriosDeseables,
    string Funciones
);

public record CondicionesVacanteDto(
    string ModalidadTrabajo,
    string DisponibilidadRequerida,
    string Seniority,
    string Prioridad
);

public record PerfilDocumentoResponseDto(
    int DocumentoId,
    int SolicitudId,
    string TipoDocumento,
    string FileName,
    string StorageProvider,
    string StoragePath,
    string? PublicUrl,
    string? GeneradoPor,
    DateTime CreatedDate
);

public record PerfilDetailResponseDto(
    int PerfilId,
    string CodigoPerfil,
    string CodigoSolicitud,
    int SolicitudId,
    string Cargo,
    string AreaSolicitante,
    string EstadoPerfil,
    string EstadoCodigo,
    DateTime UltimaActualizacion,
    string AccionRequerida,
    string Solicitante,
    int SolicitanteId,
    ResumenEjecutivoDto? ResumenEjecutivo,
    object PerfilEstructurado,
    List<PerfilObservacionResponseDto> Observaciones,
    int Version = 1,
    bool AprobadoPorArea = false,
    string? Salario = null,
    List<PerfilDocumentoResponseDto>? Documentos = null
);

public record PerfilEstructuradoPersistidoDto(
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
    object Alertas
);

public record TipoObservacionResponseDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    string Estado
);
