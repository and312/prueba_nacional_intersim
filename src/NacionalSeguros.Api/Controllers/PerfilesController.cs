using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Application.Perfiles.Commands;
using NacionalSeguros.Application.Perfiles.Queries;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1/perfiles")]
[Authorize]
public class PerfilesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ApplicationDbContext _context;

    public PerfilesController(ISender sender, ApplicationDbContext context)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private async Task<Usuario?> GetCurrentUserAsync()
    {
        string email = User.FindFirst("email")?.Value ??
                       User.FindFirst(ClaimTypes.Email)?.Value ??
                       (User.Identity?.Name != null && User.Identity.Name.Contains("@") ? User.Identity.Name : "") ??
                       "";
        if (string.IsNullOrEmpty(email)) return null;
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == email);
    }

    private bool IsAdminOrRrhh()
    {
        return User.IsInRole("Administrador") || User.IsInRole("RRHH");
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PerfilListResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool soloMisPerfiles = false)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        int? solicitanteId = (IsAdminOrRrhh() && !soloMisPerfiles) ? null : currentUser.Id;

        var query = new ListarPerfilesQuery(solicitanteId);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PerfilDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        int? solicitanteId = IsAdminOrRrhh() ? null : currentUser.Id;

        var result = await _sender.Send(new ObtenerPerfilDetalleQuery(id, solicitanteId));

        if (result.IsFailure)
        {
            if (result.Error.Code == "Perfil.Forbidden")
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = result.Error.Message });

            return NotFound(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}/resumen")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarResumen([FromRoute] int id, [FromBody] ResumenEjecutivoUpdateRequest request)
    {
        var command = new ActualizarResumenCommand(id, request.SolicitudId, request.ResumenEjecutivoRol, User.Identity?.Name ?? "RRHH", EsAutomatizacion: false);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPost("{id:int}/generar-resumen")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarResumen([FromRoute] int id)
    {
        var perfil = await _context.PerfilesCargo
            .Include(p => p.Estado)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"El perfil con ID {id} no existe." });
        }

        var webhookUrl = NacionalSeguros.Shared.Primitives.WebhookSettings.PerfilVacanteResumenUrl;
        var payload = new
        {
            perfilCargoId = perfil.Id,
            solicitudId = perfil.SolicitudId,
            codigoPerfil = perfil.Codigo,
            version = perfil.Version
        };

        try
        {
            using var httpClient = new HttpClient();
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            System.Console.WriteLine($"[Webhook Resumen] POST {webhookUrl}");
            System.Console.WriteLine($"[Webhook Resumen] Payload: {json}");
            
            var response = await httpClient.PostAsync(webhookUrl, content);
            
            System.Console.WriteLine($"[Webhook Resumen] Response: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new ApiErrorDto 
                { 
                    Code = "Webhook.Error", 
                    Message = $"El webhook de generación de resumen falló con estado {response.StatusCode}.", 
                    Detail = errorBody 
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorDto 
            { 
                Code = "Webhook.Exception", 
                Message = "Ocurrió un error al conectar con el webhook de generación de resumen.", 
                Detail = ex.Message 
            });
        }

        return Ok(new { Message = "Resumen ejecutivo generado exitosamente." });
    }

    [HttpPost("{id:int}/observaciones")]
    [Authorize(Roles = "Solicitante,RRHH,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarObservacion([FromRoute] int id, [FromBody] PerfilObservacionCreateRequest request)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var command = new RegistrarObservacionPerfilCommand(id, request.TipoObservacionId, request.Comentario, currentUser.Id, currentUser.Nombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Perfil.Forbidden")
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = result.Error.Message });

            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPost("{id:int}/enviar-observaciones")]
    [Authorize(Roles = "Solicitante,Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarObservaciones([FromRoute] int id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var command = new EnviarObservacionesSolicitanteCommand(id, currentUser.Id, currentUser.Nombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Perfil.Forbidden")
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = result.Error.Message });

            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpGet("{perfilCargoId:int}/observaciones")]
    [ProducesResponseType(typeof(IEnumerable<PerfilObservacionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerObservaciones([FromRoute] int perfilCargoId)
    {
        var perfil = await _context.Set<PerfilCargo>().FirstOrDefaultAsync(p => p.Id == perfilCargoId);
        if (perfil == null) return NotFound();

        var list = await _context.Set<PerfilObservacion>()
            .Include(o => o.TipoObservacion)
            .Include(o => o.UsuarioSolicitante)
            .Include(o => o.AtendidaPorUsuario)
            .Where(o => o.PerfilCargo.SolicitudId == perfil.SolicitudId)
            .Select(o => new PerfilObservacionResponseDto(
                o.Id,
                o.PerfilCargoId,
                o.TipoObservacionId,
                o.TipoObservacion.Nombre,
                o.Comentario,
                o.UsuarioSolicitanteId,
                o.UsuarioSolicitante.Nombre,
                o.NumeroIteracion,
                o.EstadoObservacion,
                o.CreatedDate,
                o.AtendidaPorUsuarioId,
                o.AtendidaPorUsuario != null ? o.AtendidaPorUsuario.Nombre : null,
                o.FechaAtencion
            ))
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("{id:int}/aprobar-solicitante")]
    [Authorize(Roles = "Solicitante,RRHH,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AprobarSolicitante([FromRoute] int id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var command = new AprobarSolicitantePerfilCommand(id, currentUser.Id, currentUser.Nombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Perfil.Forbidden")
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = result.Error.Message });

            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPost("{id:int}/atender-observaciones")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtenderObservaciones([FromRoute] int id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var command = new AtenderObservacionesPerfilCommand(id, currentUser.Id, currentUser.Nombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPost("observaciones/{observacionId:int}/atender")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtenderObservacionId([FromRoute] int observacionId)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var obs = await _context.Set<PerfilObservacion>().FirstOrDefaultAsync(o => o.Id == observacionId);
        if (obs == null) return NotFound();

        obs.Atender(currentUser.Id);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("observaciones/{observacionId:int}/reabrir")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReabrirObservacionId([FromRoute] int observacionId)
    {
        var obs = await _context.Set<PerfilObservacion>().FirstOrDefaultAsync(o => o.Id == observacionId);
        if (obs == null) return NotFound();

        obs.Reabrir();
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("observaciones/{observacionId:int}")]
    [Authorize(Roles = "Solicitante,Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarObservacionId([FromRoute] int observacionId)
    {
        var obs = await _context.Set<PerfilObservacion>().FirstOrDefaultAsync(o => o.Id == observacionId);
        if (obs == null) return NotFound();

        _context.Set<PerfilObservacion>().Remove(obs);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{id:int}/enviar-area")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarArea([FromRoute] int id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var command = new EnviarAreaPerfilCommand(id, currentUser.Id, currentUser.Nombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        try
        {
            var perfil = await _context.Set<PerfilCargo>().FirstOrDefaultAsync(p => p.Id == id);
            if (perfil != null)
            {
                var webhookUrl = NacionalSeguros.Shared.Primitives.WebhookSettings.EnviarAreaWebhookUrl;
                var payload = new
                {
                    solicitudId = perfil.SolicitudId,
                    perfilCargoId = perfil.Id
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        var json = System.Text.Json.JsonSerializer.Serialize(payload);
                        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        await httpClient.PostAsync(webhookUrl, content);
                    }
                    catch (Exception)
                    {
                        // Omitir errores de webhook asíncrono
                    }
                });
            }
        }
        catch (Exception)
        {
            // Omitir excepciones preventivamente
        }

        return Ok();
    }

    [HttpPost("{id:int}/aprobar-final")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AprobarFinal([FromRoute] int id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var command = new AprobarFinalPerfilCommand(id, currentUser.Id, currentUser.Nombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpGet("documentos/{solicitudId:int}/{tipo}/descarga")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarDocumento([FromRoute] int solicitudId, [FromRoute] string tipo)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud == null) return NotFound();

        // Si no es Admin ni RRHH, validar que sea el solicitante de la solicitud
        if (!IsAdminOrRrhh() && solicitud.SolicitanteId != currentUser.Id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para descargar documentos de esta solicitud." });
        }

        var doc = await _context.SolicitudDocumentos
            .FirstOrDefaultAsync(d => d.SolicitudId == solicitudId && d.TipoDocumento == tipo);
        if (doc == null) return NotFound();

        if (!string.IsNullOrEmpty(doc.PublicUrl) && doc.PublicUrl.StartsWith("http"))
        {
            return Redirect(doc.PublicUrl);
        }

        if (System.IO.File.Exists(doc.StoragePath))
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(doc.StoragePath);
            return File(bytes, "application/pdf", doc.FileName);
        }

        return NotFound();
    }

    private async Task<IActionResult> DescargarPdfInterno(int id, string tipoDocumento)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var perfil = await _context.Set<PerfilCargo>()
            .Include(p => p.Solicitud)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (perfil == null) return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = "Perfil no encontrado." });

        // Si no es Admin ni RRHH, validar que sea el solicitante de la solicitud relacionada
        if (!IsAdminOrRrhh() && perfil.Solicitud.SolicitanteId != currentUser.Id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para descargar el documento de este perfil." });
        }

        var doc = await _context.SolicitudDocumentos
            .FirstOrDefaultAsync(d => d.SolicitudId == perfil.SolicitudId && d.TipoDocumento == tipoDocumento);
        if (doc == null) return NotFound(new ApiErrorDto { Code = "Documento.NotFound", Message = "El documento solicitado no está registrado." });

        if (!string.IsNullOrEmpty(doc.PublicUrl) && doc.PublicUrl.StartsWith("http"))
        {
            return Redirect(doc.PublicUrl);
        }

        string localPath = doc.StoragePath;
        if (!System.IO.File.Exists(localPath))
        {
            if (!string.IsNullOrEmpty(doc.PublicUrl))
            {
                var relPath = doc.PublicUrl.TrimStart('/');
                var candidate = System.IO.Path.Combine(Directory.GetCurrentDirectory(), relPath);
                if (System.IO.File.Exists(candidate)) localPath = candidate;
            }

            if (!System.IO.File.Exists(localPath) && !string.IsNullOrEmpty(doc.StoragePath))
            {
                var filenameOnly = System.IO.Path.GetFileName(doc.StoragePath);
                var candidate = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "uploads", filenameOnly);
                if (System.IO.File.Exists(candidate)) localPath = candidate;
            }
        }

        if (System.IO.File.Exists(localPath))
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(localPath);
            var downloadName = doc.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? doc.FileName : $"{doc.FileName}.pdf";
            return File(bytes, "application/pdf", downloadName);
        }

        return NotFound(new ApiErrorDto { Code = "Documento.FileNotFound", Message = "El archivo físico PDF no fue encontrado en el servidor." });
    }

    [HttpGet("{id:int}/pdf-estructurado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DescargarPdfEstructurado([FromRoute] int id)
    {
        return DescargarPdfInterno(id, "PERFIL_ESTRUCTURADO_PDF");
    }

    [HttpGet("{id:int}/pdf-resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DescargarPdfResumen([FromRoute] int id)
    {
        return DescargarPdfInterno(id, "RESUMEN_EJECUTIVO_PDF");
    }
}
