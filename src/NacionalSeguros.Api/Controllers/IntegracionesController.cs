using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dapper;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Contracts.Security;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/v1/integraciones")]
public class IntegracionesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIntegracionRepository _integracionRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IApiKeyAuditoriaRepository _auditoriaRepository;
    private readonly IHistorialApiKeyRepository _historialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IntegracionesController(
        ApplicationDbContext dbContext,
        IIntegracionRepository integracionRepository,
        IApiKeyRepository apiKeyRepository,
        IApiKeyAuditoriaRepository auditoriaRepository,
        IHistorialApiKeyRepository historialRepository,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _integracionRepository = integracionRepository;
        _apiKeyRepository = apiKeyRepository;
        _auditoriaRepository = auditoriaRepository;
        _historialRepository = historialRepository;
        _unitOfWork = unitOfWork;
    }

    private string GetCorrelationId()
    {
        if (HttpContext.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null)
        {
            return cid.ToString()!;
        }
        return Guid.NewGuid().ToString();
    }

    // ==========================================
    // 1. INTEGRACIONES (CLIENTES)
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = _dbContext.Set<Integracion>()
            .Where(i => !i.IsDeleted);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Nombre.Contains(search) || i.Codigo.Contains(search) || i.Responsable.Contains(search));
        }

        int totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(i => i.Nombre)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var integracion = await _integracionRepository.GetByIdAsync(id);
        if (integracion == null || integracion.IsDeleted)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Integración no encontrada.", CorrelationId = GetCorrelationId() });
        }
        return Ok(integracion);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearIntegracionRequest request)
    {
        var existente = await _integracionRepository.GetByCodigoAsync(request.Codigo);
        if (existente != null && !existente.IsDeleted)
        {
            return BadRequest(new ApiErrorDto { Code = "DUPLICATE_CODE", Message = "Ya existe una integración con ese código.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        var integracion = new Integracion(
            request.Nombre,
            request.Codigo,
            request.Descripcion,
            request.Tipo,
            request.Responsable,
            request.CorreoResponsable,
            userEmail
        );

        if (!string.IsNullOrEmpty(request.Observaciones))
        {
            integracion.Actualizar(
                request.Nombre,
                request.Descripcion,
                request.Tipo,
                request.Responsable,
                request.CorreoResponsable,
                request.Observaciones,
                userEmail
            );
        }

        await _integracionRepository.AddAsync(integracion);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = integracion.Id }, integracion);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] ActualizarIntegracionRequest request)
    {
        var integracion = await _integracionRepository.GetByIdAsync(id);
        if (integracion == null || integracion.IsDeleted)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Integración no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        integracion.Actualizar(
            request.Nombre,
            request.Descripcion,
            request.Tipo,
            request.Responsable,
            request.CorreoResponsable,
            request.Observaciones,
            userEmail
        );

        _integracionRepository.Update(integracion);
        await _unitOfWork.SaveChangesAsync();

        return Ok(integracion);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado([FromRoute] int id, [FromBody] CambiarEstadoIntegracionRequest request)
    {
        var integracion = await _integracionRepository.GetByIdAsync(id);
        if (integracion == null || integracion.IsDeleted)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Integración no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        if (request.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase))
        {
            integracion.Activar(userEmail);
        }
        else
        {
            integracion.Inactivar(userEmail);
        }

        _integracionRepository.Update(integracion);
        await _unitOfWork.SaveChangesAsync();

        return Ok(integracion);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var integracion = await _integracionRepository.GetByIdAsync(id);
        if (integracion == null || integracion.IsDeleted)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Integración no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";
        integracion.Delete(userEmail);

        // Inactivar todas las API Keys asociadas
        foreach (var key in integracion.ApiKeys.Where(k => !k.IsDeleted))
        {
            key.Revocar(userEmail);
            key.Delete(userEmail);
        }

        _integracionRepository.Update(integracion);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    // ==========================================
    // 2. GESTIÓN DE API KEYS
    // ==========================================

    [HttpGet("{id:int}/keys")]
    public async Task<IActionResult> ListarApiKeys([FromRoute] int id)
    {
        var keys = await _dbContext.Set<ApiKey>()
            .Where(k => k.IntegracionId == id && !k.IsDeleted)
            .Include(k => k.ApiKeyPermisos)
                .ThenInclude(kp => kp.Permiso)
            .OrderByDescending(k => k.FechaCreacion)
            .Select(k => new
            {
                k.Id,
                k.Nombre,
                k.Descripcion,
                ApiKeyHash = k.ApiKeyHash.Substring(0, Math.Min(10, k.ApiKeyHash.Length)) + "...",
                k.Estado,
                k.FechaCreacion,
                k.FechaExpiracion,
                k.UltimoUso,
                k.UltimaIP,
                k.Workflow,
                k.Observaciones,
                Permisos = k.ApiKeyPermisos.Select(kp => new { kp.Permiso.Id, kp.Permiso.Codigo, kp.Permiso.Nombre })
            })
            .ToListAsync();

        return Ok(keys);
    }

    [HttpPost("{id:int}/keys")]
    public async Task<IActionResult> GenerarApiKey([FromRoute] int id, [FromBody] GenerarApiKeyRequest request)
    {
        var integracion = await _integracionRepository.GetByIdAsync(id);
        if (integracion == null || integracion.IsDeleted)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Integración no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        // Generar API Key plana criptográficamente segura
        string plainKey = "NSIR_" + Guid.NewGuid().ToString("N");
        string keyHash = ComputeSha256Hash(plainKey);

        var apiKey = new ApiKey(
            request.Nombre,
            keyHash,
            request.Workflow,
            request.Descripcion,
            "Activo",
            request.FechaExpiracion,
            userEmail,
            permisos: string.Empty, // Los permisos se asocian en la tabla intermedia
            integracionId: id,
            observaciones: request.Observaciones
        );

        await _apiKeyRepository.AddAsync(apiKey);
        await _unitOfWork.SaveChangesAsync(); // Guardar primero para obtener el ApiKeyId

        // Asociar permisos
        if (request.PermisoIds != null && request.PermisoIds.Any())
        {
            var apiKeyPermisos = request.PermisoIds.Select(pid => new ApiKeyPermiso(apiKey.Id, pid));
            apiKey.ActualizarPermisos(apiKeyPermisos);
            await _unitOfWork.SaveChangesAsync();
        }

        // Registrar historial
        var historial = new HistorialApiKey(
            apiKey.Id,
            "Creado",
            userEmail,
            $"API Key creada para la integración {integracion.Nombre} y workflow {request.Workflow}."
        );
        await _historialRepository.AddAsync(historial);
        await _unitOfWork.SaveChangesAsync();

        // Retornar la API Key plana (esta es la única vez que se muestra)
        return CreatedAtAction(nameof(ListarApiKeys), new { id = id }, new
        {
            ApiKeyId = apiKey.Id,
            apiKey.Nombre,
            apiKey.Workflow,
            ApiKeyPlana = plainKey, // SE RETORNA SOLO UNA VEZ
            apiKey.Estado,
            apiKey.FechaCreacion,
            apiKey.FechaExpiracion
        });
    }

    [HttpPost("{integracionId:int}/keys/{keyId:long}/rotar")]
    public async Task<IActionResult> RotarApiKey([FromRoute] int integracionId, [FromRoute] long keyId, [FromBody] RotarApiKeyRequest request)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(keyId);
        if (apiKey == null || apiKey.IsDeleted || apiKey.IntegracionId != integracionId)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "API Key no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        // Generar nueva API Key plana
        string plainKey = "NSIR_" + Guid.NewGuid().ToString("N");
        string keyHash = ComputeSha256Hash(plainKey);

        string viejoHashPrefix = apiKey.ApiKeyHash.Substring(0, Math.Min(8, apiKey.ApiKeyHash.Length)) + "...";

        // Actualizar datos de API Key
        apiKey.Rotar(keyHash, request.FechaExpiracion, userEmail);
        _apiKeyRepository.Update(apiKey);

        // Registrar historial
        var historial = new HistorialApiKey(
            apiKey.Id,
            "Rotado",
            userEmail,
            $"API Key rotada. Clave anterior hash prefix: {viejoHashPrefix}."
        );
        await _historialRepository.AddAsync(historial);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            ApiKeyId = apiKey.Id,
            apiKey.Nombre,
            apiKey.Workflow,
            ApiKeyPlana = plainKey, // SE RETORNA SOLO UNA VEZ
            apiKey.Estado,
            apiKey.FechaCreacion,
            apiKey.FechaExpiracion
        });
    }

    [HttpPost("{integracionId:int}/keys/{keyId:long}/revocar")]
    public async Task<IActionResult> RevocarApiKey([FromRoute] int integracionId, [FromRoute] long keyId)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(keyId);
        if (apiKey == null || apiKey.IsDeleted || apiKey.IntegracionId != integracionId)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "API Key no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        apiKey.Revocar(userEmail);
        _apiKeyRepository.Update(apiKey);

        // Registrar historial
        var historial = new HistorialApiKey(
            apiKey.Id,
            "Revocado",
            userEmail,
            "API Key revocada permanentemente."
        );
        await _historialRepository.AddAsync(historial);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { ApiKeyId = apiKey.Id, apiKey.Estado });
    }

    [HttpPut("{integracionId:int}/keys/{keyId:long}/estado")]
    public async Task<IActionResult> CambiarEstadoApiKey([FromRoute] int integracionId, [FromRoute] long keyId, [FromBody] CambiarEstadoIntegracionRequest request)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(keyId);
        if (apiKey == null || apiKey.IsDeleted || apiKey.IntegracionId != integracionId)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "API Key no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        apiKey.CambiarEstado(request.Estado, userEmail);
        _apiKeyRepository.Update(apiKey);

        // Registrar historial
        var historial = new HistorialApiKey(
            apiKey.Id,
            request.Estado == "Activo" ? "Habilitado" : "Deshabilitado",
            userEmail,
            $"API Key cambiada de estado a {request.Estado}."
        );
        await _historialRepository.AddAsync(historial);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { ApiKeyId = apiKey.Id, apiKey.Estado });
    }

    [HttpDelete("{integracionId:int}/keys/{keyId:long}")]
    public async Task<IActionResult> EliminarApiKey([FromRoute] int integracionId, [FromRoute] long keyId)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(keyId);
        if (apiKey == null || apiKey.IsDeleted || apiKey.IntegracionId != integracionId)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "API Key no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.Identity?.Name ?? "system@nacionalseguros.com.bo";

        apiKey.Delete(userEmail);
        _apiKeyRepository.Update(apiKey);

        // Registrar historial
        var historial = new HistorialApiKey(
            apiKey.Id,
            "Eliminado",
            userEmail,
            "API Key eliminada lógicamente."
        );
        await _historialRepository.AddAsync(historial);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    // ==========================================
    // 3. AUDITORÍA E HISTORIAL
    // ==========================================

    [HttpGet("auditoria")]
    public async Task<IActionResult> ListarAuditoria(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var query = _dbContext.Set<ApiKeyAuditoria>().AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.IntegracionNombre!.Contains(search) 
                                  || a.ApiKeyNombre!.Contains(search) 
                                  || a.Endpoint.Contains(search) 
                                  || a.IP!.Contains(search) 
                                  || a.CorrelationId!.Contains(search));
        }

        int totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("keys/{keyId:long}/historial")]
    public async Task<IActionResult> ObtenerHistorialApiKey([FromRoute] long keyId)
    {
        var historial = await _historialRepository.GetByApiKeyIdAsync(keyId);
        return Ok(historial);
    }

    // ==========================================
    // 4. PERMISOS DEL SISTEMA
    // ==========================================

    [HttpGet("permisos")]
    public async Task<IActionResult> ListarPermisos()
    {
        var permisos = await _dbContext.Permisos
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.Id, p.Codigo, p.Nombre })
            .ToListAsync();
        return Ok(permisos);
    }

    // ==========================================
    // HELPERS
    // ==========================================

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}

// ==========================================
// REQUEST CLASSES
// ==========================================

public class CrearIntegracionRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Responsable { get; set; } = string.Empty;
    public string CorreoResponsable { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class ActualizarIntegracionRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Responsable { get; set; } = string.Empty;
    public string CorreoResponsable { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class CambiarEstadoIntegracionRequest
{
    public string Estado { get; set; } = "Activo";
}

public class GenerarApiKeyRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Workflow { get; set; } = string.Empty;
    public DateTime? FechaExpiracion { get; set; }
    public List<int> PermisoIds { get; set; } = new();
    public string? Observaciones { get; set; }
}

public class RotarApiKeyRequest
{
    public DateTime? FechaExpiracion { get; set; }
}
