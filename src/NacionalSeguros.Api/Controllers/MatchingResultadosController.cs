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
[Route("api/v1/matching-resultados")]
public class MatchingResultadosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MatchingResultadosController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MatchingResultadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? matchingEjecucionId, [FromQuery] int? postulanteId)
    {
        var query = _context.MatchingResultados.AsNoTracking();

        if (matchingEjecucionId.HasValue)
        {
            query = query.Where(r => r.MatchingEjecucionId == matchingEjecucionId.Value);
        }

        if (postulanteId.HasValue)
        {
            query = query.Where(r => r.PostulanteId == postulanteId.Value);
        }

        var list = await query.ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MatchingResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var item = await _context.MatchingResultados.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (item == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingResultado.NotFound",
                Message = $"No se encontró el resultado de matching con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(MatchingResultadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] MatchingResultadoSaveDto dto)
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

        // Validar Postulante y Origen
        if (string.Equals(dto.Origen, "BD_INTERNA", StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.PostulantesInternos.AnyAsync(p => p.Id == dto.PostulanteId);
            if (!exists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Postulante.NotFound",
                    Message = $"No se encontró el postulante interno con ID {dto.PostulanteId}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }
        else if (string.Equals(dto.Origen, "BD_EXTERNA_HISTORICA", StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.PostulantesExternos.AnyAsync(p => p.Id == dto.PostulanteId);
            if (!exists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Postulante.NotFound",
                    Message = $"No se encontró el postulante externo con ID {dto.PostulanteId}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }
        else if (string.Equals(dto.Origen, "POOL_MANUAL", StringComparison.OrdinalIgnoreCase) ||
                 (dto.Origen ?? "").ToUpper().Contains("POOL") ||
                 (dto.Origen ?? "").ToUpper().Contains("MANUAL"))
        {
            var existsInt = await _context.PostulantesInternos.AnyAsync(p => p.Id == dto.PostulanteId);
            var existsExt = await _context.PostulantesExternos.AnyAsync(p => p.Id == dto.PostulanteId);
            if (!existsInt && !existsExt)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Postulante.NotFound",
                    Message = $"No se encontró el postulante con ID {dto.PostulanteId}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }
        else
        {
            return BadRequest(new ApiErrorDto
            {
                Code = "Origen.Invalid",
                Message = "El origen debe ser 'BD_INTERNA', 'BD_EXTERNA_HISTORICA' o 'POOL_MANUAL'.",
                CorrelationId = GetCorrelationId()
            });
        }

        var fortalezasJson = dto.Fortalezas != null ? JsonSerializer.Serialize(dto.Fortalezas) : "[]";
        var brechasJson = dto.Brechas != null ? JsonSerializer.Serialize(dto.Brechas) : "[]";
        var desgloseJson = dto.DesgloseCriterios != null ? JsonSerializer.Serialize(dto.DesgloseCriterios) : "[]";

        var entity = await _context.MatchingResultados
            .FirstOrDefaultAsync(r => r.MatchingEjecucionId == dto.MatchingEjecucionId &&
                                     r.PostulanteId == dto.PostulanteId &&
                                     r.Origen == dto.Origen);

        if (entity != null)
        {
            entity.Actualizar(
                porcentajeMatching: dto.PorcentajeMatching,
                clasificacion: dto.Clasificacion,
                posicionRanking: dto.PosicionRanking,
                puntajeObtenido: dto.PuntajeObtenido,
                puntajeMaximo: dto.PuntajeMaximo,
                fortalezasJson: fortalezasJson,
                brechasJson: brechasJson,
                desgloseCriteriosJson: desgloseJson,
                tipoDescarte: dto.TipoDescarte,
                motivoExclusion: dto.MotivoExclusion
            );
            _context.MatchingResultados.Update(entity);
        }
        else
        {
            entity = new MatchingResultado(
                matchingEjecucionId: dto.MatchingEjecucionId,
                postulanteId: dto.PostulanteId,
                origen: dto.Origen! ?? "POOL_MANUAL",
                porcentajeMatching: dto.PorcentajeMatching,
                clasificacion: dto.Clasificacion,
                posicionRanking: dto.PosicionRanking,
                puntajeObtenido: dto.PuntajeObtenido,
                puntajeMaximo: dto.PuntajeMaximo,
                fortalezasJson: fortalezasJson,
                brechasJson: brechasJson,
                desgloseCriteriosJson: desgloseJson,
                tipoDescarte: dto.TipoDescarte,
                motivoExclusion: dto.MotivoExclusion
            );
            _context.MatchingResultados.Add(entity);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MatchingResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] MatchingResultadoSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        var entity = await _context.MatchingResultados
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingResultado.NotFound",
                Message = $"No se encontró el resultado de matching con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        var fortalezasJson = dto.Fortalezas != null ? JsonSerializer.Serialize(dto.Fortalezas) : "[]";
        var brechasJson = dto.Brechas != null ? JsonSerializer.Serialize(dto.Brechas) : "[]";
        var desgloseJson = dto.DesgloseCriterios != null ? JsonSerializer.Serialize(dto.DesgloseCriterios) : "[]";

        entity.Actualizar(
            porcentajeMatching: dto.PorcentajeMatching,
            clasificacion: dto.Clasificacion,
            posicionRanking: dto.PosicionRanking,
            puntajeObtenido: dto.PuntajeObtenido,
            puntajeMaximo: dto.PuntajeMaximo,
            fortalezasJson: fortalezasJson,
            brechasJson: brechasJson,
            desgloseCriteriosJson: desgloseJson,
            tipoDescarte: dto.TipoDescarte,
            motivoExclusion: dto.MotivoExclusion
        );

        // Actualizar Origen y PostulanteId a través de entry
        var entry = _context.Entry(entity);
        entry.Property(p => p.MatchingEjecucionId).CurrentValue = dto.MatchingEjecucionId;
        entry.Property(p => p.PostulanteId).CurrentValue = dto.PostulanteId;
        entry.Property(p => p.Origen).CurrentValue = dto.Origen;

        _context.MatchingResultados.Update(entity);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var entity = await _context.MatchingResultados
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingResultado.NotFound",
                Message = $"No se encontró el resultado de matching con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        _context.MatchingResultados.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static MatchingResultadoDto MapToDto(MatchingResultado entity)
    {
        object fortalezas = new List<object>();
        object brechas = new List<object>();
        object desglose = new List<object>();

        try { fortalezas = JsonSerializer.Deserialize<object>(entity.FortalezasJson ?? "[]") ?? new List<object>(); } catch { }
        try { brechas = JsonSerializer.Deserialize<object>(entity.BrechasJson ?? "[]") ?? new List<object>(); } catch { }
        try { desglose = JsonSerializer.Deserialize<object>(entity.DesgloseCriteriosJson ?? "[]") ?? new List<object>(); } catch { }

        return new MatchingResultadoDto
        {
            Id = entity.Id,
            MatchingEjecucionId = entity.MatchingEjecucionId,
            PostulanteId = entity.PostulanteId,
            Origen = entity.Origen,
            PorcentajeMatching = entity.PorcentajeMatching,
            Clasificacion = entity.Clasificacion,
            PosicionRanking = entity.PosicionRanking,
            PuntajeObtenido = entity.PuntajeObtenido,
            PuntajeMaximo = entity.PuntajeMaximo,
            Fortalezas = fortalezas,
            Brechas = brechas,
            DesgloseCriterios = desglose,
            TipoDescarte = entity.TipoDescarte,
            MotivoExclusion = entity.MotivoExclusion,
            CreatedDate = entity.CreatedDate
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

public class MatchingResultadoDto
{
    public int Id { get; set; }
    public int MatchingEjecucionId { get; set; }
    public int PostulanteId { get; set; }
    public string Origen { get; set; } = string.Empty;
    public decimal PorcentajeMatching { get; set; }
    public string Clasificacion { get; set; } = string.Empty;
    public int PosicionRanking { get; set; }
    public decimal PuntajeObtenido { get; set; }
    public decimal PuntajeMaximo { get; set; }
    public object Fortalezas { get; set; } = new List<object>();
    public object Brechas { get; set; } = new List<object>();
    public object DesgloseCriterios { get; set; } = new List<object>();
    public string? TipoDescarte { get; set; }
    public string? MotivoExclusion { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class MatchingResultadoSaveDto
{
    public int MatchingEjecucionId { get; set; }
    public int PostulanteId { get; set; }
    public string Origen { get; set; } = "BD_INTERNA";
    public decimal PorcentajeMatching { get; set; }
    public string Clasificacion { get; set; } = "AJUSTE_MEDIO";
    public int PosicionRanking { get; set; }
    public decimal PuntajeObtenido { get; set; }
    public decimal PuntajeMaximo { get; set; }
    public object? Fortalezas { get; set; }
    public object? Brechas { get; set; }
    public object? DesgloseCriterios { get; set; }
    public string? TipoDescarte { get; set; }
    public string? MotivoExclusion { get; set; }
}
