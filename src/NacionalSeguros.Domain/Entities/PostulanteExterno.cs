using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PostulanteExterno : Entity<int>
{
    // Requerido por EF Core
    protected PostulanteExterno()
    {
    }

    public PostulanteExterno(
        int perfilCargoId,
        string nombresApellidos,
        DateTime fechaPostulacion,
        decimal pretensionSalarialBs,
        string pretensionNegociable,
        string ciudadResidencia,
        string numeroCelular,
        string createdBy,
        string? nombreCargoPostulado = null,
        string? disponibilidadIncorporacion = null,
        string? motivacionPostulacion = null,
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
        string? experienciasLaborales = null,
        string? seniority = null,
        string? certificaciones = null,
        string? cursosComplementarios = null,
        string? idiomas = null,
        string? experienciaTotalAnios = null,
        string? experienciaLiderazgoAnios = null,
        string? sectoresExperiencia = null,
        string? hardSkills = null,
        string? softSkills = null,
        string? herramientasSistemas = null,
        string? conocimientosTecnicos = null,
        string? funcionesRelevantes = null,
        string? logrosRelevantes = null)
    {
        if (perfilCargoId <= 0) throw new ArgumentException("PerfilCargoId debe ser mayor a 0", nameof(perfilCargoId));
        
        PerfilCargoId = perfilCargoId;
        NombresApellidos = nombresApellidos ?? throw new ArgumentNullException(nameof(nombresApellidos));
        FechaPostulacion = fechaPostulacion;
        PretensionSalarialBs = pretensionSalarialBs;
        PretensionNegociable = pretensionNegociable ?? throw new ArgumentNullException(nameof(pretensionNegociable));
        CiudadResidencia = ciudadResidencia ?? throw new ArgumentNullException(nameof(ciudadResidencia));
        NumeroCelular = numeroCelular ?? throw new ArgumentNullException(nameof(numeroCelular));
        
        NombreCargoPostulado = nombreCargoPostulado ?? string.Empty;
        DisponibilidadIncorporacion = disponibilidadIncorporacion;
        MotivacionPostulacion = motivacionPostulacion;
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
        ExperienciasLaborales = experienciasLaborales ?? "[]";

        Seniority = seniority;
        Certificaciones = certificaciones ?? "[]";
        CursosComplementarios = cursosComplementarios ?? "[]";
        Idiomas = idiomas ?? "[]";
        ExperienciaTotalAnios = experienciaTotalAnios;
        ExperienciaLiderazgoAnios = experienciaLiderazgoAnios;
        SectoresExperiencia = sectoresExperiencia ?? "[]";
        HardSkills = hardSkills ?? "[]";
        SoftSkills = softSkills ?? "[]";
        HerramientasSistemas = herramientasSistemas ?? "[]";
        ConocimientosTecnicos = conocimientosTecnicos ?? "[]";
        FuncionesRelevantes = funcionesRelevantes ?? "[]";
        LogrosRelevantes = logrosRelevantes ?? "[]";

        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public int PerfilCargoId { get; private set; }
    public string CodigoPostulanteExterno { get; private set; } = null!; // Autogenerado en BD
    public string NombreCargoPostulado { get; private set; } = string.Empty;
    public DateTime FechaPostulacion { get; private set; }
    public decimal PretensionSalarialBs { get; private set; }
    public string PretensionNegociable { get; private set; } = "no";
    public string? DisponibilidadIncorporacion { get; private set; }
    public string? MotivacionPostulacion { get; private set; } // Nullable
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
    public string ExperienciasLaborales { get; private set; } = "[]";

    // Nuevas Columnas Profesionales Externas
    public string? Seniority { get; private set; }
    public string Certificaciones { get; private set; } = "[]";
    public string CursosComplementarios { get; private set; } = "[]";
    public string Idiomas { get; private set; } = "[]";
    public string? ExperienciaTotalAnios { get; private set; }
    public string? ExperienciaLiderazgoAnios { get; private set; }
    public string SectoresExperiencia { get; private set; } = "[]";
    public string HardSkills { get; private set; } = "[]";
    public string SoftSkills { get; private set; } = "[]";
    public string HerramientasSistemas { get; private set; } = "[]";
    public string ConocimientosTecnicos { get; private set; } = "[]";
    public string FuncionesRelevantes { get; private set; } = "[]";
    public string LogrosRelevantes { get; private set; } = "[]";

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
        string? nombreCargoPostulado,
        string? disponibilidadIncorporacion,
        string? motivacionPostulacion,
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
        string? experienciasLaborales,
        string? seniority,
        string? certificaciones,
        string? cursosComplementarios,
        string? idiomas,
        string? experienciaTotalAnios,
        string? experienciaLiderazgoAnios,
        string? sectoresExperiencia,
        string? hardSkills,
        string? softSkills,
        string? herramientasSistemas,
        string? conocimientosTecnicos,
        string? funcionesRelevantes,
        string? logrosRelevantes,
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
        
        NombreCargoPostulado = nombreCargoPostulado ?? string.Empty;
        DisponibilidadIncorporacion = disponibilidadIncorporacion;
        MotivacionPostulacion = motivacionPostulacion;
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
        ExperienciasLaborales = experienciasLaborales ?? "[]";

        Seniority = seniority;
        Certificaciones = certificaciones ?? "[]";
        CursosComplementarios = cursosComplementarios ?? "[]";
        Idiomas = idiomas ?? "[]";
        ExperienciaTotalAnios = experienciaTotalAnios;
        ExperienciaLiderazgoAnios = experienciaLiderazgoAnios;
        SectoresExperiencia = sectoresExperiencia ?? "[]";
        HardSkills = hardSkills ?? "[]";
        SoftSkills = softSkills ?? "[]";
        HerramientasSistemas = herramientasSistemas ?? "[]";
        ConocimientosTecnicos = conocimientosTecnicos ?? "[]";
        FuncionesRelevantes = funcionesRelevantes ?? "[]";
        LogrosRelevantes = logrosRelevantes ?? "[]";

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
