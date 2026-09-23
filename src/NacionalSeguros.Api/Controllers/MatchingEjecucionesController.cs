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

using NacionalSeguros.Contracts.Responses;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Auditor,Decisor,Reclutador,RRHH")]
[Route("api/v1/matching-ejecuciones")]
public class MatchingEjecucionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MatchingEjecucionesController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MatchingEjecucionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? perfilCargoId)
    {
        var query = _context.MatchingEjecuciones
            .Include(p => p.Estado)
            .AsNoTracking();

        if (perfilCargoId.HasValue)
        {
            query = query.Where(p => p.PerfilCargoId == perfilCargoId.Value);
        }

        var list = await query.ToListAsync();
        var dtos = list.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    [HttpGet("perfiles-habilitados")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PerfilHabilitadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPerfilesHabilitados()
    {
        var query = _context.MatchingEjecuciones
            .Include(m => m.PerfilCargo)
                .ThenInclude(p => p.Solicitud)
                    .ThenInclude(s => s.Solicitante)
                        .ThenInclude(u => u.Area)
            .Include(m => m.Estado)
            .Where(m => m.EstadoId == 37 || m.EstadoId == 38 || m.EstadoId == 39)
            .OrderByDescending(m => m.Id);

        var list = await query.ToListAsync();

        var dtos = list
            .GroupBy(m => m.PerfilCargoId)
            .Select(g => g.First())
            .Select(m => new PerfilHabilitadoDto
            {
                PerfilCargoId = m.PerfilCargoId,
                MatchingEjecucionId = m.Id,
                CodigoMatching = m.CodigoMatching,
                CodigoPerfil = $"PRF-{m.PerfilCargo?.Solicitud?.Codigo ?? m.PerfilCargoId.ToString()}",
                Cargo = m.PerfilCargo?.Solicitud?.Cargo ?? "Sin Cargo",
                Area = m.PerfilCargo?.Solicitud?.Solicitante?.Area?.Nombre ?? "Sin Área",
                EstadoId = m.EstadoId,
                EstadoCodigo = m.Estado?.Codigo ?? (m.EstadoId == 37 ? "MATCH-PEN" : m.EstadoId == 38 ? "MATCH-REV" : "MATCH-ARP"),
                EstadoNombre = m.Estado?.Nombre ?? (m.EstadoId == 37 ? "Pendiente de Generación" : m.EstadoId == 38 ? "En revisión RRHH" : "Estrategia Aprobada"),
                FechaEstrategia = m.CreatedDate
            }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MatchingEjecucionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var item = await _context.MatchingEjecuciones.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (item == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(MatchingEjecucionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] MatchingEjecucionSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        // Validar Perfil de Cargo
        var perfil = await _context.PerfilesCargo
            .FirstOrDefaultAsync(p => p.Id == dto.PerfilCargoId && !p.IsDeleted);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PerfilCargo.NotFound",
                Message = $"No se encontró el perfil de cargo con ID {dto.PerfilCargoId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Obtener PerfilEstructurado correspondiente a la Solicitud del PerfilCargo
        var perfilEstructurado = await _context.PerfilEstructurados
            .FirstOrDefaultAsync(pe => pe.SolicitudId == perfil.SolicitudId);

        if (perfilEstructurado == null)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = "PerfilEstructurado.NotFound",
                Message = $"No se encontró un perfil estructurado relacionado al perfil de cargo {dto.PerfilCargoId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        var fuentesJson = dto.FuentesConsultadas != null ? JsonSerializer.Serialize(dto.FuentesConsultadas) : "[]";
        var descartesJson = dto.ResumenDescartes != null ? JsonSerializer.Serialize(dto.ResumenDescartes) : "[]";
        var parametrosJson = dto.ParametrosMatching != null ? JsonSerializer.Serialize(dto.ParametrosMatching) : "{}";

        // Verificar si ya existe una ejecución para este PerfilCargoId (Upsert)
        var entity = await _context.MatchingEjecuciones
            .FirstOrDefaultAsync(e => e.PerfilCargoId == dto.PerfilCargoId);

        if (entity != null)
        {
            var entry = _context.Entry(entity);
            entry.Property(p => p.PerfilEstructuradoId).CurrentValue = perfilEstructurado.Id;
            entry.Property(p => p.VersionPerfil).CurrentValue = dto.VersionPerfil;
            entry.Property(p => p.EstadoId).CurrentValue = dto.EstadoId;
            entry.Property(p => p.TotalEvaluados).CurrentValue = dto.TotalEvaluados;
            entry.Property(p => p.TotalInternosEvaluados).CurrentValue = dto.TotalInternosEvaluados;
            entry.Property(p => p.TotalHistoricosEvaluados).CurrentValue = dto.TotalHistoricosEvaluados;
            entry.Property(p => p.TotalMatchAlto).CurrentValue = dto.TotalMatchAlto;
            entry.Property(p => p.TotalMatchMedio).CurrentValue = dto.TotalMatchMedio;
            entry.Property(p => p.TotalMatchBajo).CurrentValue = dto.TotalMatchBajo;
            entry.Property(p => p.TotalDescartados).CurrentValue = dto.TotalDescartados;
            entry.Property(p => p.TotalPotenciales).CurrentValue = dto.TotalPotenciales;
            entry.Property(p => p.CompatibilidadPromedio).CurrentValue = dto.CompatibilidadPromedio;
            entry.Property(p => p.EstrategiaRecomendada).CurrentValue = dto.EstrategiaRecomendada;
            entry.Property(p => p.NivelConfianza).CurrentValue = dto.NivelConfianza;
            entry.Property(p => p.Justificacion).CurrentValue = dto.Justificacion;
            entry.Property(p => p.FuentesConsultadasJson).CurrentValue = fuentesJson;
            entry.Property(p => p.ResumenDescartesJson).CurrentValue = descartesJson;
            entry.Property(p => p.ParametrosMatchingJson).CurrentValue = parametrosJson;
            entry.Property(p => p.FechaInicio).CurrentValue = dto.FechaInicio == default ? DateTime.UtcNow : dto.FechaInicio;
            entry.Property(p => p.FechaFin).CurrentValue = dto.FechaFin;
            entry.Property(p => p.MensajeError).CurrentValue = dto.MensajeError;

            _context.MatchingEjecuciones.Update(entity);
        }
        else
        {
            string codigoMatching = string.Empty;
            if (!string.IsNullOrEmpty(perfil.Codigo))
            {
                if (perfil.Codigo.StartsWith("PRF-SOL-"))
                {
                    codigoMatching = perfil.Codigo.Replace("PRF-SOL-", "ME-");
                }
                else
                {
                    var parts = perfil.Codigo.Split('-');
                    if (parts.Length >= 3)
                    {
                        codigoMatching = $"ME-{parts[parts.Length - 2]}-{parts[parts.Length - 1]}";
                    }
                    else
                    {
                        codigoMatching = $"ME-{perfil.Codigo}";
                    }
                }
            }
            else
            {
                codigoMatching = $"ME-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 4)}";
            }

            entity = new MatchingEjecucion(
                perfilCargoId: dto.PerfilCargoId,
                perfilEstructuradoId: perfilEstructurado.Id,
                versionPerfil: dto.VersionPerfil,
                estadoId: dto.EstadoId,
                totalEvaluados: dto.TotalEvaluados,
                totalInternosEvaluados: dto.TotalInternosEvaluados,
                totalHistoricosEvaluados: dto.TotalHistoricosEvaluados,
                totalMatchAlto: dto.TotalMatchAlto,
                totalMatchMedio: dto.TotalMatchMedio,
                totalMatchBajo: dto.TotalMatchBajo,
                totalDescartados: dto.TotalDescartados,
                totalPotenciales: dto.TotalPotenciales,
                fuentesConsultadasJson: fuentesJson,
                resumenDescartesJson: descartesJson,
                parametrosMatchingJson: parametrosJson,
                fechaInicio: dto.FechaInicio == default ? DateTime.UtcNow : dto.FechaInicio,
                compatibilidadPromedio: dto.CompatibilidadPromedio,
                estrategiaRecomendada: dto.EstrategiaRecomendada,
                nivelConfianza: dto.NivelConfianza,
                justificacion: dto.Justificacion,
                fechaFin: dto.FechaFin,
                mensajeError: dto.MensajeError
            );
            entity.SetCodigoMatching(codigoMatching);

            _context.MatchingEjecuciones.Add(entity);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MatchingEjecucionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] MatchingEjecucionSaveDto dto)
    {
        if (dto == null) return BadRequest("Los datos son requeridos.");

        var entity = await _context.MatchingEjecuciones
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Si se cambia PerfilCargoId, volver a buscar PerfilEstructuradoId
        int perfilEstructuradoId = entity.PerfilEstructuradoId;
        if (entity.PerfilCargoId != dto.PerfilCargoId)
        {
            var perfil = await _context.PerfilesCargo
                .FirstOrDefaultAsync(p => p.Id == dto.PerfilCargoId && !p.IsDeleted);

            if (perfil == null)
            {
                return NotFound(new ApiErrorDto
                {
                    Code = "PerfilCargo.NotFound",
                    Message = $"No se encontró el perfil de cargo con ID {dto.PerfilCargoId}.",
                    CorrelationId = GetCorrelationId()
                });
            }

            var perfilEstructurado = await _context.PerfilEstructurados
                .FirstOrDefaultAsync(pe => pe.SolicitudId == perfil.SolicitudId);

            if (perfilEstructurado == null)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "PerfilEstructurado.NotFound",
                    Message = $"No se encontró un perfil estructurado relacionado al perfil de cargo {dto.PerfilCargoId}.",
                    CorrelationId = GetCorrelationId()
                });
            }

            perfilEstructuradoId = perfilEstructurado.Id;
        }

        var fuentesJson = dto.FuentesConsultadas != null ? JsonSerializer.Serialize(dto.FuentesConsultadas) : "[]";
        var descartesJson = dto.ResumenDescartes != null ? JsonSerializer.Serialize(dto.ResumenDescartes) : "[]";
        var parametrosJson = dto.ParametrosMatching != null ? JsonSerializer.Serialize(dto.ParametrosMatching) : "{}";

        // Usamos los métodos de negocio de la entidad
        if (dto.EstadoId == 39)
        {
            entity.FinalizarConExito(
                totalEvaluados: dto.TotalEvaluados,
                totalInternosEvaluados: dto.TotalInternosEvaluados,
                totalHistoricosEvaluados: dto.TotalHistoricosEvaluados,
                totalMatchAlto: dto.TotalMatchAlto,
                totalMatchMedio: dto.TotalMatchMedio,
                totalMatchBajo: dto.TotalMatchBajo,
                totalDescartados: dto.TotalDescartados,
                totalPotenciales: dto.TotalPotenciales,
                compatibilidadPromedio: dto.CompatibilidadPromedio ?? 0,
                estrategiaRecomendada: dto.EstrategiaRecomendada ?? "EXTERNA",
                nivelConfianza: dto.NivelConfianza ?? 0,
                justificacion: dto.Justificacion ?? string.Empty,
                fuentesConsultadasJson: fuentesJson,
                resumenDescartesJson: descartesJson,
                parametrosMatchingJson: parametrosJson,
                estadoId: dto.EstadoId
            );
        }
        else
        {
            var entry = _context.Entry(entity);
            entry.Property(p => p.PerfilCargoId).CurrentValue = dto.PerfilCargoId;
            entry.Property(p => p.PerfilEstructuradoId).CurrentValue = perfilEstructuradoId;
            entry.Property(p => p.VersionPerfil).CurrentValue = dto.VersionPerfil;
            entry.Property(p => p.EstadoId).CurrentValue = dto.EstadoId;
            entry.Property(p => p.TotalEvaluados).CurrentValue = dto.TotalEvaluados;
            entry.Property(p => p.TotalInternosEvaluados).CurrentValue = dto.TotalInternosEvaluados;
            entry.Property(p => p.TotalHistoricosEvaluados).CurrentValue = dto.TotalHistoricosEvaluados;
            entry.Property(p => p.TotalMatchAlto).CurrentValue = dto.TotalMatchAlto;
            entry.Property(p => p.TotalMatchMedio).CurrentValue = dto.TotalMatchMedio;
            entry.Property(p => p.TotalMatchBajo).CurrentValue = dto.TotalMatchBajo;
            entry.Property(p => p.TotalDescartados).CurrentValue = dto.TotalDescartados;
            entry.Property(p => p.TotalPotenciales).CurrentValue = dto.TotalPotenciales;
            entry.Property(p => p.CompatibilidadPromedio).CurrentValue = dto.CompatibilidadPromedio;
            entry.Property(p => p.EstrategiaRecomendada).CurrentValue = dto.EstrategiaRecomendada;
            entry.Property(p => p.NivelConfianza).CurrentValue = dto.NivelConfianza;
            entry.Property(p => p.Justificacion).CurrentValue = dto.Justificacion;
            entry.Property(p => p.FuentesConsultadasJson).CurrentValue = fuentesJson;
            entry.Property(p => p.ResumenDescartesJson).CurrentValue = descartesJson;
            entry.Property(p => p.ParametrosMatchingJson).CurrentValue = parametrosJson;
            entry.Property(p => p.FechaInicio).CurrentValue = dto.FechaInicio;
            entry.Property(p => p.FechaFin).CurrentValue = dto.FechaFin;
            entry.Property(p => p.MensajeError).CurrentValue = dto.MensajeError;
        }

        _context.MatchingEjecuciones.Update(entity);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var entity = await _context.MatchingEjecuciones
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        _context.MatchingEjecuciones.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static MatchingEjecucionDto MapToDto(MatchingEjecucion entity)
    {
        object fuentes = new List<object>();
        object descartes = new List<object>();
        object parametros = new object();

        try
        {
            if (!string.IsNullOrWhiteSpace(entity.FuentesConsultadasJson))
            {
                using var doc = JsonDocument.Parse(entity.FuentesConsultadasJson);
                fuentes = doc.RootElement.Clone();
            }
        }
        catch { }

        try
        {
            if (!string.IsNullOrWhiteSpace(entity.ResumenDescartesJson))
            {
                using var doc = JsonDocument.Parse(entity.ResumenDescartesJson);
                descartes = doc.RootElement.Clone();
            }
        }
        catch { }

        try
        {
            if (!string.IsNullOrWhiteSpace(entity.ParametrosMatchingJson))
            {
                using var doc = JsonDocument.Parse(entity.ParametrosMatchingJson);
                parametros = doc.RootElement.Clone();
            }
        }
        catch { }

        string estadoText = entity.Estado?.Nombre ?? entity.EstadoId switch
        {
            37 => "Pendiente de Generación",
            38 => "En revisión RRHH",
            39 => "Estrategia Aprobada",
            1 => "EnRevisionRRHHEstrategia",
            2 => "AprobadaEstrategia",
            3 => "ObservadaEstrategia",
            _ => "EnRevisionRRHHEstrategia"
        };

        string estadoCodigoText = entity.Estado?.Codigo ?? entity.EstadoId switch
        {
            37 => "MATCH-PEN",
            38 => "MATCH-REV",
            39 => "MATCH-ARP",
            _ => "MATCH-REV"
        };

        return new MatchingEjecucionDto
        {
            Id = entity.Id,
            CodigoMatching = entity.CodigoMatching,
            PerfilCargoId = entity.PerfilCargoId,
            PerfilEstructuradoId = entity.PerfilEstructuradoId,
            VersionPerfil = entity.VersionPerfil,
            EstadoId = entity.EstadoId,
            Estado = estadoText,
            EstadoCodigo = estadoCodigoText,
            TotalEvaluados = entity.TotalEvaluados,
            TotalInternosEvaluados = entity.TotalInternosEvaluados,
            TotalHistoricosEvaluados = entity.TotalHistoricosEvaluados,
            TotalMatchAlto = entity.TotalMatchAlto,
            TotalMatchMedio = entity.TotalMatchMedio,
            TotalMatchBajo = entity.TotalMatchBajo,
            TotalDescartados = entity.TotalDescartados,
            TotalPotenciales = entity.TotalPotenciales,
            CompatibilidadPromedio = entity.CompatibilidadPromedio,
            EstrategiaRecomendada = entity.EstrategiaRecomendada,
            NivelConfianza = entity.NivelConfianza,
            Justificacion = entity.Justificacion,
            FuentesConsultadas = fuentes,
            ResumenDescartes = descartes,
            ParametrosMatching = parametros,
            FechaInicio = entity.FechaInicio,
            FechaFin = entity.FechaFin,
            CreatedDate = entity.CreatedDate,
            MensajeError = entity.MensajeError
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

public class MatchingEjecucionDto
{
    public int Id { get; set; }
    public string CodigoMatching { get; set; } = string.Empty;
    public int PerfilCargoId { get; set; }
    public int PerfilEstructuradoId { get; set; }
    public int VersionPerfil { get; set; }
    public int EstadoId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoCodigo { get; set; } = string.Empty;
    public int TotalEvaluados { get; set; }
    public int TotalInternosEvaluados { get; set; }
    public int TotalHistoricosEvaluados { get; set; }
    public int TotalMatchAlto { get; set; }
    public int TotalMatchMedio { get; set; }
    public int TotalMatchBajo { get; set; }
    public int TotalDescartados { get; set; }
    public int TotalPotenciales { get; set; }
    public decimal? CompatibilidadPromedio { get; set; }
    public string? EstrategiaRecomendada { get; set; }
    public decimal? NivelConfianza { get; set; }
    public string? Justificacion { get; set; }
    public object FuentesConsultadas { get; set; } = new List<object>();
    public object ResumenDescartes { get; set; } = new List<object>();
    public object ParametrosMatching { get; set; } = new object();
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? MensajeError { get; set; }
}

public class MatchingEjecucionSaveDto
{
    public int PerfilCargoId { get; set; }
    public int VersionPerfil { get; set; }
    public int EstadoId { get; set; } = 37;
    public int TotalEvaluados { get; set; }
    public int TotalInternosEvaluados { get; set; }
    public int TotalHistoricosEvaluados { get; set; }
    public int TotalMatchAlto { get; set; }
    public int TotalMatchMedio { get; set; }
    public int TotalMatchBajo { get; set; }
    public int TotalDescartados { get; set; }
    public int TotalPotenciales { get; set; }
    public decimal? CompatibilidadPromedio { get; set; }
    public string? EstrategiaRecomendada { get; set; }
    public decimal? NivelConfianza { get; set; }
    public string? Justificacion { get; set; }
    public object? FuentesConsultadas { get; set; }
    public object? ResumenDescartes { get; set; }
    public object? ParametrosMatching { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? MensajeError { get; set; }
}
