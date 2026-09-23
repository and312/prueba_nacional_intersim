using System.Collections.Generic;

namespace NacionalSeguros.Contracts.Requests;

public record ResumenEjecutivoUpdateRequest(
    int PerfilId,
    int SolicitudId,
    ResumenEjecutivoRolDto ResumenEjecutivoRol
);

public record ResumenEjecutivoRolDto(
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
    string ValoracionPerfil,
    int? Version = null
);

public record PerfilObservacionCreateRequest(int TipoObservacionId, string Comentario);

public record TipoObservacionCreateRequest(string Codigo, string Nombre, string? Descripcion);

public record TipoObservacionUpdateRequest(string Nombre, string? Descripcion);
