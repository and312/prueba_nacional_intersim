using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Contracts.Security;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Auditor,Decisor,Reclutador,RRHH")]
[Route("api/v1/estrategia-externas")]
public class EstrategiaExternasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EstrategiaExternasController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EstrategiaExternaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? matchingEjecucionId)
    {
        var query = _context.EstrategiaExternas.AsNoTracking();

        if (matchingEjecucionId.HasValue)
        {
            query = query.Where(r => r.MatchingEjecucionId == matchingEjecucionId.Value);
        }

        var list = await query.ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EstrategiaExternaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var item = await _context.EstrategiaExternas.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (item == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "EstrategiaExterna.NotFound",
                Message = $"No se encontró la estrategia externa con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EstrategiaExternaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] EstrategiaExternaSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        // Validar MatchingEjecucion
        var ejecucionExists = await _context.MatchingEjecuciones.AnyAsync(e => e.Id == dto.MatchingEjecucionId);
        if (!ejecucionExists)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {dto.MatchingEjecucionId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Validar EstadoId si se envía
        if (dto.EstadoId.HasValue)
        {
            var estadoExists = await _context.Estados.AnyAsync(e => e.Id == dto.EstadoId.Value);
            if (!estadoExists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Estado.NotFound",
                    Message = $"No se encontró el estado con ID {dto.EstadoId.Value}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }

        var criteriosDificilesStr = dto.CriteriosDificiles != null ? (dto.CriteriosDificiles is string s1 ? s1 : JsonSerializer.Serialize(dto.CriteriosDificiles)) : "{}";
        var publicoObjetivoStr = dto.PublicoObjetivo != null ? (dto.PublicoObjetivo is string s2 ? s2 : JsonSerializer.Serialize(dto.PublicoObjetivo)) : "{}";
        var canalesSugeridosStr = dto.CanalesSugeridos != null ? (dto.CanalesSugeridos is string s3 ? s3 : JsonSerializer.Serialize(dto.CanalesSugeridos)) : "{}";
        var planAccionStr = dto.PlanAccion != null ? (dto.PlanAccion is string s4 ? s4 : JsonSerializer.Serialize(dto.PlanAccion)) : "{}";
        var briefingEditableStr = dto.BriefingEditable != null ? (dto.BriefingEditable is string s5 ? s5 : JsonSerializer.Serialize(dto.BriefingEditable)) : "{}";

        // Implementar lógica Upsert basada en la unicidad de MatchingEjecucionId
        var entity = await _context.EstrategiaExternas
            .FirstOrDefaultAsync(e => e.MatchingEjecucionId == dto.MatchingEjecucionId);

        if (entity != null)
        {
            entity.Actualizar(
                prioridad: dto.Prioridad,
                justificacion: dto.Justificacion,
                criteriosDificiles: criteriosDificilesStr,
                publicoObjetivo: publicoObjetivoStr,
                canalesSugeridos: canalesSugeridosStr,
                planAccion: planAccionStr,
                briefingEditable: briefingEditableStr,
                conclusion: dto.Conclusion,
                estadoId: dto.EstadoId
            );
            _context.EstrategiaExternas.Update(entity);
        }
        else
        {
            entity = new EstrategiaExterna(dto.MatchingEjecucionId);
            entity.Actualizar(
                prioridad: dto.Prioridad,
                justificacion: dto.Justificacion,
                criteriosDificiles: criteriosDificilesStr,
                publicoObjetivo: publicoObjetivoStr,
                canalesSugeridos: canalesSugeridosStr,
                planAccion: planAccionStr,
                briefingEditable: briefingEditableStr,
                conclusion: dto.Conclusion,
                estadoId: dto.EstadoId
            );
            _context.EstrategiaExternas.Add(entity);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entity.Id }, MapToDto(entity));
    }

    [Authorize(Roles = "Administrador,RRHH")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EstrategiaExternaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] EstrategiaExternaSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        var entity = await _context.EstrategiaExternas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "EstrategiaExterna.NotFound",
                Message = $"No se encontró la estrategia externa con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Validar EstadoId si se envía
        if (dto.EstadoId.HasValue)
        {
            var estadoExists = await _context.Estados.AnyAsync(e => e.Id == dto.EstadoId.Value);
            if (!estadoExists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Estado.NotFound",
                    Message = $"No se encontró el estado con ID {dto.EstadoId.Value}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }

        var criteriosDificilesStr = dto.CriteriosDificiles != null ? (dto.CriteriosDificiles is string s1 ? s1 : JsonSerializer.Serialize(dto.CriteriosDificiles)) : "{}";
        var publicoObjetivoStr = dto.PublicoObjetivo != null ? (dto.PublicoObjetivo is string s2 ? s2 : JsonSerializer.Serialize(dto.PublicoObjetivo)) : "{}";
        var canalesSugeridosStr = dto.CanalesSugeridos != null ? (dto.CanalesSugeridos is string s3 ? s3 : JsonSerializer.Serialize(dto.CanalesSugeridos)) : "{}";
        var planAccionStr = dto.PlanAccion != null ? (dto.PlanAccion is string s4 ? s4 : JsonSerializer.Serialize(dto.PlanAccion)) : "{}";
        var briefingEditableStr = dto.BriefingEditable != null ? (dto.BriefingEditable is string s5 ? s5 : JsonSerializer.Serialize(dto.BriefingEditable)) : "{}";

        entity.Actualizar(
            prioridad: dto.Prioridad,
            justificacion: dto.Justificacion,
            criteriosDificiles: criteriosDificilesStr,
            publicoObjetivo: publicoObjetivoStr,
            canalesSugeridos: canalesSugeridosStr,
            planAccion: planAccionStr,
            briefingEditable: briefingEditableStr,
            conclusion: dto.Conclusion,
            estadoId: dto.EstadoId
        );

        // Actualizar MatchingEjecucionId a través de entry
        var entry = _context.Entry(entity);
        entry.Property(p => p.MatchingEjecucionId).CurrentValue = dto.MatchingEjecucionId;

        _context.EstrategiaExternas.Update(entity);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var entity = await _context.EstrategiaExternas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "EstrategiaExterna.NotFound",
                Message = $"No se encontró la estrategia externa con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        _context.EstrategiaExternas.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static EstrategiaExternaDto MapToDto(EstrategiaExterna entity)
    {
        object criteriosDificiles = new object();
        object publicoObjetivo = new object();
        object canalesSugeridos = new object();
        object planAccion = new object();
        object briefingEditable = new object();

        try { criteriosDificiles = JsonSerializer.Deserialize<object>(entity.CriteriosDificiles ?? "{}") ?? new object(); } catch { }
        try { publicoObjetivo = JsonSerializer.Deserialize<object>(entity.PublicoObjetivo ?? "{}") ?? new object(); } catch { }
        try { canalesSugeridos = JsonSerializer.Deserialize<object>(entity.CanalesSugeridos ?? "{}") ?? new object(); } catch { }
        try { planAccion = JsonSerializer.Deserialize<object>(entity.PlanAccion ?? "{}") ?? new object(); } catch { }
        try { briefingEditable = JsonSerializer.Deserialize<object>(entity.BriefingEditable ?? "{}") ?? new object(); } catch { }

        return new EstrategiaExternaDto
        {
            EstrategiaExternaId = entity.Id,
            MatchingEjecucionId = entity.MatchingEjecucionId,
            Prioridad = entity.Prioridad,
            Justificacion = entity.Justificacion,
            CriteriosDificiles = criteriosDificiles,
            PublicoObjetivo = publicoObjetivo,
            CanalesSugeridos = canalesSugeridos,
            PlanAccion = planAccion,
            BriefingEditable = briefingEditable,
            Conclusion = entity.Conclusion,
            EstadoId = entity.EstadoId,
            FechaCreacion = entity.FechaCreacion,
            FechaModificacion = entity.FechaModificacion
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

public class EstrategiaExternaDto
{
    public int EstrategiaExternaId { get; set; }
    public int MatchingEjecucionId { get; set; }
    public string? Prioridad { get; set; }
    public string? Justificacion { get; set; }
    public object CriteriosDificiles { get; set; } = new object();
    public object PublicoObjetivo { get; set; } = new object();
    public object CanalesSugeridos { get; set; } = new object();
    public object PlanAccion { get; set; } = new object();
    public object BriefingEditable { get; set; } = new object();
    public string? Conclusion { get; set; }
    public int? EstadoId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class EstrategiaExternaSaveDto
{
    public int MatchingEjecucionId { get; set; }
    public string? Prioridad { get; set; }
    public string? Justificacion { get; set; }
    public object? CriteriosDificiles { get; set; }
    public object? PublicoObjetivo { get; set; }
    public object? CanalesSugeridos { get; set; }
    public object? PlanAccion { get; set; }
    public object? BriefingEditable { get; set; }
    public string? Conclusion { get; set; }
    public int? EstadoId { get; set; }
}
