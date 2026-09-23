using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class MatchingEjecucion : Entity<int>
{
    // Requerido por EF Core
    protected MatchingEjecucion()
    {
    }

    public MatchingEjecucion(
        int perfilCargoId,
        int perfilEstructuradoId,
        int versionPerfil,
        int estadoId,
        int totalEvaluados,
        int totalInternosEvaluados,
        int totalHistoricosEvaluados,
        int totalMatchAlto,
        int totalMatchMedio,
        int totalMatchBajo,
        int totalDescartados,
        int totalPotenciales,
        string fuentesConsultadasJson,
        string resumenDescartesJson,
        string parametrosMatchingJson,
        DateTime fechaInicio,
        decimal? compatibilidadPromedio = null,
        string? estrategiaRecomendada = null,
        decimal? nivelConfianza = null,
        string? justificacion = null,
        DateTime? fechaFin = null,
        string? mensajeError = null)
    {
        if (perfilCargoId <= 0) throw new ArgumentException("PerfilCargoId debe ser mayor a 0", nameof(perfilCargoId));
        if (perfilEstructuradoId <= 0) throw new ArgumentException("PerfilEstructuradoId debe ser mayor a 0", nameof(perfilEstructuradoId));

        PerfilCargoId = perfilCargoId;
        PerfilEstructuradoId = perfilEstructuradoId;
        VersionPerfil = versionPerfil;
        EstadoId = estadoId;
        TotalEvaluados = totalEvaluados;
        TotalInternosEvaluados = totalInternosEvaluados;
        TotalHistoricosEvaluados = totalHistoricosEvaluados;
        TotalMatchAlto = totalMatchAlto;
        TotalMatchMedio = totalMatchMedio;
        TotalMatchBajo = totalMatchBajo;
        TotalDescartados = totalDescartados;
        TotalPotenciales = totalPotenciales;
        FuentesConsultadasJson = fuentesConsultadasJson ?? "[]";
        ResumenDescartesJson = resumenDescartesJson ?? "[]";
        ParametrosMatchingJson = parametrosMatchingJson ?? "{}";
        FechaInicio = fechaInicio;
        
        CompatibilidadPromedio = compatibilidadPromedio;
        EstrategiaRecomendada = estrategiaRecomendada;
        NivelConfianza = nivelConfianza;
        Justificacion = justificacion;
        FechaFin = fechaFin;
        MensajeError = mensajeError;
        
        CreatedDate = DateTime.UtcNow;
    }

    public string CodigoMatching { get; private set; } = null!; // Autogenerado en BD
    public int PerfilCargoId { get; private set; }
    public int PerfilEstructuradoId { get; private set; }
    public int VersionPerfil { get; private set; }
    public int EstadoId { get; private set; }
    public int TotalEvaluados { get; private set; }
    public int TotalInternosEvaluados { get; private set; }
    public int TotalHistoricosEvaluados { get; private set; }
    public int TotalMatchAlto { get; private set; }
    public int TotalMatchMedio { get; private set; }
    public int TotalMatchBajo { get; private set; }
    public int TotalDescartados { get; private set; }
    public int TotalPotenciales { get; private set; }
    public decimal? CompatibilidadPromedio { get; private set; }
    public string? EstrategiaRecomendada { get; private set; }
    public decimal? NivelConfianza { get; private set; }
    public string? Justificacion { get; private set; }
    public string FuentesConsultadasJson { get; private set; } = "[]";
    public string ResumenDescartesJson { get; private set; } = "[]";
    public string ParametrosMatchingJson { get; private set; } = "{}";
    public DateTime FechaInicio { get; private set; }
    public DateTime? FechaFin { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string? MensajeError { get; private set; }

    // Propiedades de navegación
    public PerfilCargo PerfilCargo { get; private set; } = null!;
    public PerfilEstructurado PerfilEstructurado { get; private set; } = null!;
    public Estado Estado { get; private set; } = null!;

    public void SetCodigoMatching(string codigoMatching)
    {
        if (string.IsNullOrWhiteSpace(codigoMatching)) throw new ArgumentException("El codigo de matching no puede estar vacio", nameof(codigoMatching));
        CodigoMatching = codigoMatching;
    }

    public void FinalizarConExito(
        int totalEvaluados,
        int totalInternosEvaluados,
        int totalHistoricosEvaluados,
        int totalMatchAlto,
        int totalMatchMedio,
        int totalMatchBajo,
        int totalDescartados,
        int totalPotenciales,
        decimal compatibilidadPromedio,
        string estrategiaRecomendada,
        decimal nivelConfianza,
        string justificacion,
        string fuentesConsultadasJson,
        string resumenDescartesJson,
        string parametrosMatchingJson,
        int estadoId = 39)
    {
        EstadoId = estadoId;
        TotalEvaluados = totalEvaluados;
        TotalInternosEvaluados = totalInternosEvaluados;
        TotalHistoricosEvaluados = totalHistoricosEvaluados;
        TotalMatchAlto = totalMatchAlto;
        TotalMatchMedio = totalMatchMedio;
        TotalMatchBajo = totalMatchBajo;
        TotalDescartados = totalDescartados;
        TotalPotenciales = totalPotenciales;
        CompatibilidadPromedio = compatibilidadPromedio;
        EstrategiaRecomendada = estrategiaRecomendada;
        NivelConfianza = nivelConfianza;
        Justificacion = justificacion;
        FuentesConsultadasJson = fuentesConsultadasJson;
        ResumenDescartesJson = resumenDescartesJson;
        ParametrosMatchingJson = parametrosMatchingJson;
        FechaFin = DateTime.UtcNow;
    }

    public void FinalizarConError(string mensajeError, int estadoId = 37)
    {
        EstadoId = estadoId;
        MensajeError = mensajeError;
        FechaFin = DateTime.UtcNow;
    }
}
