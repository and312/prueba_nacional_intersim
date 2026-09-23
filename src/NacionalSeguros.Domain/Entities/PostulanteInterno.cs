using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PostulanteInterno : Entity<int>
{
    // Requerido por EF Core
    protected PostulanteInterno()
    {
    }

    public PostulanteInterno(
        int perfilCargoId,
        string nombresApellidos,
        DateTime fechaPostulacion,
        decimal pretensionSalarialBs,
        string pretensionNegociable,
        string ciudadResidencia,
        string numeroCelular,
        string disponibleCambioRegional,
        string createdBy,
        string? nombreCargoPostulado = null,
        string? vinculoGrupoNacional = null,
        string? detalleVinculoGrupo = null,
        string? vinculoSectorFinanciero = null,
        string? disponibilidadIncorporacion = null,
        string? edad = null,
        string? direccion = null,
        string? correoElectronico = null,
        string? ciIdentidad = null,
        string? estadoCivil = null,
        string? numeroHijos = null,
        string? colegio = null,
        string? carreraInstitucionUniversitaria = null,
        string? estadoAcademico = null,
        string? postgradoInstitucion = null,
        string? maestriaInstitucion = null,
        string? empresaActual = null,
        string? areaActualTrabajo = null,
        string? supervisorNombreCargo = null,
        string? cargoActual = null,
        string? motivacionPostulacion = null,
        string? fechaIngresoCompania = null,
        string? seniorityActual = null,
        string? certificaciones = null,
        string? cursosComplementarios = null,
        string? idiomas = null,
        string? experienciaTotalAnios = null,
        string? experienciaRelevanteAnios = null,
        string? sectoresExperienciaJson = null,
        string? funcionesActualesJson = null,
        string? hardSkillsJson = null,
        string? softSkillsJson = null,
        string? herramientasSistemasJson = null,
        string? conocimientosTecnicosJson = null,
        string? logrosRelevantesJson = null)
    {
        if (perfilCargoId <= 0) throw new ArgumentException("PerfilCargoId debe ser mayor a 0", nameof(perfilCargoId));
        
        PerfilCargoId = perfilCargoId;
        NombresApellidos = nombresApellidos ?? throw new ArgumentNullException(nameof(nombresApellidos));
        FechaPostulacion = fechaPostulacion;
        PretensionSalarialBs = pretensionSalarialBs;
        PretensionNegociable = pretensionNegociable ?? throw new ArgumentNullException(nameof(pretensionNegociable));
        CiudadResidencia = ciudadResidencia ?? throw new ArgumentNullException(nameof(ciudadResidencia));
        NumeroCelular = numeroCelular ?? throw new ArgumentNullException(nameof(numeroCelular));
        DisponibleCambioRegional = disponibleCambioRegional ?? throw new ArgumentNullException(nameof(disponibleCambioRegional));
        
        NombreCargoPostulado = nombreCargoPostulado ?? string.Empty;
        VinculoGrupoNacional = vinculoGrupoNacional ?? "no";
        DetalleVinculoGrupo = detalleVinculoGrupo;
        VinculoSectorFinanciero = vinculoSectorFinanciero ?? "no";
        DisponibilidadIncorporacion = disponibilidadIncorporacion;
        Edad = edad;
        Direccion = direccion;
        CorreoElectronico = correoElectronico;
        CiIdentidad = ciIdentidad;
        EstadoCivil = estadoCivil;
        NumeroHijos = numeroHijos;
        Colegio = colegio;
        CarreraInstitucionUniversitaria = carreraInstitucionUniversitaria;
        EstadoAcademico = estadoAcademico;
        PostgradoInstitucion = postgradoInstitucion ?? "[]";
        MaestriaInstitucion = maestriaInstitucion ?? "[]";
        EmpresaActual = empresaActual;
        AreaActualTrabajo = areaActualTrabajo;
        SupervisorNombreCargo = supervisorNombreCargo;
        CargoActual = cargoActual;
        MotivacionPostulacion = motivacionPostulacion;
        FechaIngresoCompania = fechaIngresoCompania;

        SeniorityActual = seniorityActual;
        Certificaciones = certificaciones ?? "[]";
        CursosComplementarios = cursosComplementarios ?? "[]";
        Idiomas = idiomas ?? "[]";
        ExperienciaTotalAnios = experienciaTotalAnios;
        ExperienciaRelevanteAnios = experienciaRelevanteAnios;
        SectoresExperienciaJson = sectoresExperienciaJson ?? "[]";
        FuncionesActualesJson = funcionesActualesJson ?? "[]";
        HardSkillsJson = hardSkillsJson ?? "[]";
        SoftSkillsJson = softSkillsJson ?? "[]";
        HerramientasSistemasJson = herramientasSistemasJson ?? "[]";
        ConocimientosTecnicosJson = conocimientosTecnicosJson ?? "[]";
        LogrosRelevantesJson = logrosRelevantesJson ?? "[]";

        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public int PerfilCargoId { get; private set; }
    public string CodigoPostulanteInterno { get; private set; } = null!; // Autogenerado en BD
    public string NombreCargoPostulado { get; private set; } = string.Empty;
    public DateTime FechaPostulacion { get; private set; }
    public decimal PretensionSalarialBs { get; private set; }
    public string PretensionNegociable { get; private set; } = "no";
    public string VinculoGrupoNacional { get; private set; } = "no";
    public string? DetalleVinculoGrupo { get; private set; }
    public string VinculoSectorFinanciero { get; private set; } = "no";
    public string? DisponibilidadIncorporacion { get; private set; }
    public string DisponibleCambioRegional { get; private set; } = "no"; // Obligatorio: "si" o "no"
    public string NombresApellidos { get; private set; } = string.Empty;
    public string? Edad { get; private set; }
    public string CiudadResidencia { get; private set; } = string.Empty;
    public string? Direccion { get; private set; }
    public string NumeroCelular { get; private set; } = string.Empty;
    public string? CorreoElectronico { get; private set; }
    public string? CiIdentidad { get; private set; }
    public string? EstadoCivil { get; private set; }
    public string? NumeroHijos { get; private set; }
    public string? Colegio { get; private set; }
    public string? CarreraInstitucionUniversitaria { get; private set; }
    public string? EstadoAcademico { get; private set; }
    public string PostgradoInstitucion { get; private set; } = "[]";
    public string MaestriaInstitucion { get; private set; } = "[]";
    public string? EmpresaActual { get; private set; }
    public string? AreaActualTrabajo { get; private set; }
    public string? SupervisorNombreCargo { get; private set; }
    public string? CargoActual { get; private set; }
    public string? MotivacionPostulacion { get; private set; }
    public string? FechaIngresoCompania { get; private set; }

    // Nuevas Columnas Profesionales Internas
    public string? SeniorityActual { get; private set; }
    public string Certificaciones { get; private set; } = "[]";
    public string CursosComplementarios { get; private set; } = "[]";
    public string Idiomas { get; private set; } = "[]";
    public string? ExperienciaTotalAnios { get; private set; }
    public string? ExperienciaRelevanteAnios { get; private set; }
    public string SectoresExperienciaJson { get; private set; } = "[]";
    public string FuncionesActualesJson { get; private set; } = "[]";
    public string HardSkillsJson { get; private set; } = "[]";
    public string SoftSkillsJson { get; private set; } = "[]";
    public string HerramientasSistemasJson { get; private set; } = "[]";
    public string ConocimientosTecnicosJson { get; private set; } = "[]";
    public string LogrosRelevantesJson { get; private set; } = "[]";

    // Campos de Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedad de navegación
    public PerfilCargo PerfilCargo { get; private set; } = null!;

    public void Actualizar(
        int perfilCargoId,
        string nombresApellidos,
        DateTime fechaPostulacion,
        decimal pretensionSalarialBs,
        string pretensionNegociable,
        string ciudadResidencia,
        string numeroCelular,
        string disponibleCambioRegional,
        string? nombreCargoPostulado,
        string? vinculoGrupoNacional,
        string? detalleVinculoGrupo,
        string? vinculoSectorFinanciero,
        string? disponibilidadIncorporacion,
        string? edad,
        string? direccion,
        string? correoElectronico,
        string? ciIdentidad,
        string? estadoCivil,
        string? numeroHijos,
        string? colegio,
        string? carreraInstitucionUniversitaria,
        string? estadoAcademico,
        string? postgradoInstitucion,
        string? maestriaInstitucion,
        string? empresaActual,
        string? areaActualTrabajo,
        string? supervisorNombreCargo,
        string? cargoActual,
        string? motivacionPostulacion,
        string? fechaIngresoCompania,
        string? seniorityActual,
        string? certificaciones,
        string? cursosComplementarios,
        string? idiomas,
        string? experienciaTotalAnios,
        string? experienciaRelevanteAnios,
        string? sectoresExperienciaJson,
        string? funcionesActualesJson,
        string? hardSkillsJson,
        string? softSkillsJson,
        string? herramientasSistemasJson,
        string? conocimientosTecnicosJson,
        string? logrosRelevantesJson,
        string modifiedBy)
    {
        if (perfilCargoId <= 0) throw new ArgumentException("PerfilCargoId debe ser mayor a 0", nameof(perfilCargoId));
        
        PerfilCargoId = perfilCargoId;
        NombresApellidos = nombresApellidos ?? throw new ArgumentNullException(nameof(nombresApellidos));
        FechaPostulacion = fechaPostulacion;
        PretensionSalarialBs = pretensionSalarialBs;
        PretensionNegociable = pretensionNegociable ?? throw new ArgumentNullException(nameof(pretensionNegociable));
        CiudadResidencia = ciudadResidencia ?? throw new ArgumentNullException(nameof(ciudadResidencia));
        NumeroCelular = numeroCelular ?? throw new ArgumentNullException(nameof(numeroCelular));
        DisponibleCambioRegional = disponibleCambioRegional ?? throw new ArgumentNullException(nameof(disponibleCambioRegional));
        
        NombreCargoPostulado = nombreCargoPostulado ?? string.Empty;
        VinculoGrupoNacional = vinculoGrupoNacional ?? "no";
        DetalleVinculoGrupo = detalleVinculoGrupo;
        VinculoSectorFinanciero = vinculoSectorFinanciero ?? "no";
        DisponibilidadIncorporacion = disponibilidadIncorporacion;
        Edad = edad;
        Direccion = direccion;
        CorreoElectronico = correoElectronico;
        CiIdentidad = ciIdentidad;
        EstadoCivil = estadoCivil;
        NumeroHijos = numeroHijos;
        Colegio = colegio;
        CarreraInstitucionUniversitaria = carreraInstitucionUniversitaria;
        EstadoAcademico = estadoAcademico;
        PostgradoInstitucion = postgradoInstitucion ?? "[]";
        MaestriaInstitucion = maestriaInstitucion ?? "[]";
        EmpresaActual = empresaActual;
        AreaActualTrabajo = areaActualTrabajo;
        SupervisorNombreCargo = supervisorNombreCargo;
        CargoActual = cargoActual;
        MotivacionPostulacion = motivacionPostulacion;
        FechaIngresoCompania = fechaIngresoCompania;

        SeniorityActual = seniorityActual;
        Certificaciones = certificaciones ?? "[]";
        CursosComplementarios = cursosComplementarios ?? "[]";
        Idiomas = idiomas ?? "[]";
        ExperienciaTotalAnios = experienciaTotalAnios;
        ExperienciaRelevanteAnios = experienciaRelevanteAnios;
        SectoresExperienciaJson = sectoresExperienciaJson ?? "[]";
        FuncionesActualesJson = funcionesActualesJson ?? "[]";
        HardSkillsJson = hardSkillsJson ?? "[]";
        SoftSkillsJson = softSkillsJson ?? "[]";
        HerramientasSistemasJson = herramientasSistemasJson ?? "[]";
        ConocimientosTecnicosJson = conocimientosTecnicosJson ?? "[]";
        LogrosRelevantesJson = logrosRelevantesJson ?? "[]";

        ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        ModifiedDate = DateTime.UtcNow;
    }

    public void ActualizarAuditoria(string modificadoPor)
    {
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
