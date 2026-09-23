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
[Route("api/v1/estrategia-internas")]
public class EstrategiaInternasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EstrategiaInternasController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EstrategiaInternaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? matchingEjecucionId)
    {
        var query = _context.EstrategiaInternas.AsNoTracking();

        if (matchingEjecucionId.HasValue)
        {
            query = query.Where(r => r.MatchingEjecucionId == matchingEjecucionId.Value);
        }

        var list = await query.ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EstrategiaInternaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var item = await _context.EstrategiaInternas.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (item == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "EstrategiaInterna.NotFound",
                Message = $"No se encontró la estrategia interna con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EstrategiaInternaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] EstrategiaInternaSaveDto dto)
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

        var planAccionStr = dto.PlanAccion != null ? (dto.PlanAccion is string s1 ? s1 : JsonSerializer.Serialize(dto.PlanAccion)) : "{}";
        var planEvaluacionStr = dto.PlanEvaluacion != null ? (dto.PlanEvaluacion is string s2 ? s2 : JsonSerializer.Serialize(dto.PlanEvaluacion)) : "{}";

        // Implementar lógica Upsert basada en la unicidad de MatchingEjecucionId
        var entity = await _context.EstrategiaInternas
            .FirstOrDefaultAsync(e => e.MatchingEjecucionId == dto.MatchingEjecucionId);

        if (entity != null)
        {
            entity.Actualizar(
                prioridad: dto.Prioridad,
                justificacion: dto.Justificacion,
                planAccion: planAccionStr,
                planEvaluacion: planEvaluacionStr,
                mensajeContingencia: dto.MensajeContingencia,
                conclusion: dto.Conclusion,
                estadoId: dto.EstadoId
            );
            _context.EstrategiaInternas.Update(entity);
        }
        else
        {
            entity = new EstrategiaInterna(dto.MatchingEjecucionId);
            entity.Actualizar(
                prioridad: dto.Prioridad,
                justificacion: dto.Justificacion,
                planAccion: planAccionStr,
                planEvaluacion: planEvaluacionStr,
                mensajeContingencia: dto.MensajeContingencia,
                conclusion: dto.Conclusion,
                estadoId: dto.EstadoId
            );
            _context.EstrategiaInternas.Add(entity);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EstrategiaInternaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] EstrategiaInternaSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        var entity = await _context.EstrategiaInternas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "EstrategiaInterna.NotFound",
                Message = $"No se encontró la estrategia interna con ID {id}.",
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

        var planAccionStr = dto.PlanAccion != null ? (dto.PlanAccion is string s1 ? s1 : JsonSerializer.Serialize(dto.PlanAccion)) : "{}";
        var planEvaluacionStr = dto.PlanEvaluacion != null ? (dto.PlanEvaluacion is string s2 ? s2 : JsonSerializer.Serialize(dto.PlanEvaluacion)) : "{}";

        entity.Actualizar(
            prioridad: dto.Prioridad,
            justificacion: dto.Justificacion,
            planAccion: planAccionStr,
            planEvaluacion: planEvaluacionStr,
            mensajeContingencia: dto.MensajeContingencia,
            conclusion: dto.Conclusion,
            estadoId: dto.EstadoId
        );

        // Actualizar MatchingEjecucionId a través de entry
        var entry = _context.Entry(entity);
        entry.Property(p => p.MatchingEjecucionId).CurrentValue = dto.MatchingEjecucionId;

        _context.EstrategiaInternas.Update(entity);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var entity = await _context.EstrategiaInternas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "EstrategiaInterna.NotFound",
                Message = $"No se encontró la estrategia interna con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        _context.EstrategiaInternas.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static EstrategiaInternaDto MapToDto(EstrategiaInterna entity)
    {
        object planAccion = new object();
        object planEvaluacion = new object();

        try { planAccion = JsonSerializer.Deserialize<object>(entity.PlanAccion ?? "{}") ?? new object(); } catch { }
        try { planEvaluacion = JsonSerializer.Deserialize<object>(entity.PlanEvaluacion ?? "{}") ?? new object(); } catch { }

        return new EstrategiaInternaDto
        {
            EstrategiaInternaId = entity.Id,
            MatchingEjecucionId = entity.MatchingEjecucionId,
            Prioridad = entity.Prioridad,
            Justificacion = entity.Justificacion,
            PlanAccion = planAccion,
            PlanEvaluacion = planEvaluacion,
            MensajeContingencia = entity.MensajeContingencia,
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

public class EstrategiaInternaDto
{
    public int EstrategiaInternaId { get; set; }
    public int MatchingEjecucionId { get; set; }
    public string? Prioridad { get; set; }
    public string? Justificacion { get; set; }
    public object PlanAccion { get; set; } = new object();
    public object PlanEvaluacion { get; set; } = new object();
    public string? MensajeContingencia { get; set; }
    public string? Conclusion { get; set; }
    public int? EstadoId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class EstrategiaInternaSaveDto
{
    public int MatchingEjecucionId { get; set; }
    public string? Prioridad { get; set; }
    public string? Justificacion { get; set; }
    public object? PlanAccion { get; set; }
    public object? PlanEvaluacion { get; set; }
    public string? MensajeContingencia { get; set; }
    public string? Conclusion { get; set; }
    public int? EstadoId { get; set; }
}
