using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PerfilEstructurado : Entity<int>
{
    protected PerfilEstructurado()
    {
    }

    public PerfilEstructurado(
        int solicitudId,
        string objetivoPrincipalCargo,
        string perfilIdealCandidato,
        string perfilTipoAltoAjuste,
        string estadoGeneracion,
        string datosGeneralesCargo,
        string perfilRequerido,
        string herramientasSistemas,
        string filtrosClaveSeleccion,
        string conocimientosTecnicosRequeridos,
        string funcionesPrincipalesCargo,
        string competenciasClave,
        string indicadoresExitoCargo,
        string matrizPonderacion,
        string fuentesUtilizadas,
        string alertas,
        string createdBy)
    {
        if (solicitudId <= 0)
        {
            throw new ArgumentException("SolicitudId debe ser mayor a 0", nameof(solicitudId));
        }

        SolicitudId = solicitudId;
        ObjetivoPrincipalCargo = objetivoPrincipalCargo ?? string.Empty;
        PerfilIdealCandidato = perfilIdealCandidato ?? string.Empty;
        PerfilTipoAltoAjuste = perfilTipoAltoAjuste ?? string.Empty;
        EstadoGeneracion = estadoGeneracion ?? string.Empty;
        DatosGeneralesCargo = datosGeneralesCargo ?? "{}";
        PerfilRequerido = perfilRequerido ?? "{}";
        HerramientasSistemas = herramientasSistemas ?? "{}";
        FiltrosClaveSeleccion = filtrosClaveSeleccion ?? "{}";
        ConocimientosTecnicosRequeridos = conocimientosTecnicosRequeridos ?? "[]";
        FuncionesPrincipalesCargo = funcionesPrincipalesCargo ?? "[]";
        CompetenciasClave = competenciasClave ?? "[]";
        IndicadoresExitoCargo = indicadoresExitoCargo ?? "[]";
        MatrizPonderacion = matrizPonderacion ?? "[]";
        FuentesUtilizadas = fuentesUtilizadas ?? "[]";
        Alertas = alertas ?? "[]";
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
    }

    public int SolicitudId { get; private set; }
    public string ObjetivoPrincipalCargo { get; private set; } = string.Empty;
    public string PerfilIdealCandidato { get; private set; } = string.Empty;
    public string PerfilTipoAltoAjuste { get; private set; } = string.Empty;
    public string EstadoGeneracion { get; private set; } = string.Empty;

    // JSON columns stored as NVARCHAR(MAX)
    public string DatosGeneralesCargo { get; private set; } = "{}";
    public string PerfilRequerido { get; private set; } = "{}";
    public string HerramientasSistemas { get; private set; } = "{}";
    public string FiltrosClaveSeleccion { get; private set; } = "{}";
    public string ConocimientosTecnicosRequeridos { get; private set; } = "[]";
    public string FuncionesPrincipalesCargo { get; private set; } = "[]";
    public string CompetenciasClave { get; private set; } = "[]";
    public string IndicadoresExitoCargo { get; private set; } = "[]";
    public string MatrizPonderacion { get; private set; } = "[]";
    public string FuentesUtilizadas { get; private set; } = "[]";
    public string Alertas { get; private set; } = "[]";

    // Audit fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    // Navigation property
    public Solicitud Solicitud { get; private set; } = null!;

    public void Actualizar(
        string objetivoPrincipalCargo,
        string perfilIdealCandidato,
        string perfilTipoAltoAjuste,
        string estadoGeneracion,
        string datosGeneralesCargo,
        string perfilRequerido,
        string herramientasSistemas,
        string filtrosClaveSeleccion,
        string conocimientosTecnicosRequeridos,
        string funcionesPrincipalesCargo,
        string competenciasClave,
        string indicadoresExitoCargo,
        string matrizPonderacion,
        string fuentesUtilizadas,
        string alertas,
        string modifiedBy)
    {
        ObjetivoPrincipalCargo = objetivoPrincipalCargo ?? string.Empty;
        PerfilIdealCandidato = perfilIdealCandidato ?? string.Empty;
        PerfilTipoAltoAjuste = perfilTipoAltoAjuste ?? string.Empty;
        EstadoGeneracion = estadoGeneracion ?? string.Empty;
        DatosGeneralesCargo = datosGeneralesCargo ?? "{}";
        PerfilRequerido = perfilRequerido ?? "{}";
        HerramientasSistemas = herramientasSistemas ?? "{}";
        FiltrosClaveSeleccion = filtrosClaveSeleccion ?? "{}";
        ConocimientosTecnicosRequeridos = conocimientosTecnicosRequeridos ?? "[]";
        FuncionesPrincipalesCargo = funcionesPrincipalesCargo ?? "[]";
        CompetenciasClave = competenciasClave ?? "[]";
        IndicadoresExitoCargo = indicadoresExitoCargo ?? "[]";
        MatrizPonderacion = matrizPonderacion ?? "[]";
        FuentesUtilizadas = fuentesUtilizadas ?? "[]";
        Alertas = alertas ?? "[]";
        ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        ModifiedDate = DateTime.UtcNow;
    }
}
