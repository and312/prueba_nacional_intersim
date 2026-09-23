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
[Route("api/v1/postulantes-externos")]
public class PostulantesExternosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostulantesExternosController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PostulanteExternoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? perfilCargoId)
    {
        var query = _context.PostulantesExternos.AsNoTracking().Where(p => !p.IsDeleted);

        if (perfilCargoId.HasValue)
        {
            query = query.Where(p => p.PerfilCargoId == perfilCargoId.Value);
        }

        var list = await query.ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PostulanteExternoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var item = await _context.PostulantesExternos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (item == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PostulanteExterno.NotFound",
                Message = $"No se encontró el postulante externo con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostulanteExternoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] PostulanteExternoSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos de postulación son requeridos.");

        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";

        var postgradoJson = dto.PostgradoInstitucion != null ? JsonSerializer.Serialize(dto.PostgradoInstitucion) : "[]";
        var maestriaJson = dto.MaestriaInstitucion != null ? JsonSerializer.Serialize(dto.MaestriaInstitucion) : "[]";
        var expJson = dto.ExperienciasLaborales != null ? JsonSerializer.Serialize(dto.ExperienciasLaborales) : "[]";
        
        var certJson = dto.Certificaciones != null ? JsonSerializer.Serialize(dto.Certificaciones) : "[]";
        var cursosJson = dto.CursosComplementarios != null ? JsonSerializer.Serialize(dto.CursosComplementarios) : "[]";
        var idiomasJson = dto.Idiomas != null ? JsonSerializer.Serialize(dto.Idiomas) : "[]";
        var sectoresJson = dto.SectoresExperiencia != null ? JsonSerializer.Serialize(dto.SectoresExperiencia) : "[]";
        var hardJson = dto.HardSkills != null ? JsonSerializer.Serialize(dto.HardSkills) : "[]";
        var softJson = dto.SoftSkills != null ? JsonSerializer.Serialize(dto.SoftSkills) : "[]";
        var herramientasJson = dto.HerramientasSistemas != null ? JsonSerializer.Serialize(dto.HerramientasSistemas) : "[]";
        var conTecnicosJson = dto.ConocimientosTecnicos != null ? JsonSerializer.Serialize(dto.ConocimientosTecnicos) : "[]";
        var funcionesJson = dto.FuncionesRelevantes != null ? JsonSerializer.Serialize(dto.FuncionesRelevantes) : "[]";
        var logrosJson = dto.LogrosRelevantes != null ? JsonSerializer.Serialize(dto.LogrosRelevantes) : "[]";

        var entity = new PostulanteExterno(
            perfilCargoId: dto.PerfilCargoId,
            nombresApellidos: dto.NombresApellidos,
            fechaPostulacion: dto.FechaPostulacion == default ? DateTime.UtcNow : dto.FechaPostulacion,
            pretensionSalarialBs: dto.PretensionSalarialBs,
            pretensionNegociable: dto.PretensionNegociable,
            ciudadResidencia: dto.CiudadResidencia,
            numeroCelular: dto.NumeroCelular,
            createdBy: userEmail,
            nombreCargoPostulado: dto.NombreCargoPostulado,
            disponibilidadIncorporacion: dto.DisponibilidadIncorporacion,
            motivacionPostulacion: dto.MotivacionPostulacion,
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
            experienciasLaborales: expJson,
            seniority: dto.Seniority,
            certificaciones: certJson,
            cursosComplementarios: cursosJson,
            idiomas: idiomasJson,
            experienciaTotalAnios: dto.ExperienciaTotalAnios,
            experienciaLiderazgoAnios: dto.ExperienciaLiderazgoAnios,
            sectoresExperiencia: sectoresJson,
            hardSkills: hardJson,
            softSkills: softJson,
            herramientasSistemas: herramientasJson,
            conocimientosTecnicos: conTecnicosJson,
            funcionesRelevantes: funcionesJson,
            logrosRelevantes: logrosJson
        );

        _context.PostulantesExternos.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PostulanteExternoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] PostulanteExternoSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        var entity = await _context.PostulantesExternos
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PostulanteExterno.NotFound",
                Message = $"No se encontró el postulante externo con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";

        var postgradoJson = dto.PostgradoInstitucion != null ? JsonSerializer.Serialize(dto.PostgradoInstitucion) : "[]";
        var maestriaJson = dto.MaestriaInstitucion != null ? JsonSerializer.Serialize(dto.MaestriaInstitucion) : "[]";
        var expJson = dto.ExperienciasLaborales != null ? JsonSerializer.Serialize(dto.ExperienciasLaborales) : "[]";
        
        var certJson = dto.Certificaciones != null ? JsonSerializer.Serialize(dto.Certificaciones) : "[]";
        var cursosJson = dto.CursosComplementarios != null ? JsonSerializer.Serialize(dto.CursosComplementarios) : "[]";
        var idiomasJson = dto.Idiomas != null ? JsonSerializer.Serialize(dto.Idiomas) : "[]";
        var sectoresJson = dto.SectoresExperiencia != null ? JsonSerializer.Serialize(dto.SectoresExperiencia) : "[]";
        var hardJson = dto.HardSkills != null ? JsonSerializer.Serialize(dto.HardSkills) : "[]";
        var softJson = dto.SoftSkills != null ? JsonSerializer.Serialize(dto.SoftSkills) : "[]";
        var herramientasJson = dto.HerramientasSistemas != null ? JsonSerializer.Serialize(dto.HerramientasSistemas) : "[]";
        var conTecnicosJson = dto.ConocimientosTecnicos != null ? JsonSerializer.Serialize(dto.ConocimientosTecnicos) : "[]";
        var funcionesJson = dto.FuncionesRelevantes != null ? JsonSerializer.Serialize(dto.FuncionesRelevantes) : "[]";
        var logrosJson = dto.LogrosRelevantes != null ? JsonSerializer.Serialize(dto.LogrosRelevantes) : "[]";

        entity.Actualizar(
            perfilCargoId: dto.PerfilCargoId,
            nombresApellidos: dto.NombresApellidos,
            fechaPostulacion: dto.FechaPostulacion == default ? entity.FechaPostulacion : dto.FechaPostulacion,
            pretensionSalarialBs: dto.PretensionSalarialBs,
            pretensionNegociable: dto.PretensionNegociable,
            ciudadResidencia: dto.CiudadResidencia,
            numeroCelular: dto.NumeroCelular,
            nombreCargoPostulado: dto.NombreCargoPostulado,
            disponibilidadIncorporacion: dto.DisponibilidadIncorporacion,
            motivacionPostulacion: dto.MotivacionPostulacion,
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
            experienciasLaborales: expJson,
            seniority: dto.Seniority,
            certificaciones: certJson,
            cursosComplementarios: cursosJson,
            idiomas: idiomasJson,
            experienciaTotalAnios: dto.ExperienciaTotalAnios,
            experienciaLiderazgoAnios: dto.ExperienciaLiderazgoAnios,
            sectoresExperiencia: sectoresJson,
            hardSkills: hardJson,
            softSkills: softJson,
            herramientasSistemas: herramientasJson,
            conocimientosTecnicos: conTecnicosJson,
            funcionesRelevantes: funcionesJson,
            logrosRelevantes: logrosJson,
            modifiedBy: userEmail
        );

        _context.PostulantesExternos.Update(entity);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var entity = await _context.PostulantesExternos
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PostulanteExterno.NotFound",
                Message = $"No se encontró el postulante externo con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        entity.Eliminar();
        entity.ActualizarAuditoria(User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo");
        _context.PostulantesExternos.Update(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static PostulanteExternoDto MapToDto(PostulanteExterno entity)
    {
        List<string> postgrado = new();
        List<string> maestria = new();
        List<ExperienciaLaboralDto> exp = new();
        List<string> certs = new();
        List<string> cursos = new();
        List<string> idiomas = new();
        List<string> sectores = new();
        List<string> hard = new();
        List<string> soft = new();
        List<string> herramientas = new();
        List<string> conTecnicos = new();
        List<string> funciones = new();
        List<string> logros = new();

        try { postgrado = JsonSerializer.Deserialize<List<string>>(entity.PostgradoInstitucion ?? "[]") ?? new(); } catch { }
        try { maestria = JsonSerializer.Deserialize<List<string>>(entity.MaestriaInstitucion ?? "[]") ?? new(); } catch { }
        try { exp = JsonSerializer.Deserialize<List<ExperienciaLaboralDto>>(entity.ExperienciasLaborales ?? "[]") ?? new(); } catch { }
        try { certs = JsonSerializer.Deserialize<List<string>>(entity.Certificaciones ?? "[]") ?? new(); } catch { }
        try { cursos = JsonSerializer.Deserialize<List<string>>(entity.CursosComplementarios ?? "[]") ?? new(); } catch { }
        try { idiomas = JsonSerializer.Deserialize<List<string>>(entity.Idiomas ?? "[]") ?? new(); } catch { }
        try { sectores = JsonSerializer.Deserialize<List<string>>(entity.SectoresExperiencia ?? "[]") ?? new(); } catch { }
        try { hard = JsonSerializer.Deserialize<List<string>>(entity.HardSkills ?? "[]") ?? new(); } catch { }
        try { soft = JsonSerializer.Deserialize<List<string>>(entity.SoftSkills ?? "[]") ?? new(); } catch { }
        try { herramientas = JsonSerializer.Deserialize<List<string>>(entity.HerramientasSistemas ?? "[]") ?? new(); } catch { }
        try { conTecnicos = JsonSerializer.Deserialize<List<string>>(entity.ConocimientosTecnicos ?? "[]") ?? new(); } catch { }
        try { funciones = JsonSerializer.Deserialize<List<string>>(entity.FuncionesRelevantes ?? "[]") ?? new(); } catch { }
        try { logros = JsonSerializer.Deserialize<List<string>>(entity.LogrosRelevantes ?? "[]") ?? new(); } catch { }

        return new PostulanteExternoDto
        {
            Id = entity.Id,
            PerfilCargoId = entity.PerfilCargoId,
            CodigoPostulanteExterno = entity.CodigoPostulanteExterno,
            NombreCargoPostulado = entity.NombreCargoPostulado,
            FechaPostulacion = entity.FechaPostulacion,
            PretensionSalarialBs = entity.PretensionSalarialBs,
            PretensionNegociable = entity.PretensionNegociable,
            DisponibilidadIncorporacion = entity.DisponibilidadIncorporacion,
            MotivacionPostulacion = entity.MotivacionPostulacion,
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
            ExperienciasLaborales = exp,
            Seniority = entity.Seniority,
            Certificaciones = certs,
            CursosComplementarios = cursos,
            Idiomas = idiomas,
            ExperienciaTotalAnios = entity.ExperienciaTotalAnios,
            ExperienciaLiderazgoAnios = entity.ExperienciaLiderazgoAnios,
            SectoresExperiencia = sectores,
            HardSkills = hard,
            SoftSkills = soft,
            HerramientasSistemas = herramientas,
            ConocimientosTecnicos = conTecnicos,
            FuncionesRelevantes = funciones,
            LogrosRelevantes = logros
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

public class PostulanteExternoDto
{
    public int Id { get; set; }
    public int PerfilCargoId { get; set; }
    public string CodigoPostulanteExterno { get; set; } = string.Empty;
    public string NombreCargoPostulado { get; set; } = string.Empty;
    public DateTime FechaPostulacion { get; set; }
    public decimal PretensionSalarialBs { get; set; }
    public string PretensionNegociable { get; set; } = "no";
    public string? DisponibilidadIncorporacion { get; set; }
    public string? MotivacionPostulacion { get; set; }
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
    public List<ExperienciaLaboralDto> ExperienciasLaborales { get; set; } = new();

    // Campos Profesionales
    public string? Seniority { get; set; }
    public List<string> Certificaciones { get; set; } = new();
    public List<string> CursosComplementarios { get; set; } = new();
    public List<string> Idiomas { get; set; } = new();
    public string? ExperienciaTotalAnios { get; set; }
    public string? ExperienciaLiderazgoAnios { get; set; }
    public List<string> SectoresExperiencia { get; set; } = new();
    public List<string> HardSkills { get; set; } = new();
    public List<string> SoftSkills { get; set; } = new();
    public List<string> HerramientasSistemas { get; set; } = new();
    public List<string> ConocimientosTecnicos { get; set; } = new();
    public List<string> FuncionesRelevantes { get; set; } = new();
    public List<string> LogrosRelevantes { get; set; } = new();
}

public class PostulanteExternoSaveDto
{
    public int PerfilCargoId { get; set; }
    public string? NombreCargoPostulado { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public decimal PretensionSalarialBs { get; set; }
    public string PretensionNegociable { get; set; } = "no";
    public string? DisponibilidadIncorporacion { get; set; }
    public string? MotivacionPostulacion { get; set; }
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
    public List<ExperienciaLaboralDto>? ExperienciasLaborales { get; set; }

    // Campos Profesionales
    public string? Seniority { get; set; }
    public List<string>? Certificaciones { get; set; }
    public List<string>? CursosComplementarios { get; set; }
    public List<string>? Idiomas { get; set; }
    public string? ExperienciaTotalAnios { get; set; }
    public string? ExperienciaLiderazgoAnios { get; set; }
    public List<string>? SectoresExperiencia { get; set; }
    public List<string>? HardSkills { get; set; }
    public List<string>? SoftSkills { get; set; }
    public List<string>? HerramientasSistemas { get; set; }
    public List<string>? ConocimientosTecnicos { get; set; }
    public List<string>? FuncionesRelevantes { get; set; }
    public List<string>? LogrosRelevantes { get; set; }
}

public class ExperienciaLaboralDto
{
    public string NombreEmpresa { get; set; } = string.Empty;
    public string CargoDesempeniado { get; set; } = string.Empty;
    public string Funciones { get; set; } = string.Empty;
    public string PeriodoTrabajo { get; set; } = string.Empty;
    public string MotivoRetiro { get; set; } = string.Empty;
    public string NombreCargoSupervisor { get; set; } = string.Empty;
    public string ContactoSupervisor { get; set; } = string.Empty;
}
