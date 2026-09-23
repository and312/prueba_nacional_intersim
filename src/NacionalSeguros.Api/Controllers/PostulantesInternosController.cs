using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Auditor,Decisor,Reclutador,RRHH")]
[Route("api/v1/postulantes-internos")]
public class PostulantesInternosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostulantesInternosController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PostulanteInternoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? perfilCargoId)
    {
        var query = _context.PostulantesInternos.AsNoTracking().Where(p => !p.IsDeleted);

        if (perfilCargoId.HasValue)
        {
            query = query.Where(p => p.PerfilCargoId == perfilCargoId.Value);
        }

        var list = await query.ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PostulanteInternoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var item = await _context.PostulantesInternos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (item == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PostulanteInterno.NotFound",
                Message = $"No se encontró el postulante interno con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostulanteInternoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] PostulanteInternoSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos de postulación son requeridos.");

        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";

        var postgradoJson = dto.PostgradoInstitucion != null ? JsonSerializer.Serialize(dto.PostgradoInstitucion) : "[]";
        var maestriaJson = dto.MaestriaInstitucion != null ? JsonSerializer.Serialize(dto.MaestriaInstitucion) : "[]";
        
        var certJson = dto.Certificaciones != null ? JsonSerializer.Serialize(dto.Certificaciones) : "[]";
        var cursosJson = dto.CursosComplementarios != null ? JsonSerializer.Serialize(dto.CursosComplementarios) : "[]";
        var idiomasJson = dto.Idiomas != null ? JsonSerializer.Serialize(dto.Idiomas) : "[]";
        var sectoresJson = dto.SectoresExperienciaJson != null ? JsonSerializer.Serialize(dto.SectoresExperienciaJson) : "[]";
        var funcionesJson = dto.FuncionesActualesJson != null ? JsonSerializer.Serialize(dto.FuncionesActualesJson) : "[]";
        var hardJson = dto.HardSkillsJson != null ? JsonSerializer.Serialize(dto.HardSkillsJson) : "[]";
        var softJson = dto.SoftSkillsJson != null ? JsonSerializer.Serialize(dto.SoftSkillsJson) : "[]";
        var herramientasJson = dto.HerramientasSistemasJson != null ? JsonSerializer.Serialize(dto.HerramientasSistemasJson) : "[]";
        var conTecnicosJson = dto.ConocimientosTecnicosJson != null ? JsonSerializer.Serialize(dto.ConocimientosTecnicosJson) : "[]";
        var logrosJson = dto.LogrosRelevantesJson != null ? JsonSerializer.Serialize(dto.LogrosRelevantesJson) : "[]";

        var entity = new PostulanteInterno(
            perfilCargoId: dto.PerfilCargoId,
            nombresApellidos: dto.NombresApellidos,
            fechaPostulacion: dto.FechaPostulacion == default ? DateTime.UtcNow : dto.FechaPostulacion,
            pretensionSalarialBs: dto.PretensionSalarialBs,
            pretensionNegociable: dto.PretensionNegociable,
            ciudadResidencia: dto.CiudadResidencia,
            numeroCelular: dto.NumeroCelular,
            disponibleCambioRegional: dto.DisponibleCambioRegional,
            createdBy: userEmail,
            nombreCargoPostulado: dto.NombreCargoPostulado,
            vinculoGrupoNacional: dto.VinculoGrupoNacional,
            detalleVinculoGrupo: dto.DetalleVinculoGrupo,
            vinculoSectorFinanciero: dto.VinculoSectorFinanciero,
            disponibilidadIncorporacion: dto.DisponibilidadIncorporacion,
            edad: dto.Edad,
            direccion: dto.Direccion,
            correoElectronico: dto.CorreoElectronico,
            ciIdentidad: dto.CiIdentidad,
            estadoCivil: dto.EstadoCivil,
            numeroHijos: dto.NumeroHijos,
            colegio: dto.Colegio,
            carreraInstitucionUniversitaria: dto.CarreraInstitucionUniversitaria,
            estadoAcademico: dto.EstadoAcademico,
            postgradoInstitucion: postgradoJson,
            maestriaInstitucion: maestriaJson,
            empresaActual: dto.EmpresaActual,
            areaActualTrabajo: dto.AreaActualTrabajo,
            supervisorNombreCargo: dto.SupervisorNombreCargo,
            cargoActual: dto.CargoActual,
            motivacionPostulacion: dto.MotivacionPostulacion,
            fechaIngresoCompania: dto.FechaIngresoCompania,
            seniorityActual: dto.SeniorityActual,
            certificaciones: certJson,
            cursosComplementarios: cursosJson,
            idiomas: idiomasJson,
            experienciaTotalAnios: dto.ExperienciaTotalAnios,
            experienciaRelevanteAnios: dto.ExperienciaRelevanteAnios,
            sectoresExperienciaJson: sectoresJson,
            funcionesActualesJson: funcionesJson,
            hardSkillsJson: hardJson,
            softSkillsJson: softJson,
            herramientasSistemasJson: herramientasJson,
            conocimientosTecnicosJson: conTecnicosJson,
            logrosRelevantesJson: logrosJson
        );

        _context.PostulantesInternos.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PostulanteInternoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] PostulanteInternoSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        var entity = await _context.PostulantesInternos
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PostulanteInterno.NotFound",
                Message = $"No se encontró el postulante interno con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";

        var postgradoJson = dto.PostgradoInstitucion != null ? JsonSerializer.Serialize(dto.PostgradoInstitucion) : "[]";
        var maestriaJson = dto.MaestriaInstitucion != null ? JsonSerializer.Serialize(dto.MaestriaInstitucion) : "[]";
        
        var certJson = dto.Certificaciones != null ? JsonSerializer.Serialize(dto.Certificaciones) : "[]";
        var cursosJson = dto.CursosComplementarios != null ? JsonSerializer.Serialize(dto.CursosComplementarios) : "[]";
        var idiomasJson = dto.Idiomas != null ? JsonSerializer.Serialize(dto.Idiomas) : "[]";
        var sectoresJson = dto.SectoresExperienciaJson != null ? JsonSerializer.Serialize(dto.SectoresExperienciaJson) : "[]";
        var funcionesJson = dto.FuncionesActualesJson != null ? JsonSerializer.Serialize(dto.FuncionesActualesJson) : "[]";
        var hardJson = dto.HardSkillsJson != null ? JsonSerializer.Serialize(dto.HardSkillsJson) : "[]";
        var softJson = dto.SoftSkillsJson != null ? JsonSerializer.Serialize(dto.SoftSkillsJson) : "[]";
        var herramientasJson = dto.HerramientasSistemasJson != null ? JsonSerializer.Serialize(dto.HerramientasSistemasJson) : "[]";
        var conTecnicosJson = dto.ConocimientosTecnicosJson != null ? JsonSerializer.Serialize(dto.ConocimientosTecnicosJson) : "[]";
        var logrosJson = dto.LogrosRelevantesJson != null ? JsonSerializer.Serialize(dto.LogrosRelevantesJson) : "[]";

        entity.Actualizar(
            perfilCargoId: dto.PerfilCargoId,
            nombresApellidos: dto.NombresApellidos,
            fechaPostulacion: dto.FechaPostulacion == default ? entity.FechaPostulacion : dto.FechaPostulacion,
            pretensionSalarialBs: dto.PretensionSalarialBs,
            pretensionNegociable: dto.PretensionNegociable,
            ciudadResidencia: dto.CiudadResidencia,
            numeroCelular: dto.NumeroCelular,
            disponibleCambioRegional: dto.DisponibleCambioRegional,
            nombreCargoPostulado: dto.NombreCargoPostulado,
            vinculoGrupoNacional: dto.VinculoGrupoNacional,
            detalleVinculoGrupo: dto.DetalleVinculoGrupo,
            vinculoSectorFinanciero: dto.VinculoSectorFinanciero,
            disponibilidadIncorporacion: dto.DisponibilidadIncorporacion,
            edad: dto.Edad,
            direccion: dto.Direccion,
            correoElectronico: dto.CorreoElectronico,
            ciIdentidad: dto.CiIdentidad,
            estadoCivil: dto.EstadoCivil,
            numeroHijos: dto.NumeroHijos,
            colegio: dto.Colegio,
            carreraInstitucionUniversitaria: dto.CarreraInstitucionUniversitaria,
            estadoAcademico: dto.EstadoAcademico,
            postgradoInstitucion: postgradoJson,
            maestriaInstitucion: maestriaJson,
            empresaActual: dto.EmpresaActual,
            areaActualTrabajo: dto.AreaActualTrabajo,
            supervisorNombreCargo: dto.SupervisorNombreCargo,
            cargoActual: dto.CargoActual,
            motivacionPostulacion: dto.MotivacionPostulacion,
            fechaIngresoCompania: dto.FechaIngresoCompania,
            seniorityActual: dto.SeniorityActual,
            certificaciones: certJson,
            cursosComplementarios: cursosJson,
            idiomas: idiomasJson,
            experienciaTotalAnios: dto.ExperienciaTotalAnios,
            experienciaRelevanteAnios: dto.ExperienciaRelevanteAnios,
            sectoresExperienciaJson: sectoresJson,
            funcionesActualesJson: funcionesJson,
            hardSkillsJson: hardJson,
            softSkillsJson: softJson,
            herramientasSistemasJson: herramientasJson,
            conocimientosTecnicosJson: conTecnicosJson,
            logrosRelevantesJson: logrosJson,
            modifiedBy: userEmail
        );

        _context.PostulantesInternos.Update(entity);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var entity = await _context.PostulantesInternos
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PostulanteInterno.NotFound",
                Message = $"No se encontró el postulante interno con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        entity.Eliminar();
        entity.ActualizarAuditoria(User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo");
        _context.PostulantesInternos.Update(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static PostulanteInternoDto MapToDto(PostulanteInterno entity)
    {
        List<string> postgrado = new();
        List<string> maestria = new();
        List<string> certs = new();
        List<string> cursos = new();
        List<string> idiomas = new();
        List<string> sectores = new();
        List<string> funciones = new();
        List<string> hard = new();
        List<string> soft = new();
        List<string> herramientas = new();
        List<string> conTecnicos = new();
        List<string> logros = new();

        try { postgrado = JsonSerializer.Deserialize<List<string>>(entity.PostgradoInstitucion ?? "[]") ?? new(); } catch { }
        try { maestria = JsonSerializer.Deserialize<List<string>>(entity.MaestriaInstitucion ?? "[]") ?? new(); } catch { }
        try { certs = JsonSerializer.Deserialize<List<string>>(entity.Certificaciones ?? "[]") ?? new(); } catch { }
        try { cursos = JsonSerializer.Deserialize<List<string>>(entity.CursosComplementarios ?? "[]") ?? new(); } catch { }
        try { idiomas = JsonSerializer.Deserialize<List<string>>(entity.Idiomas ?? "[]") ?? new(); } catch { }
        try { sectores = JsonSerializer.Deserialize<List<string>>(entity.SectoresExperienciaJson ?? "[]") ?? new(); } catch { }
        try { funciones = JsonSerializer.Deserialize<List<string>>(entity.FuncionesActualesJson ?? "[]") ?? new(); } catch { }
        try { hard = JsonSerializer.Deserialize<List<string>>(entity.HardSkillsJson ?? "[]") ?? new(); } catch { }
        try { soft = JsonSerializer.Deserialize<List<string>>(entity.SoftSkillsJson ?? "[]") ?? new(); } catch { }
        try { herramientas = JsonSerializer.Deserialize<List<string>>(entity.HerramientasSistemasJson ?? "[]") ?? new(); } catch { }
        try { conTecnicos = JsonSerializer.Deserialize<List<string>>(entity.ConocimientosTecnicosJson ?? "[]") ?? new(); } catch { }
        try { logros = JsonSerializer.Deserialize<List<string>>(entity.LogrosRelevantesJson ?? "[]") ?? new(); } catch { }

        return new PostulanteInternoDto
        {
            Id = entity.Id,
            PerfilCargoId = entity.PerfilCargoId,
            CodigoPostulanteInterno = entity.CodigoPostulanteInterno,
            NombreCargoPostulado = entity.NombreCargoPostulado,
            FechaPostulacion = entity.FechaPostulacion,
            PretensionSalarialBs = entity.PretensionSalarialBs,
            PretensionNegociable = entity.PretensionNegociable,
            VinculoGrupoNacional = entity.VinculoGrupoNacional,
            DetalleVinculoGrupo = entity.DetalleVinculoGrupo,
            VinculoSectorFinanciero = entity.VinculoSectorFinanciero,
            DisponibilidadIncorporacion = entity.DisponibilidadIncorporacion,
            DisponibleCambioRegional = entity.DisponibleCambioRegional,
            NombresApellidos = entity.NombresApellidos,
            Edad = entity.Edad,
            CiudadResidencia = entity.CiudadResidencia,
            Direccion = entity.Direccion,
            NumeroCelular = entity.NumeroCelular,
            CorreoElectronico = entity.CorreoElectronico,
            CiIdentidad = entity.CiIdentidad,
            EstadoCivil = entity.EstadoCivil,
            NumeroHijos = entity.NumeroHijos,
            Colegio = entity.Colegio,
            CarreraInstitucionUniversitaria = entity.CarreraInstitucionUniversitaria,
            EstadoAcademico = entity.EstadoAcademico,
            PostgradoInstitucion = postgrado,
            MaestriaInstitucion = maestria,
            EmpresaActual = entity.EmpresaActual,
            AreaActualTrabajo = entity.AreaActualTrabajo,
            SupervisorNombreCargo = entity.SupervisorNombreCargo,
            CargoActual = entity.CargoActual,
            MotivacionPostulacion = entity.MotivacionPostulacion,
            FechaIngresoCompania = entity.FechaIngresoCompania,
            SeniorityActual = entity.SeniorityActual,
            Certificaciones = certs,
            CursosComplementarios = cursos,
            Idiomas = idiomas,
            ExperienciaTotalAnios = entity.ExperienciaTotalAnios,
            ExperienciaRelevanteAnios = entity.ExperienciaRelevanteAnios,
            SectoresExperienciaJson = sectores,
            FuncionesActualesJson = funciones,
            HardSkillsJson = hard,
            SoftSkillsJson = soft,
            HerramientasSistemasJson = herramientas,
            ConocimientosTecnicosJson = conTecnicos,
            LogrosRelevantesJson = logros
        };
    }

    private string GetCorrelationId()
    {
        if (HttpContext.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null)
        {
            return cid.ToString()!;
        }
        return Guid.NewGuid().ToString();
    }
}

public class PostulanteInternoDto
{
    public int Id { get; set; }
    public int PerfilCargoId { get; set; }
    public string CodigoPostulanteInterno { get; set; } = string.Empty;
    public string NombreCargoPostulado { get; set; } = string.Empty;
    public DateTime FechaPostulacion { get; set; }
    public decimal PretensionSalarialBs { get; set; }
    public string PretensionNegociable { get; set; } = "no";
    public string VinculoGrupoNacional { get; set; } = "no";
    public string? DetalleVinculoGrupo { get; set; }
    public string VinculoSectorFinanciero { get; set; } = "no";
    public string? DisponibilidadIncorporacion { get; set; }
    public string DisponibleCambioRegional { get; set; } = "no";
    public string NombresApellidos { get; set; } = string.Empty;
    public string? Edad { get; set; }
    public string CiudadResidencia { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string NumeroCelular { get; set; } = string.Empty;
    public string? CorreoElectronico { get; set; }
    public string? CiIdentidad { get; set; }
    public string? EstadoCivil { get; set; }
    public string? NumeroHijos { get; set; }
    public string? Colegio { get; set; }
    public string? CarreraInstitucionUniversitaria { get; set; }
    public string? EstadoAcademico { get; set; }
    public List<string> PostgradoInstitucion { get; set; } = new();
    public List<string> MaestriaInstitucion { get; set; } = new();
    public string? EmpresaActual { get; set; }
    public string? AreaActualTrabajo { get; set; }
    public string? SupervisorNombreCargo { get; set; }
    public string? CargoActual { get; set; }
    public string? MotivacionPostulacion { get; set; }
    public string? FechaIngresoCompania { get; set; }

    // Campos Profesionales
    public string? SeniorityActual { get; set; }
    public List<string> Certificaciones { get; set; } = new();
    public List<string> CursosComplementarios { get; set; } = new();
    public List<string> Idiomas { get; set; } = new();
    public string? ExperienciaTotalAnios { get; set; }
    public string? ExperienciaRelevanteAnios { get; set; }
    public List<string> SectoresExperienciaJson { get; set; } = new();
    public List<string> FuncionesActualesJson { get; set; } = new();
    public List<string> HardSkillsJson { get; set; } = new();
    public List<string> SoftSkillsJson { get; set; } = new();
    public List<string> HerramientasSistemasJson { get; set; } = new();
    public List<string> ConocimientosTecnicosJson { get; set; } = new();
    public List<string> LogrosRelevantesJson { get; set; } = new();
}

public class PostulanteInternoSaveDto
{
    public int PerfilCargoId { get; set; }
    public string? NombreCargoPostulado { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public decimal PretensionSalarialBs { get; set; }
    public string PretensionNegociable { get; set; } = "no";
    public string? VinculoGrupoNacional { get; set; }
    public string? DetalleVinculoGrupo { get; set; }
    public string? VinculoSectorFinanciero { get; set; }
    public string? DisponibilidadIncorporacion { get; set; }
    public string DisponibleCambioRegional { get; set; } = "no";
    public string NombresApellidos { get; set; } = string.Empty;
    public string? Edad { get; set; }
    public string CiudadResidencia { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string NumeroCelular { get; set; } = string.Empty;
    public string? CorreoElectronico { get; set; }
    public string? CiIdentidad { get; set; }
    public string? EstadoCivil { get; set; }
    public string? NumeroHijos { get; set; }
    public string? Colegio { get; set; }
    public string? CarreraInstitucionUniversitaria { get; set; }
    public string? EstadoAcademico { get; set; }
    public List<string>? PostgradoInstitucion { get; set; }
    public List<string>? MaestriaInstitucion { get; set; }
    public string? EmpresaActual { get; set; }
    public string? AreaActualTrabajo { get; set; }
    public string? SupervisorNombreCargo { get; set; }
    public string? CargoActual { get; set; }
    public string? MotivacionPostulacion { get; set; }
    public string? FechaIngresoCompania { get; set; }

    // Campos Profesionales
    public string? SeniorityActual { get; set; }
    public List<string>? Certificaciones { get; set; }
    public List<string>? CursosComplementarios { get; set; }
    public List<string>? Idiomas { get; set; }
    public string? ExperienciaTotalAnios { get; set; }
    public string? ExperienciaRelevanteAnios { get; set; }
    public List<string>? SectoresExperienciaJson { get; set; }
    public List<string>? FuncionesActualesJson { get; set; }
    public List<string>? HardSkillsJson { get; set; }
    public List<string>? SoftSkillsJson { get; set; }
    public List<string>? HerramientasSistemasJson { get; set; }
    public List<string>? ConocimientosTecnicosJson { get; set; }
    public List<string>? LogrosRelevantesJson { get; set; }
}
