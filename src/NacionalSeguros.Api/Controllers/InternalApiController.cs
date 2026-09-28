using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dapper;
using MediatR;
using NacionalSeguros.Application.Solicitudes.Commands.ActualizarSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.CancelarSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.CrearSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.TransitarSolicitud;
using NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudById;
using NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudStateHistory;
using NacionalSeguros.Application.Solicitudes.Queries.ListSolicitudesKanban;
using NacionalSeguros.Application.Solicitudes.Commands.GuardarResumen;
using NacionalSeguros.Application.Solicitudes.Commands.RegistrarDocumento;
using NacionalSeguros.Application.Solicitudes.Queries.GetDocumentos;
using NacionalSeguros.Application.Perfiles.Commands;
using NacionalSeguros.Application.Perfiles.Queries;
using NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.GenerarPerfil;
using NacionalSeguros.Application.Perfiles.Commands.ObservarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfilPorId;
using NacionalSeguros.Application.Vacantes.Commands.CrearVacante;
using NacionalSeguros.Application.Vacantes.Commands.PublicarVacante;
using NacionalSeguros.Application.Vacantes.Commands.PausarVacante;
using NacionalSeguros.Application.Vacantes.Commands.ReanudarVacante;
using NacionalSeguros.Application.Vacantes.Commands.CerrarVacante;
using NacionalSeguros.Application.Vacantes.Commands.CancelarVacante;
using NacionalSeguros.Application.Vacantes.Queries.BuscarVacantes;
using NacionalSeguros.Application.Vacantes.Queries.ObtenerVacantePorId;
using NacionalSeguros.Application.PerfilesEstructurados.Commands.CrearPerfilEstructurado;
using NacionalSeguros.Application.PerfilesEstructurados.Commands.ActualizarPerfilEstructurado;
using NacionalSeguros.Application.PerfilesEstructurados.Queries;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/internal")]
public class InternalApiController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ApplicationDbContext _dbContext;

    public InternalApiController(ISender sender, ApplicationDbContext dbContext)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    private bool UserHasPermission(string permission)
    {
        return User.Claims.Any(c => c.Type == "permission" && c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));
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
    // 1. SOLICITUDES
    // ==========================================

    [HttpGet("solicitudes")]
    [ProducesResponseType(typeof(PagedSolicitudesResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarSolicitudes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? estadoId = null,
        [FromQuery] string? search = null,
        [FromQuery] int? requestedByUserId = null,
        [FromQuery] int? solicitanteId = null)
    {
        if (!UserHasPermission("solicitudes.read") && !UserHasPermission("solicitudes.listar"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para listar solicitudes.", CorrelationId = GetCorrelationId() });
        }

        int? targetSolicitanteId = requestedByUserId ?? solicitanteId;

        string userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var query = new ListSolicitudesKanbanQuery(pageNumber, pageSize, estadoId, search, userEmail, false, targetSolicitanteId);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al listar solicitudes.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpGet("solicitudes/{id:int}")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerSolicitudPorId([FromRoute] int id)
    {
        if (!UserHasPermission("solicitudes.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer solicitudes.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var query = new GetSolicitudByIdQuery(id, userEmail);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Solicitud no encontrada.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPost("solicitudes")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudCreateDto request)
    {
        if (!UserHasPermission("solicitudes.crear") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para crear solicitudes.", CorrelationId = GetCorrelationId() });
        }

        string workflow = User.FindFirst("workflow")?.Value ?? "Desconocido";
        long? apiKeyId = null;
        if (long.TryParse(User.FindFirst("ApiKeyId")?.Value, out long idVal))
        {
            apiKeyId = idVal;
        }

        string createdBy = "system@nacionalseguros.com.bo";
        if (request.UsuarioId.HasValue)
        {
            var user = await _dbContext.Usuarios.FindAsync(request.UsuarioId.Value);
            if (user != null)
            {
                createdBy = user.Correo;
            }
        }

        var command = new CrearSolicitudCommand(
            request,
            createdBy,
            UsuarioId: request.UsuarioId,
            CanalOrigen: "N8N",
            WorkflowOrigen: workflow,
            ApiKeyId: apiKeyId,
            CorrelationId: Guid.TryParse(GetCorrelationId(), out var cid) ? cid : (Guid?)null
        );

        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al crear la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(ObtenerSolicitudPorId), new { id = result.Value.SolicitudId }, result.Value);
    }

    [HttpPut("solicitudes/{id:int}")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarSolicitud([FromRoute] int id, [FromBody] SolicitudUpdateDto request)
    {
        if (!UserHasPermission("solicitudes.editar") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para actualizar solicitudes.", CorrelationId = GetCorrelationId() });
        }

        string userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                           ?? User.Identity?.Name 
                           ?? "system@nacionalseguros.com.bo";
        var command = new ActualizarSolicitudCommand(id, request, userEmail, CanalOrigen: "N8N");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al actualizar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPost("solicitudes/{id:int}/aprobar")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AprobarSolicitud([FromRoute] int id, [FromBody] AprobarSolicitudRequest request)
    {
        if (!UserHasPermission("solicitudes.aprobar") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para aprobar solicitudes.", CorrelationId = GetCorrelationId() });
        }

        var decisor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Correo == "rrhh@nacionalseguros.com.bo" 
            || u.Correo == "system@nacionalseguros.com.bo" 
            || u.Correo == "admin@nacionalseguros.com.bo" 
            || u.Correo == "admin@nacionalseguros.com")
            ?? await _dbContext.Usuarios.FirstOrDefaultAsync();

        if (decisor == null)
        {
            return BadRequest(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = "No se encontró ningún usuario en la base de datos.", CorrelationId = GetCorrelationId() });
        }

        var command = new TransitarSolicitudEstadoCommand(id, "SOL-APR", request.Observaciones ?? "Aprobación automática vía N8N", decisor.Id, decisor.Correo);
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al aprobar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPost("solicitudes/{id:int}/rechazar")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RechazarSolicitud([FromRoute] int id, [FromBody] RechazarSolicitudRequest request)
    {
        if (!UserHasPermission("solicitudes.rechazar") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para rechazar solicitudes.", CorrelationId = GetCorrelationId() });
        }

        var decisor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Correo == "rrhh@nacionalseguros.com.bo" 
            || u.Correo == "system@nacionalseguros.com.bo" 
            || u.Correo == "admin@nacionalseguros.com.bo" 
            || u.Correo == "admin@nacionalseguros.com")
            ?? await _dbContext.Usuarios.FirstOrDefaultAsync();

        if (decisor == null)
        {
            return BadRequest(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = "No se encontró ningún usuario en la base de datos.", CorrelationId = GetCorrelationId() });
        }

        var command = new TransitarSolicitudEstadoCommand(id, "SOL-RECH", request.Justificacion ?? "Rechazo automático vía N8N", decisor.Id, decisor.Correo);
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al rechazar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpGet("solicitudes/{id:int}/historial")]
    [ProducesResponseType(typeof(IEnumerable<StateHistoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerSolicitudHistorial([FromRoute] int id)
    {
        if (!UserHasPermission("solicitudes.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer el historial.", CorrelationId = GetCorrelationId() });
        }

        var query = new GetSolicitudStateHistoryQuery(id);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al obtener historial.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPut("solicitudes/{id:int}/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GuardarResumen([FromRoute] int id, [FromBody] SolicitudResumenRequestDto request)
    {
        if (!UserHasPermission("solicitudes.editar") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para actualizar solicitudes.", CorrelationId = GetCorrelationId() });
        }

        var command = new GuardarResumenCommand(id, request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al guardar el resumen estructurado.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    [HttpGet("solicitudes/{id:int}/documentos")]
    [ProducesResponseType(typeof(List<SolicitudDocumentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerDocumentos([FromRoute] int id, [FromQuery] string? tipoDocumento = null)
    {
        if (!UserHasPermission("solicitudes.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer solicitudes.", CorrelationId = GetCorrelationId() });
        }

        var query = new GetDocumentosQuery(id, tipoDocumento);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al obtener los documentos.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPost("solicitudes/{id:int}/documentos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarDocumento([FromRoute] int id, [FromBody] SolicitudDocumentoDto request)
    {
        if (!UserHasPermission("solicitudes.editar") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para actualizar solicitudes.", CorrelationId = GetCorrelationId() });
        }

        var command = new RegistrarDocumentoCommand(id, request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al registrar el documento.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    // ==========================================
    // 2. OBSERVACIONES
    // ==========================================

    [HttpPost("observaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearObservacion([FromBody] CrearObservacionRequest request, [FromServices] ISolicitudRepository solicitudRepository, [FromServices] IUsuarioRepository usuarioRepository, [FromServices] IUnitOfWork unitOfWork)
    {
        if (!UserHasPermission("observaciones.crear") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para crear observaciones.", CorrelationId = GetCorrelationId() });
        }

        var solicitud = await solicitudRepository.GetByIdAsync(request.SolicitudId);
        if (solicitud == null) return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Solicitud no encontrada.", CorrelationId = GetCorrelationId() });

        var usuario = await usuarioRepository.GetByCorreoAsync("rrhh@nacionalseguros.com.bo")
                      ?? await usuarioRepository.GetByCorreoAsync("system@nacionalseguros.com.bo")
                      ?? await usuarioRepository.GetByCorreoAsync("admin@nacionalseguros.com.bo")
                      ?? await usuarioRepository.GetByCorreoAsync("admin@nacionalseguros.com");

        if (usuario == null)
        {
            var allUsers = await usuarioRepository.GetPagedAsync(1, 1, null);
            usuario = allUsers.Items.FirstOrDefault();
        }

        if (usuario == null) return BadRequest(new ApiErrorDto { Code = "ERROR", Message = "No hay usuario del sistema.", CorrelationId = GetCorrelationId() });

        solicitud.AgregarComentario(usuario.Id, request.Texto);
        await unitOfWork.SaveChangesAsync();

        return Ok(new { SolicitudId = solicitud.Id, Texto = request.Texto, CreadoPor = usuario.Correo, Fecha = DateTime.UtcNow });
    }

    [HttpGet("observaciones/{solicitudId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerObservaciones([FromRoute] int solicitudId)
    {
        if (!UserHasPermission("solicitudes.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer observaciones.", CorrelationId = GetCorrelationId() });
        }

        var comentarios = await _dbContext.SolicitudComentarios
            .Where(c => c.SolicitudId == solicitudId)
            .OrderByDescending(c => c.Fecha)
            .Select(c => new
            {
                ComentarioId = c.Id,
                c.SolicitudId,
                c.Texto,
                c.Fecha,
                Usuario = c.Usuario.Correo
            })
            .ToListAsync();

        return Ok(comentarios);
    }

    // ==========================================
    // 3. PERFILES
    // ==========================================

    [HttpPost("perfiles/generar")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> GenerarPerfil([FromBody] GenerarPerfilRequest request)
    {
        if (!UserHasPermission("perfiles.generar") && !UserHasPermission("perfiles.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para generar perfiles.", CorrelationId = GetCorrelationId() });
        }

        var command = new GenerarPerfilCommand(request.SolicitudId, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al generar perfil.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Accepted();
    }

    [HttpPost("perfiles/callback-creacion")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CallbackPerfilCreacion([FromBody] PerfilCreacionCallbackRequest request)
    {
        if (!UserHasPermission("perfiles.callback") && !UserHasPermission("perfiles.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para ejecutar el callback de perfiles.", CorrelationId = GetCorrelationId() });
        }

        var command = new CrearPerfilCargoCommand(request, "AgentePerfil");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error en callback de perfil.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpGet("perfiles/{id:int}")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPerfilPorId([FromRoute] int id)
    {
        if (!UserHasPermission("perfiles.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer perfiles.", CorrelationId = GetCorrelationId() });
        }

        var query = new ObtenerPerfilPorIdQuery(id);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Perfil no encontrado.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpGet("perfiles-estructurados/{solicitudId:int}")]
    [ProducesResponseType(typeof(PerfilEstructuradoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerPerfilEstructurado([FromRoute] int solicitudId)
    {
        if (!UserHasPermission("perfiles.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer perfiles estructurados.", CorrelationId = GetCorrelationId() });
        }

        var query = new ObtenerPerfilEstructuradoPorSolicitudIdQuery(solicitudId);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            if (result.Error.Code == "PerfilEstructurado.NotFound")
            {
                return Ok(null);
            }

            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Perfil estructurado no encontrado.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPut("perfiles-estructurados/{solicitudId:int}")]
    [ProducesResponseType(typeof(PerfilEstructuradoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActualizarPerfilEstructurado([FromRoute] int solicitudId, [FromBody] PerfilEstructuradoInputDto request)
    {
        if (!UserHasPermission("perfiles.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para actualizar perfiles estructurados.", CorrelationId = GetCorrelationId() });
        }

        // Si la SolicitudId de la ruta difiere de la del body, usar la de la ruta
        if (request.SolicitudId != solicitudId)
        {
            request.SolicitudId = solicitudId;
        }

        // Verificar si ya existe el perfil estructurado para hacer update, o si se debe crear (Upsert)
        var existing = await _dbContext.Set<PerfilEstructurado>().AnyAsync(pe => pe.SolicitudId == solicitudId);
        
        IRequest<Result<PerfilEstructuradoResponseDto>> command;
        if (existing)
        {
            command = new ActualizarPerfilEstructuradoCommand(null, solicitudId, request, "AgentePerfil");
        }
        else
        {
            command = new CrearPerfilEstructuradoCommand(request, "AgentePerfil");
        }

        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Solicitud.NotFound" || result.Error.Code == "PerfilEstructurado.NotFound")
            {
                return Ok(new { message = "Perfil no encontrado" });
            }

            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = existing ? "Error en la actualización del perfil estructurado." : "Error en la creación del perfil estructurado.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(new
        {
            message = existing ? "Actualizado con éxito" : "Creado con éxito",
            data = result.Value
        });
    }

    [HttpPost("perfiles/{id:int}/aprobar")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AprobarPerfil([FromRoute] int id)
    {
        if (!UserHasPermission("perfiles.aprobar") && !UserHasPermission("perfiles.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para aprobar perfiles.", CorrelationId = GetCorrelationId() });
        }

        var command = new AprobarPerfilCargoCommand(id, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al aprobar perfil.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPost("perfiles/{id:int}/observar")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObservarPerfil([FromRoute] int id, [FromBody] ObservarPerfilRequest request)
    {
        if (!UserHasPermission("perfiles.observar") && !UserHasPermission("perfiles.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para observar perfiles.", CorrelationId = GetCorrelationId() });
        }

        var command = new ObservarPerfilCargoCommand(id, request.Justificacion, "rrhh@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al observar perfil.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    // ==========================================
    // 4. ESTRATEGIAS
    // ==========================================

    [HttpGet("estrategias")]
    public IActionResult ObtenerEstrategias()
    {
        if (!UserHasPermission("estrategias.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer estrategias.", CorrelationId = GetCorrelationId() });
        }

        var estrategias = new[]
        {
            new { Codigo = "EST-LK", Nombre = "Búsqueda Activa en LinkedIn", Estado = "Activo", Canal = "LinkedIn" },
            new { Codigo = "EST-WEB", Nombre = "Portal de Empleo Nacional", Estado = "Activo", Canal = "Web" },
            new { Codigo = "EST-REF", Nombre = "Programa de Referidos Internos", Estado = "Activo", Canal = "Referido" },
            new { Codigo = "EST-CAB", Nombre = "Caza de Talentos Directa (Headhunting)", Estado = "Activo", Canal = "Manual" }
        };
        return Ok(estrategias);
    }

    // ==========================================
    // 5. VACANTES
    // ==========================================

    [HttpGet("vacantes")]
    public async Task<IActionResult> ListarVacantes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!UserHasPermission("vacantes.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer vacantes.", CorrelationId = GetCorrelationId() });
        }

        var query = new BuscarVacantesQuery(pageNumber, pageSize);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al buscar vacantes.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpGet("vacantes/{id:int}")]
    public async Task<IActionResult> ObtenerVacantePorId([FromRoute] int id)
    {
        if (!UserHasPermission("vacantes.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer vacantes.", CorrelationId = GetCorrelationId() });
        }

        var query = new ObtenerVacantePorIdQuery(id);
        var result = await _sender.Send(query);
        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Vacante no encontrada.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok(result.Value);
    }

    [HttpPost("vacantes")]
    public async Task<IActionResult> CrearVacante([FromBody] CrearVacanteCommand command)
    {
        if (!UserHasPermission("vacantes.crear") && !UserHasPermission("vacantes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para crear vacantes.", CorrelationId = GetCorrelationId() });
        }

        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al crear la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }
        return CreatedAtAction(nameof(ObtenerVacantePorId), new { id = result.Value.VacanteId }, result.Value);
    }

    [HttpPost("vacantes/{id:int}/publicar")]
    public async Task<IActionResult> PublicarVacante([FromRoute] int id, [FromBody] PublicarVacanteRequest request)
    {
        if (!UserHasPermission("vacantes.publicar") && !UserHasPermission("vacantes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para publicar vacantes.", CorrelationId = GetCorrelationId() });
        }

        var command = new PublicarVacanteCommand(id, request.CanalIds, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al publicar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    [HttpPost("vacantes/{id:int}/pausar")]
    public async Task<IActionResult> PausarVacante([FromRoute] int id, [FromBody] PausarVacanteRequest request)
    {
        if (!UserHasPermission("vacantes.pausar") && !UserHasPermission("vacantes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para pausar vacantes.", CorrelationId = GetCorrelationId() });
        }

        var command = new PausarVacanteCommand(id, request.Justificacion, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al pausar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    [HttpPost("vacantes/{id:int}/reanudar")]
    public async Task<IActionResult> ReanudarVacante([FromRoute] int id, [FromBody] ReanudarVacanteRequest request)
    {
        if (!UserHasPermission("vacantes.reanudar") && !UserHasPermission("vacantes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para reanudar vacantes.", CorrelationId = GetCorrelationId() });
        }

        var command = new ReanudarVacanteCommand(id, request.Justificacion, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al reanudar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    [HttpPost("vacantes/{id:int}/cerrar")]
    public async Task<IActionResult> CerrarVacante([FromRoute] int id, [FromBody] CerrarVacanteRequest request)
    {
        if (!UserHasPermission("vacantes.cerrar") && !UserHasPermission("vacantes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para cerrar vacantes.", CorrelationId = GetCorrelationId() });
        }

        var command = new CerrarVacanteCommand(id, request.PostulanteContratadoId, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al cerrar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    [HttpPost("vacantes/{id:int}/cancelar")]
    public async Task<IActionResult> CancelarVacante([FromRoute] int id, [FromBody] CancelarVacanteRequest request)
    {
        if (!UserHasPermission("vacantes.cancelar") && !UserHasPermission("vacantes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para cancelar vacantes.", CorrelationId = GetCorrelationId() });
        }

        var command = new CancelarVacanteCommand(id, request.MotivoCancelacionCodigo, "system@nacionalseguros.com.bo");
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al cancelar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }
        return Ok();
    }

    // ==========================================
    // 6. EVENTOS (HISTORIAL DE ESTADOS)
    // ==========================================

    [HttpGet("eventos")]
    public async Task<IActionResult> ObtenerEventos([FromQuery] string? entidad = null, [FromQuery] int? entidadId = null)
    {
        if (!UserHasPermission("eventos.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer la bitácora de eventos.", CorrelationId = GetCorrelationId() });
        }

        var query = _dbContext.StateHistories
            .Include(h => h.EstadoAnterior)
            .Include(h => h.EstadoNuevo)
            .Include(h => h.Usuario)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(entidad))
        {
            query = query.Where(h => h.Entidad == entidad);
        }

        if (entidadId.HasValue)
        {
            query = query.Where(h => h.EntidadId == entidadId.Value);
        }

        var logs = await query
            .OrderByDescending(h => h.Fecha)
            .Take(100)
            .Select(h => new
            {
                HistoryId = h.Id,
                h.Entidad,
                h.EntidadId,
                EstadoAnterior = h.EstadoAnterior != null ? h.EstadoAnterior.Nombre : null,
                EstadoNuevo = h.EstadoNuevo != null ? h.EstadoNuevo.Nombre : null,
                Usuario = h.Usuario != null ? h.Usuario.Correo : "Sistema",
                h.Comentario,
                h.Fecha,
                h.CorrelationId
            })
            .ToListAsync();

        return Ok(logs);
    }

    // ==========================================
    // 7. NOTIFICACIONES
    // ==========================================

    [HttpGet("notificaciones")]
    public async Task<IActionResult> ObtenerNotificaciones()
    {
        if (!UserHasPermission("notificaciones.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer el log de notificaciones.", CorrelationId = GetCorrelationId() });
        }

        using var connection = _dbContext.Database.GetDbConnection();
        var sql = @"SELECT TOP 100 NotificationLogId, Fecha, Destinatario, TipoCanal, Asunto, EstadoEnvio, CorrelationId 
                    FROM NotificationLogs 
                    ORDER BY Fecha DESC";

        var logs = await connection.QueryAsync(sql);
        return Ok(logs);
    }

    // ==========================================
    // 8. AUDITORÍA
    // ==========================================

    [HttpGet("auditoria")]
    public async Task<IActionResult> ObtenerAuditoria([FromQuery] string? modulo = null, [FromQuery] string? canal = null)
    {
        if (!UserHasPermission("auditoria.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer la auditoría.", CorrelationId = GetCorrelationId() });
        }

        var query = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(modulo))
        {
            query = query.Where(a => a.Modulo == modulo);
        }

        if (!string.IsNullOrEmpty(canal))
        {
            query = query.Where(a => a.Canal == canal);
        }

        var logs = await query
            .OrderByDescending(a => a.FechaHoraUTC)
            .Take(100)
            .Select(a => new
            {
                a.Id,
                a.FechaHoraUTC,
                a.UsuarioId,
                a.UsuarioNombre,
                a.Rol,
                a.Modulo,
                a.Entidad,
                a.EntidadId,
                a.Accion,
                a.Canal,
                a.CorrelationId
            })
            .ToListAsync();

        return Ok(logs);
    }

    // ==========================================
    // 9. CANALES AUTORIZADOS (VALIDACIÓN DE WHATSAPP / N8N)
    // ==========================================

    [HttpPost("authorized-channels/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidarCanalAutorizado([FromBody] ValidarCanalRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ChannelIdentifier))
        {
            return BadRequest(new { Message = "El identificador del canal es requerido." });
        }

        var identifier = request.ChannelIdentifier.Trim();
        var searchDigits = new string(identifier.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(searchDigits))
        {
            return BadRequest(new { Message = "Identificador de canal no válido." });
        }

        // Buscar el usuario en la base de datos comparando su número de teléfono en memoria para evitar errores de traducción de EF Core
        var users = await _dbContext.Usuarios
            .Include(u => u.Area)
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permisos)
            .Where(u => !u.IsDeleted && u.Telefono != null && u.Estado == Domain.Enums.UsuarioEstado.Activo)
            .ToListAsync();

        var usuario = users.FirstOrDefault(u =>
        {
            var userDigits = new string(u.Telefono!.Where(char.IsDigit).ToArray());
            return userDigits.Contains(searchDigits) || searchDigits.Contains(userDigits);
        });

        if (usuario == null)
        {
            return Ok(new
            {
                IsAuthorized = false,
                Reason = "Usuario no encontrado o no activo para el identificador proporcionado.",
                UserId = (string?)null,
                FullName = (string?)null,
                Email = (string?)null,
                Department = (string?)null,
                JobTitle = (string?)null,
                Roles = new List<string>(),
                Permissions = new List<string>(),
                ChannelType = request.ChannelType,
                NormalizedIdentifier = (string?)null
            });
        }

        // Registrar la última interacción del usuario con n8n
        usuario.RegistrarInteraccionN8N();
        await _dbContext.SaveChangesAsync();

        // Formatear identificador normalizado
        var normalizedIdentifier = identifier.StartsWith("+") ? identifier : "+591" + identifier;

        // Generar un Guid determinista a partir del ID del usuario
        string userGuid;
        if (!string.IsNullOrEmpty(usuario.ActiveDirectoryId))
        {
            userGuid = usuario.ActiveDirectoryId;
        }
        else
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"usuario-{usuario.Id}"));
                userGuid = new Guid(hash).ToString();
            }
        }

        return Ok(new
        {
            IsAuthorized = true,
            Reason = (string?)null,
            UserId = usuario.Id.ToString(),
            FullName = usuario.Nombre,
            Email = usuario.Correo,
            Department = usuario.Area?.Nombre ?? string.Empty,
            JobTitle = usuario.Cargo ?? string.Empty,
            Roles = usuario.Roles.Select(r => r.Nombre).ToList(),
            Permissions = usuario.Roles.SelectMany(r => r.Permisos).Select(p => p.Codigo).Distinct().ToList(),
            ChannelType = request.ChannelType,
            NormalizedIdentifier = normalizedIdentifier
        });
    }

    // ==========================================
    // 10. n8n INTEGRATION ENDPOINTS
    // ==========================================

    private async Task<Usuario?> ResolveUsuarioByGuidAsync(string guidStr)
    {
        bool isPlaceholder = string.IsNullOrWhiteSpace(guidStr) || 
                             guidStr.Contains("{{") || 
                             guidStr.Equals("3fa85f64-5717-4562-b3fc-2c963f66afa6", StringComparison.OrdinalIgnoreCase);

        if (!isPlaceholder)
        {
            // Try parsing as integer ID first (for direct database UsuarioId support)
            if (int.TryParse(guidStr, out int userId))
            {
                var userById = await _dbContext.Usuarios.Include(u => u.Area).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
                if (userById != null) return userById;
            }

            // Try exact match on ActiveDirectoryId
            var user = await _dbContext.Usuarios.Include(u => u.Area).FirstOrDefaultAsync(u => u.ActiveDirectoryId == guidStr && !u.IsDeleted);
            if (user != null) return user;

            // Try MD5 Guid matching
            var allUsers = await _dbContext.Usuarios.Include(u => u.Area).Where(u => !u.IsDeleted).ToListAsync();
            foreach (var u in allUsers)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"usuario-{u.Id}"));
                    string generatedGuid = new Guid(hash).ToString();
                    if (string.Equals(generatedGuid, guidStr, StringComparison.OrdinalIgnoreCase))
                    {
                        return u;
                    }
                }
            }
        }

        // Fallback: Resolve a default active user to prevent 400 bad request errors
        var defaultUser = await _dbContext.Usuarios.Include(u => u.Area)
            .FirstOrDefaultAsync(u => u.Id == 6 && !u.IsDeleted) ??
            await _dbContext.Usuarios.Include(u => u.Area)
            .FirstOrDefaultAsync(u => u.Correo == "rrhh@nacionalseguros.com.bo" && !u.IsDeleted) ??
            await _dbContext.Usuarios.Include(u => u.Area)
            .FirstOrDefaultAsync(u => !u.IsDeleted);

        return defaultUser;
    }

    private string GetUserGuid(Usuario usuario)
    {
        return usuario.Id.ToString();
    }

    private async Task<VacancyRequestResponseDto> MapToVacancyRequestResponseDto(Solicitud s)
    {
        var solicitante = await _dbContext.Usuarios.Include(u => u.Area).FirstOrDefaultAsync(u => u.Id == s.SolicitanteId);
        string requestedByUsuarioId = solicitante != null ? GetUserGuid(solicitante) : string.Empty;

        var decisor = s.DecisorId.HasValue ? await _dbContext.Usuarios.FindAsync(s.DecisorId.Value) : null;
        string decisionMakerName = decisor != null ? decisor.Nombre : string.Empty;

        var regional = s.RegionalId.HasValue ? await _dbContext.Regionales.FindAsync(s.RegionalId.Value) : null;
        var tipo = s.TipoSolicitudId.HasValue ? await _dbContext.TiposSolicitud.FindAsync(s.TipoSolicitudId.Value) : null;
        var mod = s.ModalidadTrabajoId.HasValue ? await _dbContext.ModalidadesTrabajo.FindAsync(s.ModalidadTrabajoId.Value) : null;

        return new VacancyRequestResponseDto
        {
            Id = s.Id,
            RequestCode = s.Codigo ?? string.Empty,
            Title = s.Cargo,
            CargoId = s.CargoId,
            RequestedByUsuarioId = requestedByUsuarioId,
            RequestedByUserId = requestedByUsuarioId,
            Reason = s.Motivo ?? string.Empty,
            VacancyCount = s.CantidadVacantes ?? 0,
            Seniority = s.Seniority,
            MainFunctions = s.Funciones,
            Channel = s.CanalOrigen ?? "BACKOFFICE",
            DecisionMaker = decisionMakerName,
            Priority = s.Prioridad,
            Comments = s.Observaciones ?? string.Empty,
            Status = s.Estado?.Nombre ?? string.Empty,

            Regional = regional != null ? new CatalogValueDto(regional.Id, regional.Codigo, regional.Nombre) : null,
            TipoSolicitud = tipo != null ? new CatalogValueDto(tipo.Id, tipo.Codigo, tipo.Nombre) : null,
            ModalidadTrabajo = mod != null ? new CatalogValueDto(mod.Id, mod.Codigo, mod.Nombre) : null,

            // Perfil
            ObjetivoCargo = s.ObjetivoCargo ?? string.Empty,
            ExperienciaMinima = s.ExperienciaMinima ?? string.Empty,
            ConocimientosTecnicos = s.ConocimientosTecnicos ?? string.Empty,

            // Recomendados
            FormacionAcademica = s.FormacionAcademica,
            ExperienciaIndispensable = s.ExperienciaIndispensable,
            HerramientasSistemas = s.HerramientasSistemas,
            CompetenciasClave = s.CompetenciasClave,
            DisponibilidadRequerida = s.DisponibilidadRequerida,
            CriteriosExcluyentes = s.CriteriosExcluyentes,
            CriteriosDeseables = s.CriteriosDeseables
        };
    }

    [HttpGet("vacancy-requests/fields")]
    public async Task<IActionResult> GetVacancyRequestFields()
    {
        var regionales = await _dbContext.Regionales
            .Where(r => !r.IsDeleted && r.Estado == "Activo")
            .Select(r => new { id = r.Id, nombre = r.Nombre })
            .ToListAsync();

        var tipos = await _dbContext.TiposSolicitud
            .Where(t => !t.IsDeleted && t.Estado == "Activo")
            .Select(t => new { id = t.Id, nombre = t.Nombre })
            .ToListAsync();

        var modalidades = await _dbContext.ModalidadesTrabajo
            .Where(m => !m.IsDeleted && m.Estado == "Activo")
            .Select(m => new { id = m.Id, nombre = m.Nombre })
            .ToListAsync();

        var response = new
        {
            obligatorios = new object[]
            {
                new { campo = "requestedByUsuarioId", nombre = "Solicitante responsable", tipo = "entero", origen = "interno", orden = 1 },
                new { campo = "title", nombre = "Cargo requerido", tipo = "texto", orden = 2 },
                new { campo = "regionalId", nombre = "Regional", tipo = "catalogo", orden = 3, opciones = regionales },
                new { campo = "vacancyCount", nombre = "Cantidad de vacantes", tipo = "entero", valorPorDefecto = 1, minimo = 1, orden = 4 },
                new { campo = "tipoSolicitudId", nombre = "Tipo de solicitud", tipo = "catalogo", orden = 5, opciones = tipos },
                new { campo = "reason", nombre = "Motivo de la vacante", tipo = "texto", orden = 6 },
                new { campo = "workModeId", nombre = "Modalidad de trabajo", tipo = "catalogo", orden = 7, opciones = modalidades },
                new {
                    campo = "seniority",
                    nombre = "Seniority",
                    tipo = "lista",
                    orden = 8,
                    opciones = new[]
                    {
                        new { valor = "Junior", nombre = "Junior" },
                        new { valor = "SemiSenior", nombre = "Semi Senior" },
                        new { valor = "Senior", nombre = "Senior" },
                        new { valor = "EspecialistaJefe", nombre = "Especialista / Jefe" }
                    }
                },
                new {
                    campo = "priority",
                    nombre = "Prioridad",
                    tipo = "lista",
                    orden = 9,
                    opciones = new[]
                    {
                        new { valor = "Baja", nombre = "Baja" },
                        new { valor = "Media", nombre = "Media" },
                        new { valor = "Alta", nombre = "Alta" }
                    }
                },
                new { campo = "objetivoCargo", nombre = "Objetivo principal del cargo", tipo = "texto", orden = 10 },
                new { campo = "experienciaMinima", nombre = "Experiencia mínima requerida", tipo = "texto", orden = 11 },
                new { campo = "conocimientosTecnicos", nombre = "Conocimientos técnicos requeridos", tipo = "texto", orden = 12 },
                new { campo = "mainFunctions", nombre = "Funciones principales", tipo = "texto", orden = 13 }
            },
            recomendados = new object[]
            {
                new { campo = "formacionAcademica", nombre = "Formación académica requerida", tipo = "texto", orden = 14 },
                new { campo = "experienciaIndispensable", nombre = "Experiencia específica indispensable", tipo = "texto", orden = 15 },
                new { campo = "herramientasSistemas", nombre = "Herramientas o sistemas requeridos", tipo = "texto", orden = 16 },
                new { campo = "competenciasClave", nombre = "Competencias clave del perfil", tipo = "texto", orden = 17 },
                new { campo = "disponibilidadRequerida", nombre = "Disponibilidad requerida", tipo = "texto", orden = 18 },
                new { campo = "criteriosExcluyentes", nombre = "Criterios excluyentes", tipo = "texto", orden = 19 },
                new { campo = "criteriosDeseables", nombre = "Criterios deseables", tipo = "texto", orden = 20 }
            },
            internos = new object[]
            {
                new { campo = "channel", origen = "workflow" },
                new { campo = "workflowOrigen", origen = "workflow" }
            }
        };

        return Ok(response);
    }

    [HttpGet("solicitudes/{solicitudId:int}/completitud")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSolicitudCompletitud([FromRoute] int solicitudId)
    {
        if (!UserHasPermission("solicitudes.read") && !UserHasPermission("perfiles.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer la completitud de solicitudes.", CorrelationId = GetCorrelationId() });
        }

        var solicitud = await _dbContext.Solicitudes
            .Include(s => s.Regional)
            .Include(s => s.TipoSolicitud)
            .Include(s => s.ModalidadTrabajo)
            .FirstOrDefaultAsync(s => s.Id == solicitudId && !s.IsDeleted);

        if (solicitud == null)
        {
            return NotFound(new ApiErrorDto { Code = "Solicitud.NotFound", Message = $"La solicitud con ID {solicitudId} no existe.", CorrelationId = GetCorrelationId() });
        }

        // Determinar campos obligatorios y su cumplimiento
        var obligatorios = new List<object>
        {
            new { key = "cargo", label = "Cargo requerido", cumplido = !string.IsNullOrWhiteSpace(solicitud.Cargo) },
            new { key = "regional", label = "Regional", cumplido = solicitud.RegionalId.HasValue && solicitud.Regional != null },
            new { key = "cantidadVacantes", label = "Cantidad de vacantes", cumplido = solicitud.CantidadVacantes.HasValue && solicitud.CantidadVacantes > 0 },
            new { key = "tipoSolicitud", label = "Tipo de solicitud", cumplido = solicitud.TipoSolicitudId.HasValue && solicitud.TipoSolicitud != null },
            new { key = "motivo", label = "Motivo de la vacante", cumplido = !string.IsNullOrWhiteSpace(solicitud.Motivo) },
            new { key = "modalidadTrabajo", label = "Modalidad de trabajo", cumplido = solicitud.ModalidadTrabajoId.HasValue && solicitud.ModalidadTrabajo != null },
            new { key = "seniority", label = "Seniority", cumplido = !string.IsNullOrWhiteSpace(solicitud.Seniority) },
            new { key = "prioridad", label = "Prioridad", cumplido = !string.IsNullOrWhiteSpace(solicitud.Prioridad) },
            new { key = "objetivoCargo", label = "Objetivo principal del cargo", cumplido = !string.IsNullOrWhiteSpace(solicitud.ObjetivoCargo) },
            new { key = "experienciaMinima", label = "Experiencia mínima requerida", cumplido = !string.IsNullOrWhiteSpace(solicitud.ExperienciaMinima) },
            new { key = "conocimientosTecnicos", label = "Conocimientos técnicos requeridos", cumplido = !string.IsNullOrWhiteSpace(solicitud.ConocimientosTecnicos) },
            new { key = "funciones", label = "Funciones principales", cumplido = !string.IsNullOrWhiteSpace(solicitud.Funciones) }
        };

        // Determinar campos opcionales y su cumplimiento
        var opcionales = new List<object>
        {
            new { key = "formacionAcademica", label = "Formación académica requerida", cumplido = !string.IsNullOrWhiteSpace(solicitud.FormacionAcademica) },
            new { key = "experienciaIndispensable", label = "Experiencia específica indispensable", cumplido = !string.IsNullOrWhiteSpace(solicitud.ExperienciaIndispensable) },
            new { key = "herramientasSistemas", label = "Herramientas o sistemas requeridos", cumplido = !string.IsNullOrWhiteSpace(solicitud.HerramientasSistemas) },
            new { key = "competenciasClave", label = "Competencias clave del perfil", cumplido = !string.IsNullOrWhiteSpace(solicitud.CompetenciasClave) },
            new { key = "disponibilidadRequerida", label = "Disponibilidad requerida", cumplido = !string.IsNullOrWhiteSpace(solicitud.DisponibilidadRequerida) },
            new { key = "criteriosExcluyentes", label = "Criterios excluyentes", cumplido = !string.IsNullOrWhiteSpace(solicitud.CriteriosExcluyentes) },
            new { key = "criteriosDeseables", label = "Criterios deseables", cumplido = !string.IsNullOrWhiteSpace(solicitud.CriteriosDeseables) }
        };

        // Calcular completitud
        int totalCampos = obligatorios.Count + opcionales.Count;
        int camposDetectados = obligatorios.Count(o => ((dynamic)o).cumplido) + opcionales.Count(o => ((dynamic)o).cumplido);
        double completitudPorcentaje = totalCampos > 0 ? Math.Round((double)camposDetectados / totalCampos * 100, 2) : 0;

        var result = new
        {
            solicitud = new
            {
                solicitudId = solicitud.Id,
                codigo = solicitud.Codigo,
                solicitanteId = solicitud.SolicitanteId,
                cargo = solicitud.Cargo,
                regional = solicitud.Regional?.Nombre,
                cantidadVacantes = solicitud.CantidadVacantes,
                tipoSolicitud = solicitud.TipoSolicitud?.Nombre,
                motivo = solicitud.Motivo,
                modalidadTrabajo = solicitud.ModalidadTrabajo?.Nombre,
                seniority = solicitud.Seniority,
                prioridad = solicitud.Prioridad,
                objetivoCargo = solicitud.ObjetivoCargo,
                experienciaMinima = solicitud.ExperienciaMinima,
                conocimientosTecnicos = solicitud.ConocimientosTecnicos,
                funciones = solicitud.Funciones,
                formacionAcademica = solicitud.FormacionAcademica,
                experienciaIndispensable = solicitud.ExperienciaIndispensable,
                herramientasSistemas = solicitud.HerramientasSistemas,
                competenciasClave = solicitud.CompetenciasClave,
                disponibilidadRequerida = solicitud.DisponibilidadRequerida,
                criteriosExcluyentes = solicitud.CriteriosExcluyentes,
                criteriosDeseables = solicitud.CriteriosDeseables
            },
            completitud = new
            {
                camposEsperados = totalCampos,
                camposDetectados = camposDetectados,
                completitudPorcentaje = completitudPorcentaje,
                camposObligatorios = obligatorios,
                camposOpcionales = opcionales
            }
        };

        return Ok(result);
    }

    // A. VACANCY REQUESTS
    [HttpPost("vacancy-requests")]
    [ProducesResponseType(typeof(VacancyRequestResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearVacancyRequest([FromBody] VacancyRequestCreateDto request)
    {
        string userKey = !string.IsNullOrEmpty(request.RequestedByUsuarioId) 
            ? request.RequestedByUsuarioId 
            : request.RequestedByUserId;
        var user = await ResolveUsuarioByGuidAsync(userKey);
        if (user == null)
        {
            return BadRequest(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = $"El usuario solicitante '{userKey}' no existe.", CorrelationId = GetCorrelationId() });
        }

        var createDto = new SolicitudCreateDto(
            Cargo: request.Title,
            RegionalId: request.RegionalId,
            CantidadVacantes: request.VacancyCount,
            TipoSolicitudId: request.TipoSolicitudId,
            Motivo: request.Reason,
            ObjetivoCargo: request.ObjetivoCargo,
            FormacionAcademica: request.FormacionAcademica,
            ExperienciaMinima: request.ExperienciaMinima,
            ExperienciaIndispensable: request.ExperienciaIndispensable,
            ConocimientosTecnicos: request.ConocimientosTecnicos,
            HerramientasSistemas: request.HerramientasSistemas,
            CompetenciasClave: request.CompetenciasClave,
            CriteriosExcluyentes: request.CriteriosExcluyentes,
            CriteriosDeseables: request.CriteriosDeseables,
            Funciones: request.MainFunctions,
            ModalidadTrabajoId: request.WorkModeId,
            DisponibilidadRequerida: request.DisponibilidadRequerida,
            Seniority: (request.Seniority ?? "").Replace(" ", ""),
            Prioridad: request.Priority,
            Observaciones: request.Comments,
            UsuarioId: user.Id,
            CargoId: request.CargoId
        );

        var command = new CrearSolicitudCommand(
            createDto,
            user.Correo,
            UsuarioId: createDto.UsuarioId,
            CanalOrigen: request.Channel ?? "WHATSAPP",
            WorkflowOrigen: request.WorkflowOrigen ?? "n8n_integration",
            ApiKeyId: null,
            CorrelationId: Guid.TryParse(GetCorrelationId(), out var cid) ? cid : (Guid?)null
        );

        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message, CorrelationId = GetCorrelationId() });
        }

        var s = await _dbContext.Solicitudes
            .Include(sol => sol.Estado)
            .FirstOrDefaultAsync(sol => sol.Id == result.Value.SolicitudId);

        if (s == null) return NotFound();

        var responseDto = await MapToVacancyRequestResponseDto(s);
        return StatusCode(StatusCodes.Status201Created, responseDto);
    }

    [HttpGet("vacancy-requests/{id:int}")]
    [ProducesResponseType(typeof(VacancyRequestResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerVacancyRequestPorId([FromRoute] int id)
    {
        var s = await _dbContext.Solicitudes
            .Include(sol => sol.Estado)
            .FirstOrDefaultAsync(sol => sol.Id == id);

        if (s == null) return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Solicitud no encontrada.", CorrelationId = GetCorrelationId() });

        var responseDto = await MapToVacancyRequestResponseDto(s);
        return Ok(responseDto);
    }

    [HttpGet("vacancy-requests")]
    [ProducesResponseType(typeof(List<VacancyRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarVacancyRequests(
        [FromQuery] string? area = null,
        [FromQuery] string? requestedByUsuarioId = null,
        [FromQuery] string? requestedByUserId = null,
        [FromQuery] string? status = null)
    {
        var query = _dbContext.Solicitudes
            .Include(s => s.Estado)
            .AsQueryable();

        if (!string.IsNullOrEmpty(area))
        {
            query = query.Where(s => s.Solicitante != null && s.Solicitante.Area != null && s.Solicitante.Area.Nombre == area);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Estado.Nombre == status || s.Estado.Codigo == status);
        }

        string? userFilter = !string.IsNullOrEmpty(requestedByUsuarioId) ? requestedByUsuarioId : requestedByUserId;
        if (!string.IsNullOrEmpty(userFilter))
        {
            var user = await ResolveUsuarioByGuidAsync(userFilter);
            if (user != null)
            {
                query = query.Where(s => s.SolicitanteId == user.Id);
            }
            else
            {
                return Ok(new List<VacancyRequestResponseDto>());
            }
        }

        var list = await query.ToListAsync();
        var dtos = new List<VacancyRequestResponseDto>();
        foreach (var item in list)
        {
            dtos.Add(await MapToVacancyRequestResponseDto(item));
        }

        return Ok(dtos);
    }

    [HttpPatch("vacancy-requests/{id:int}")]
    [ProducesResponseType(typeof(VacancyRequestResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarParcialVacancyRequest([FromRoute] int id, [FromBody] VacancyRequestUpdateDto request)
    {
        var s = await _dbContext.Solicitudes
            .Include(sol => sol.Estado)
            .FirstOrDefaultAsync(sol => sol.Id == id);

        if (s == null) return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Solicitud no encontrada.", CorrelationId = GetCorrelationId() });

        s.Actualizar(
            cargo: request.Title ?? s.Cargo,
            regionalId: request.RegionalId ?? s.RegionalId,
            tipoSolicitudId: request.TipoSolicitudId ?? s.TipoSolicitudId,
            modalidadTrabajoId: request.WorkModeId ?? s.ModalidadTrabajoId,
            seniority: request.Seniority != null ? request.Seniority.Replace(" ", "") : s.Seniority,
            prioridad: request.Priority ?? s.Prioridad,
            funciones: request.MainFunctions ?? s.Funciones,
            modificadoPor: "system@nacionalseguros.com.bo",
            objetivoCargo: request.ObjetivoCargo ?? s.ObjetivoCargo,
            formacionAcademica: request.FormacionAcademica ?? s.FormacionAcademica,
            experienciaMinima: request.ExperienciaMinima ?? s.ExperienciaMinima,
            experienciaIndispensable: request.ExperienciaIndispensable ?? s.ExperienciaIndispensable,
            conocimientosTecnicos: request.ConocimientosTecnicos ?? s.ConocimientosTecnicos,
            herramientasSistemas: request.HerramientasSistemas ?? s.HerramientasSistemas,
            competenciasClave: request.CompetenciasClave ?? s.CompetenciasClave,
            disponibilidadRequerida: request.DisponibilidadRequerida ?? s.DisponibilidadRequerida,
            criteriosExcluyentes: request.CriteriosExcluyentes ?? s.CriteriosExcluyentes,
            criteriosDeseables: request.CriteriosDeseables ?? s.CriteriosDeseables,
            motivo: request.Reason ?? s.Motivo,
            cantidadVacantes: request.VacancyCount ?? s.CantidadVacantes,
            observaciones: request.Comments ?? s.Observaciones,
            canalOrigen: request.Channel ?? "WHATSAPP"
        );

        _dbContext.Solicitudes.Update(s);
        await _dbContext.SaveChangesAsync();

        var responseDto = await MapToVacancyRequestResponseDto(s);
        return Ok(responseDto);
    }

    [HttpGet("vacancy-requests/by-code/{code}")]
    [ProducesResponseType(typeof(VacancyRequestResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerVacancyRequestPorCodigo([FromRoute] string code)
    {
        var s = await _dbContext.Solicitudes
            .Include(sol => sol.Estado)
            .FirstOrDefaultAsync(sol => sol.Codigo == code);

        if (s == null) return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Solicitud no encontrada.", CorrelationId = GetCorrelationId() });

        var responseDto = await MapToVacancyRequestResponseDto(s);
        return Ok(responseDto);
    }

    // B. OBSERVATIONS
    [HttpPost("vacancy-request-observations/{id:int}/response")]
    [ProducesResponseType(typeof(ObservationResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResponderObservacion([FromRoute] int id, [FromBody] ObservationResponseDto request)
    {
        var comment = await _dbContext.SolicitudComentarios.FindAsync(id);
        if (comment == null)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Observación no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string userKey = !string.IsNullOrEmpty(request.RespondedByUsuarioId) 
            ? request.RespondedByUsuarioId 
            : request.RespondedByUserId;
        var user = await ResolveUsuarioByGuidAsync(userKey);
        if (user == null)
        {
            return BadRequest(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = $"El usuario respondedor '{userKey}' no existe.", CorrelationId = GetCorrelationId() });
        }

        var solicitud = await _dbContext.Solicitudes.FindAsync(comment.SolicitudId);
        if (solicitud == null) return NotFound();

        solicitud.AgregarComentario(user.Id, request.ResponseText);
        await _dbContext.SaveChangesAsync();

        return Ok(request);
    }

    // C. AGENT EVENTS
    [HttpPost("agent-events")]
    [ProducesResponseType(typeof(AgentEventResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearAgentEvent([FromBody] AgentEventCreateDto request)
    {
        string? metadataJson = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null;

        var ev = new AgentEvent(
            request.ChannelType,
            request.ChannelIdentifier,
            request.EventType,
            request.Description,
            request.EventSource,
            request.CorrelationId,
            metadataJson,
            request.RelatedEntityType,
            request.RelatedEntityId
        );

        await _dbContext.AgentEvents.AddAsync(ev);
        await _dbContext.SaveChangesAsync();

        object? metadataObj = null;
        if (!string.IsNullOrEmpty(ev.MetadataJson))
        {
            metadataObj = JsonSerializer.Deserialize<object>(ev.MetadataJson);
        }

        var res = new AgentEventResponseDto
        {
            Id = ev.Id,
            ChannelType = ev.ChannelType,
            ChannelIdentifier = ev.ChannelIdentifier,
            EventType = ev.EventType,
            Description = ev.Description,
            EventSource = ev.EventSource,
            CorrelationId = ev.CorrelationId,
            Metadata = metadataObj,
            RelatedEntityType = ev.RelatedEntityType,
            RelatedEntityId = ev.RelatedEntityId,
            CreatedAt = ev.CreatedAt
        };

        return StatusCode(StatusCodes.Status201Created, res);
    }

    [HttpGet("agent-events/{id:int}")]
    [ProducesResponseType(typeof(AgentEventResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerAgentEventPorId([FromRoute] int id)
    {
        var ev = await _dbContext.AgentEvents.FindAsync(id);
        if (ev == null) return NotFound();

        object? metadataObj = null;
        if (!string.IsNullOrEmpty(ev.MetadataJson))
        {
            metadataObj = JsonSerializer.Deserialize<object>(ev.MetadataJson);
        }

        var res = new AgentEventResponseDto
        {
            Id = ev.Id,
            ChannelType = ev.ChannelType,
            ChannelIdentifier = ev.ChannelIdentifier,
            EventType = ev.EventType,
            Description = ev.Description,
            EventSource = ev.EventSource,
            CorrelationId = ev.CorrelationId,
            Metadata = metadataObj,
            RelatedEntityType = ev.RelatedEntityType,
            RelatedEntityId = ev.RelatedEntityId,
            CreatedAt = ev.CreatedAt
        };

        return Ok(res);
    }

    [HttpGet("agent-events")]
    [ProducesResponseType(typeof(List<AgentEventResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAgentEvents([FromQuery] Guid? correlationId)
    {
        var query = _dbContext.AgentEvents.AsQueryable();

        if (correlationId.HasValue)
        {
            query = query.Where(ev => ev.CorrelationId == correlationId.Value);
        }

        var list = await query.ToListAsync();
        var resList = new List<AgentEventResponseDto>();
        foreach (var ev in list)
        {
            object? metadataObj = null;
            if (!string.IsNullOrEmpty(ev.MetadataJson))
            {
                metadataObj = JsonSerializer.Deserialize<object>(ev.MetadataJson);
            }

            resList.Add(new AgentEventResponseDto
            {
                Id = ev.Id,
                ChannelType = ev.ChannelType,
                ChannelIdentifier = ev.ChannelIdentifier,
                EventType = ev.EventType,
                Description = ev.Description,
                EventSource = ev.EventSource,
                CorrelationId = ev.CorrelationId,
                Metadata = metadataObj,
                RelatedEntityType = ev.RelatedEntityType,
                RelatedEntityId = ev.RelatedEntityId,
                CreatedAt = ev.CreatedAt
            });
        }

        return Ok(resList);
    }

    // D. WS SESSIONS
    [HttpPost("ws-sessions")]
    [ProducesResponseType(typeof(WSSessionResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearWSSession([FromBody] WSSessionCreateUpdateDto request)
    {
        string? tempDataJson = request.TemporaryData != null ? JsonSerializer.Serialize(request.TemporaryData) : null;
        string? pendingFieldsJson = request.PendingFields != null ? JsonSerializer.Serialize(request.PendingFields) : null;
        string? perfilSessionDataJson = request.PerfilSessionData != null ? JsonSerializer.Serialize(request.PerfilSessionData) : null;

        var existing = await _dbContext.WSSessions.FirstOrDefaultAsync(ws => ws.NormalizedIdentifier == request.NormalizedIdentifier);
        if (existing != null)
        {
            existing.Actualizar(
                request.ChannelIdentifier,
                request.ActiveAgent,
                request.SessionStatus,
                tempDataJson,
                pendingFieldsJson,
                perfilSessionDataJson
            );
            _dbContext.WSSessions.Update(existing);
            await _dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, MapToWSSessionResponseDto(existing));
        }

        var session = new WSSession(
            request.ChannelIdentifier,
            request.NormalizedIdentifier,
            request.ActiveAgent,
            request.SessionStatus,
            tempDataJson,
            pendingFieldsJson,
            perfilSessionDataJson
        );

        await _dbContext.WSSessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, MapToWSSessionResponseDto(session));
    }

    [HttpGet("ws-sessions/{normalizedIdentifier}")]
    [ProducesResponseType(typeof(WSSessionResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerWSSession(
        [FromRoute] string normalizedIdentifier,
        [FromQuery] bool incluirEtapa = false)
    {
        var session = await _dbContext.WSSessions.FirstOrDefaultAsync(ws => ws.NormalizedIdentifier == normalizedIdentifier);
        if (session == null)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Sesión no encontrada.", CorrelationId = GetCorrelationId() });
        }

        EtapaUsuarioDto? etapaUsuario = null;
        if (incluirEtapa)
        {
            etapaUsuario = await CalcularEtapaUsuarioAsync(normalizedIdentifier);
        }

        return Ok(MapToWSSessionResponseDto(session, etapaUsuario));
    }

    [HttpPut("ws-sessions/{normalizedIdentifier}")]
    [ProducesResponseType(typeof(WSSessionResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarWSSession([FromRoute] string normalizedIdentifier, [FromBody] WSSessionCreateUpdateDto request)
    {
        var session = await _dbContext.WSSessions.FirstOrDefaultAsync(ws => ws.NormalizedIdentifier == normalizedIdentifier);
        if (session == null)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Sesión no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string? tempDataJson = request.TemporaryData != null ? JsonSerializer.Serialize(request.TemporaryData) : null;
        string? pendingFieldsJson = request.PendingFields != null ? JsonSerializer.Serialize(request.PendingFields) : null;
        string? perfilSessionDataJson = request.PerfilSessionData != null ? JsonSerializer.Serialize(request.PerfilSessionData) : null;

        session.Actualizar(
            request.ChannelIdentifier,
            request.ActiveAgent,
            request.SessionStatus,
            tempDataJson,
            pendingFieldsJson,
            perfilSessionDataJson
        );

        _dbContext.WSSessions.Update(session);
        await _dbContext.SaveChangesAsync();

        return Ok(MapToWSSessionResponseDto(session));
    }

    [HttpPatch("ws-sessions/{normalizedIdentifier}")]
    [ProducesResponseType(typeof(WSSessionResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarParcialWSSession([FromRoute] string normalizedIdentifier, [FromBody] WSSessionPatchDto request)
    {
        var session = await _dbContext.WSSessions.FirstOrDefaultAsync(ws => ws.NormalizedIdentifier == normalizedIdentifier);
        if (session == null)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Sesión no encontrada.", CorrelationId = GetCorrelationId() });
        }

        string? tempDataJson = request.TemporaryData != null ? JsonSerializer.Serialize(request.TemporaryData) : null;
        string? pendingFieldsJson = request.PendingFields != null ? JsonSerializer.Serialize(request.PendingFields) : null;
        string? perfilSessionDataJson = request.PerfilSessionData != null ? JsonSerializer.Serialize(request.PerfilSessionData) : null;

        session.ActualizarParcial(
            request.ActiveAgent,
            request.SessionStatus,
            tempDataJson,
            pendingFieldsJson,
            perfilSessionDataJson
        );

        _dbContext.WSSessions.Update(session);
        await _dbContext.SaveChangesAsync();

        return Ok(MapToWSSessionResponseDto(session));
    }

    [HttpDelete("ws-sessions/{normalizedIdentifier}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EliminarWSSession([FromRoute] string normalizedIdentifier)
    {
        var session = await _dbContext.WSSessions.FirstOrDefaultAsync(ws => ws.NormalizedIdentifier == normalizedIdentifier);
        if (session == null)
        {
            return NotFound(new ApiErrorDto { Code = "NOT_FOUND", Message = "Sesión no encontrada.", CorrelationId = GetCorrelationId() });
        }

        _dbContext.WSSessions.Remove(session);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    private WSSessionResponseDto MapToWSSessionResponseDto(WSSession session, EtapaUsuarioDto? etapaUsuario = null)
    {
        object? tempDataObj = null;
        if (!string.IsNullOrEmpty(session.TemporaryDataJson))
        {
            tempDataObj = JsonSerializer.Deserialize<object>(session.TemporaryDataJson);
        }
        tempDataObj ??= new Dictionary<string, object>();

        List<string>? pendingFieldsList = null;
        if (!string.IsNullOrEmpty(session.PendingFieldsJson))
        {
            pendingFieldsList = JsonSerializer.Deserialize<List<string>>(session.PendingFieldsJson);
        }
        pendingFieldsList ??= new List<string>();

        object? perfilSessionDataObj = null;
        if (!string.IsNullOrEmpty(session.PerfilSessionDataJson))
        {
            perfilSessionDataObj = JsonSerializer.Deserialize<object>(session.PerfilSessionDataJson);
        }
        perfilSessionDataObj ??= new Dictionary<string, object>();

        return new WSSessionResponseDto
        {
            Id = session.Id,
            ChannelIdentifier = session.ChannelIdentifier,
            NormalizedIdentifier = session.NormalizedIdentifier,
            ActiveAgent = session.ActiveAgent,
            SessionStatus = session.SessionStatus,
            TemporaryData = tempDataObj,
            PendingFields = pendingFieldsList,
            PerfilSessionData = perfilSessionDataObj,
            LastInteractionAt = session.LastInteractionAt,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            EtapaUsuario = etapaUsuario
        };
    }

    private async Task<EtapaUsuarioDto> CalcularEtapaUsuarioAsync(string normalizedIdentifier)
    {
        var cleanPhone = normalizedIdentifier.StartsWith("+") ? normalizedIdentifier.Substring(1) : normalizedIdentifier;
        var localPhone = cleanPhone.StartsWith("591") ? cleanPhone.Substring(3) : cleanPhone;

        var usuario = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => !u.IsDeleted && (u.Telefono == normalizedIdentifier || u.Telefono == cleanPhone || u.Telefono == localPhone || u.Telefono == "+591" + localPhone));

        if (usuario == null)
        {
            return new EtapaUsuarioDto
            {
                CalculadoEn = DateTime.UtcNow,
                Resumen = new EtapaUsuarioResumenDto
                {
                    SolicitudesEnCurso = 0,
                    SolicitudesAprobadasSinPerfil = 0,
                    SolicitudesConPerfil = 0,
                    PerfilesEsperandoAlUsuario = 0
                },
                IndiceSolicitudesConPerfil = new List<int>(),
                PerfilesDestacados = new List<PerfilDestacadoDto>()
            };
        }

        int userId = usuario.Id;

        var solicitudesEnCurso = await _dbContext.Solicitudes
            .Include(s => s.Estado)
            .Where(s => !s.IsDeleted && s.SolicitanteId == userId && s.Estado.Codigo != "SOL-APR" && s.Estado.Codigo != "SOL-RECH")
            .CountAsync();

        var perfiles = await _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
            .Include(p => p.Estado)
            .Where(p => !p.IsDeleted && !p.Solicitud.IsDeleted && p.Solicitud.SolicitanteId == userId)
            .ToListAsync();

        int solicitudesAprobadasSinPerfil = perfiles.Count(p => p.Estado.Codigo == "PERF-PEN-GEN");

        var perfilesConPerfil = perfiles.Where(p => p.Estado.Codigo != "PERF-PEN-GEN").ToList();
        int solicitudesConPerfilCount = perfilesConPerfil.Count;

        int perfilesEsperandoAlUsuario = perfiles.Count(p => p.Estado.Codigo == "PERF-REV-AREA");

        var indiceSolicitudesConPerfil = perfilesConPerfil
            .Select(p => p.SolicitudId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var perfilesDestacados = perfiles
            .OrderByDescending(p => p.Estado.Codigo == "PERF-REV-AREA")
            .ThenByDescending(p => p.ModifiedDate ?? p.CreatedDate)
            .Take(10)
            .Select(p => new PerfilDestacadoDto
            {
                SolicitudId = p.SolicitudId,
                SolicitudCodigo = p.Solicitud.Codigo,
                PerfilId = p.Id,
                PerfilCodigo = p.Codigo,
                Cargo = p.Cargo,
                EstadoCodigo = p.Estado.Codigo,
                EstadoEtiqueta = p.Estado.Nombre,
                EsperaAlUsuario = p.Estado.Codigo == "PERF-REV-AREA",
                FechaModificacion = p.ModifiedDate ?? p.CreatedDate
            })
            .ToList();

        return new EtapaUsuarioDto
        {
            CalculadoEn = DateTime.UtcNow,
            Resumen = new EtapaUsuarioResumenDto
            {
                SolicitudesEnCurso = solicitudesEnCurso,
                SolicitudesAprobadasSinPerfil = solicitudesAprobadasSinPerfil,
                SolicitudesConPerfil = solicitudesConPerfilCount,
                PerfilesEsperandoAlUsuario = perfilesEsperandoAlUsuario
            },
            IndiceSolicitudesConPerfil = indiceSolicitudesConPerfil,
            PerfilesDestacados = perfilesDestacados
        };
    }

    // ==========================================
    // NUEVOS ENDPOINTS DE INTEGRACIÓN DE PERFILES (n8n / WhatsApp)
    // ==========================================

    [HttpGet("vacancy-requests/perfiles")]
    public async Task<IActionResult> GetPerfilesInterno([FromQuery] int? solicitanteId, [FromQuery] string? telefono)
    {
        int? resolvedSolicitanteId = solicitanteId;
        if (!resolvedSolicitanteId.HasValue && !string.IsNullOrEmpty(telefono))
        {
            var normalizedIdentifier = telefono.StartsWith("+") ? telefono : "+591" + telefono;
            var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Telefono == normalizedIdentifier || u.Telefono == normalizedIdentifier.Replace("+591", ""));
            if (usuario == null) return NotFound(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = "No se encontró el usuario." });
            resolvedSolicitanteId = usuario.Id;
        }

        if (!resolvedSolicitanteId.HasValue)
        {
            return BadRequest(new ApiErrorDto { Code = "BAD_REQUEST", Message = "Debe proporcionar solicitanteId o telefono." });
        }

        var query = _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
            .Include(p => p.Estado)
            .Where(p => !p.IsDeleted && p.Solicitud.SolicitanteId == resolvedSolicitanteId.Value)
            .OrderByDescending(p => p.ModifiedDate ?? p.CreatedDate);

        var perfiles = await query.ToListAsync();
        var list = perfiles.Select(p => new {
            perfilId = p.Id,
            codigoPerfil = $"PRF-{p.Solicitud.Codigo}",
            cargo = p.Solicitud.Cargo,
            estado = p.Estado.Nombre,
            estadoCodigo = p.Estado.Codigo,
            solicitudId = p.SolicitudId,
            fechaCreacion = p.CreatedDate,
            fechaModificacion = p.ModifiedDate ?? p.CreatedDate
        }).ToList();

        return Ok(list);
    }

    [HttpGet("perfiles/{id:int}/detalle-completo")]
    public async Task<IActionResult> GetPerfilDetalleInterno([FromRoute] int id, [FromQuery] string? telefono = null)
    {
        var perfil = await _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
            .Include(p => p.Estado)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (perfil == null) return NotFound();

        // Validar acceso según el tipo de solicitud
        if (!string.IsNullOrEmpty(telefono))
        {
            var normalizedIdentifier = telefono.StartsWith("+") ? telefono : "+591" + telefono;
            var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Telefono == normalizedIdentifier || u.Telefono == normalizedIdentifier.Replace("+591", ""));
            if (usuario == null)
            {
                return NotFound(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = "No se encontró el usuario solicitante.", CorrelationId = GetCorrelationId() });
            }

            if (perfil.Solicitud.SolicitanteId != usuario.Id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para consultar este perfil.", CorrelationId = GetCorrelationId() });
            }
        }
        else
        {
            // Uso técnico de Automatización: Exige permisos del sistema
            if (!UserHasPermission("perfiles.read") && !UserHasPermission("perfiles.write"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos de sistema para consultar perfiles.", CorrelationId = GetCorrelationId() });
            }
        }

        var resumen = await _dbContext.ResumenEjecutivos.FirstOrDefaultAsync(r => r.PerfilCargoId == id);
        object? resumenRol = null;
        if (resumen != null)
        {
            resumenRol = new {
                resumen = resumen.Resumen,
                objetivoCargo = resumen.ObjetivoCargo,
                funcionesPrincipales = JsonSerializer.Deserialize<List<string>>(resumen.FuncionesPrincipales) ?? new List<string>(),
                requisitosMinimos = JsonSerializer.Deserialize<List<string>>(resumen.RequisitosMinimos) ?? new List<string>(),
                formacionExperiencia = resumen.FormacionExperiencia,
                hardSkills = JsonSerializer.Deserialize<List<string>>(resumen.HardSkills) ?? new List<string>(),
                softSkills = JsonSerializer.Deserialize<List<string>>(resumen.SoftSkills) ?? new List<string>(),
                modalidad = resumen.Modalidad,
                ubicacion = resumen.Ubicacion,
                bandaSalarial = resumen.BandaSalarial,
                criteriosEvaluacion = resumen.CriteriosEvaluacion,
                caracteristicasClave = resumen.CaracteristicasClave,
                valoracionPerfil = resumen.ValoracionPerfil
            };
        }

        return Ok(new {
            perfilId = perfil.Id,
            solicitudId = perfil.SolicitudId,
            cargo = perfil.Solicitud.Cargo,
            estadoCodigo = perfil.Estado.Codigo,
            resumenEjecutivoRol = resumenRol
        });
    }

    [HttpPut("perfiles/{perfilId:int}/resumen")]
    public async Task<IActionResult> ActualizarResumenInterno([FromRoute] int perfilId, [FromBody] ResumenEjecutivoUpdateRequest request)
    {
        var command = new ActualizarResumenCommand(perfilId, request.SolicitudId, request.ResumenEjecutivoRol, "n8n_IA", EsAutomatizacion: true);
        var result = await _sender.Send(command);
        if (result.IsFailure) return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        return Ok();
    }

    [HttpGet("perfiles/solicitud/{solicitudId:int}/resumen")]
    [ProducesResponseType(typeof(ResumenEjecutivoRolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerResumenPorSolicitudId([FromRoute] int solicitudId)
    {
        if (!UserHasPermission("perfiles.read"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para leer perfiles.", CorrelationId = GetCorrelationId() });
        }

        var perfil = await _dbContext.PerfilesCargo.FirstOrDefaultAsync(p => p.SolicitudId == solicitudId && !p.IsDeleted);
        if (perfil == null)
        {
            return Ok(null);
        }

        var resumen = await _dbContext.ResumenEjecutivos.FirstOrDefaultAsync(r => r.PerfilCargoId == perfil.Id);
        if (resumen == null)
        {
            return Ok(null);
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dto = new ResumenEjecutivoRolDto(
            Resumen: resumen.Resumen,
            ObjetivoCargo: resumen.ObjetivoCargo,
            FuncionesPrincipales: JsonSerializer.Deserialize<List<string>>(resumen.FuncionesPrincipales, options) ?? new List<string>(),
            RequisitosMinimos: JsonSerializer.Deserialize<List<string>>(resumen.RequisitosMinimos, options) ?? new List<string>(),
            FormacionExperiencia: resumen.FormacionExperiencia,
            HardSkills: JsonSerializer.Deserialize<List<string>>(resumen.HardSkills, options) ?? new List<string>(),
            SoftSkills: JsonSerializer.Deserialize<List<string>>(resumen.SoftSkills, options) ?? new List<string>(),
            Modalidad: resumen.Modalidad,
            Ubicacion: resumen.Ubicacion,
            BandaSalarial: resumen.BandaSalarial,
            CriteriosEvaluacion: resumen.CriteriosEvaluacion,
            CaracteristicasClave: resumen.CaracteristicasClave,
            ValoracionPerfil: resumen.ValoracionPerfil,
            Version: perfil.Version
        );

        return Ok(dto);
    }

    [HttpPut("perfiles/solicitud/{solicitudId:int}/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActualizarResumenPorSolicitudId([FromRoute] int solicitudId, [FromBody] ResumenEjecutivoRolDto request)
    {
        if (!UserHasPermission("perfiles.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para actualizar perfiles.", CorrelationId = GetCorrelationId() });
        }

        var perfil = await _dbContext.PerfilesCargo.FirstOrDefaultAsync(p => p.SolicitudId == solicitudId && !p.IsDeleted);
        if (perfil == null)
        {
            return Ok(new { message = "Perfil no encontrado" });
        }

        var existing = await _dbContext.ResumenEjecutivos.AnyAsync(r => r.PerfilCargoId == perfil.Id);

        var command = new ActualizarResumenCommand(perfil.Id, solicitudId, request, "n8n_IA", EsAutomatizacion: true);
        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message, CorrelationId = GetCorrelationId() });
        }

        return Ok(new
        {
            message = existing ? "Actualizado con éxito" : "Creado con éxito"
        });
    }

    [HttpPost("perfiles/{perfilId:int}/observaciones")]
    public async Task<IActionResult> RegistrarObservacionInterna([FromRoute] int perfilId, [FromBody] PerfilObservacionCreateRequest request, [FromQuery] string telefono)
    {
        var normalizedIdentifier = telefono.StartsWith("+") ? telefono : "+591" + telefono;
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Telefono == normalizedIdentifier || u.Telefono == normalizedIdentifier.Replace("+591", ""));
        if (usuario == null) return NotFound(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = "No se encontró el usuario solicitante." });

        var command = new RegistrarObservacionPerfilCommand(perfilId, request.TipoObservacionId, request.Comentario, usuario.Id, usuario.Nombre);
        var result = await _sender.Send(command);
        if (result.IsFailure) return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        return Ok();
    }

    [HttpPost("perfiles/{perfilId:int}/aprobar-solicitante")]
    public async Task<IActionResult> AprobarSolicitanteInterna([FromRoute] int perfilId, [FromQuery] string telefono)
    {
        var normalizedIdentifier = telefono.StartsWith("+") ? telefono : "+591" + telefono;
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Telefono == normalizedIdentifier || u.Telefono == normalizedIdentifier.Replace("+591", ""));
        if (usuario == null) return NotFound(new ApiErrorDto { Code = "USER_NOT_FOUND", Message = "No se encontró el usuario solicitante." });

        var command = new AprobarSolicitantePerfilCommand(perfilId, usuario.Id, usuario.Nombre);
        var result = await _sender.Send(command);
        if (result.IsFailure) return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        return Ok();
    }

    [HttpGet("solicitudes/{solicitudId:int}/documentos/{tipo}/descarga")]
    public async Task<IActionResult> DescargarDocumento([FromRoute] int solicitudId, [FromRoute] string tipo)
    {
        if (!UserHasPermission("solicitudes.read") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos de sistema para descargar documentos.", CorrelationId = GetCorrelationId() });
        }

        var doc = await _dbContext.SolicitudDocumentos
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

    [HttpGet("tipos-observacion")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTiposObservacionActivos()
    {
        var list = await _dbContext.Set<TipoObservacion>()
            .Where(t => t.Estado == "Activo" && !t.IsDeleted)
            .Select(t => new
            {
                id = t.Id,
                nombre = t.Nombre,
                descripcion = t.Descripcion
            })
            .ToListAsync();

        return Ok(list);
    }

    private async Task<IActionResult> GuardarPdfInterno(int perfilId, string tipoDocumento, IFormFile? file, string? publicUrl)
    {
        if (!UserHasPermission("perfiles.write") && !UserHasPermission("solicitudes.editar") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para subir documentos.", CorrelationId = GetCorrelationId() });
        }

        var perfil = await _dbContext.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
            .FirstOrDefaultAsync(p => p.Id == perfilId);
        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"El perfil con ID {perfilId} no existe.", CorrelationId = GetCorrelationId() });
        }

        string fileName = file?.FileName ?? $"{tipoDocumento}_{perfilId}.pdf";
        string storageProvider = file != null ? "DATABASE" : "AZURE";
        string storagePath = "";
        string? resolvedUrl = publicUrl;
        byte[]? binaryContent = null;

        if (file != null)
        {
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                binaryContent = ms.ToArray();
            }
            storagePath = $"DATABASE://{fileName}";
            if (string.IsNullOrEmpty(resolvedUrl))
            {
                resolvedUrl = $"/api/internal/perfiles/{perfilId}/documentos/perfil-estructurado/descarga";
            }
        }
        else if (!string.IsNullOrEmpty(publicUrl))
        {
            storagePath = publicUrl;
        }
        else
        {
            return BadRequest(new ApiErrorDto { Code = "Documento.Empty", Message = "Debe subir un archivo PDF o enviar una URL pública.", CorrelationId = GetCorrelationId() });
        }

        var existing = await _dbContext.Set<SolicitudDocumento>()
            .FirstOrDefaultAsync(d => d.SolicitudId == perfil.SolicitudId && d.TipoDocumento == tipoDocumento);

        if (existing != null)
        {
            existing.Actualizar(
                fileName,
                storageProvider,
                storagePath,
                resolvedUrl,
                "n8n_automation",
                Guid.NewGuid(),
                binaryContent
            );
            _dbContext.Set<SolicitudDocumento>().Update(existing);
        }
        else
        {
            var document = new SolicitudDocumento(
                perfil.SolicitudId,
                tipoDocumento,
                fileName,
                storageProvider,
                storagePath,
                resolvedUrl,
                "n8n_automation",
                Guid.NewGuid()
            );
            if (binaryContent != null)
            {
                document.SetContenidoBinario(binaryContent);
            }
            await _dbContext.Set<SolicitudDocumento>().AddAsync(document);
        }

        // Transición automática a PERF-REV-AREA cuando n8n guarda/actualiza el PDF
        var targetEstado = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-REV-AREA");
        bool esEstadoPermitidoParaAutoTransicion = perfil.Estado?.Codigo == "PERF-RES-GEN" || perfil.Estado?.Codigo == "PERF-COR-RRHH";
        if (targetEstado != null && perfil.EstadoId != targetEstado.Id && esEstadoPermitidoParaAutoTransicion)
        {
            int oldEstadoId = perfil.EstadoId;
            var oldEstado = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Id == oldEstadoId);
            if (oldEstado?.Codigo == "PERF-OBS-AREA" || oldEstado?.Codigo == "PERF-COR-RRHH")
            {
                perfil.IncrementarVersion();
            }
            perfil.CambiarEstado(targetEstado.Id, "n8n_automation");
            
            var systemUser = await _dbContext.Set<Usuario>().FirstOrDefaultAsync(u => u.Correo == "system@nacionalseguros.com.bo");
            int systemUserId = systemUser?.Id ?? 16;

            var history = new StateHistory(
                "Perfil",
                perfil.Id,
                oldEstadoId,
                targetEstado.Id,
                systemUserId,
                "Transición automática a En Revisión Área por actualización de documento desde n8n",
                Guid.NewGuid(),
                null,
                "n8n"
            );
            await _dbContext.Set<StateHistory>().AddAsync(history);
            _dbContext.Set<PerfilCargo>().Update(perfil);
        }

        await _dbContext.SaveChangesAsync();
        return Ok(new { Message = "PDF guardado exitosamente en la base de datos." });
    }

    [HttpPost("perfiles/{perfilId:int}/documentos/perfil-estructurado")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GuardarPdfPerfilEstructurado([FromRoute] int perfilId, [FromForm] GuardarPdfForm request)
    {
        return GuardarPdfInterno(perfilId, "PERFIL_ESTRUCTURADO_PDF", request.File, request.PublicUrl);
    }

    [HttpPost("perfiles/{perfilId:int}/documentos/resumen-ejecutivo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GuardarPdfResumenEjecutivo([FromRoute] int perfilId, [FromForm] GuardarPdfForm request)
    {
        return GuardarPdfInterno(perfilId, "RESUMEN_EJECUTIVO_PDF", request.File, request.PublicUrl);
    }

    private async Task<IActionResult> DescargarPdfPorPerfilIdInterno(int perfilId, string tipoDocumento)
    {
        if (!UserHasPermission("perfiles.read") && !UserHasPermission("solicitudes.leer") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para descargar documentos.", CorrelationId = GetCorrelationId() });
        }

        var perfil = await _dbContext.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
            .FirstOrDefaultAsync(p => p.Id == perfilId);
        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"El perfil con ID {perfilId} no existe.", CorrelationId = GetCorrelationId() });
        }

        var doc = await _dbContext.SolicitudDocumentos
            .FirstOrDefaultAsync(d => d.SolicitudId == perfil.SolicitudId && d.TipoDocumento == tipoDocumento);
        if (doc == null) return NotFound(new ApiErrorDto { Code = "Documento.NotFound", Message = "El documento solicitado no está registrado.", CorrelationId = GetCorrelationId() });

        // Prioridad 1: Servir desde ContenidoBinario almacenado en la Base de Datos
        if (doc.ContenidoBinario != null && doc.ContenidoBinario.Length > 0)
        {
            var downloadName = doc.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? doc.FileName : $"{doc.FileName}.pdf";
            return File(doc.ContenidoBinario, "application/pdf", downloadName);
        }

        // Prioridad 2: Si es una URL pública externa HTTP
        if (!string.IsNullOrEmpty(doc.PublicUrl) && doc.PublicUrl.StartsWith("http"))
        {
            return Redirect(doc.PublicUrl);
        }

        // Prioridad 3: Archivo en disco local (fallback)
        if (System.IO.File.Exists(doc.StoragePath))
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(doc.StoragePath);
            return File(bytes, "application/pdf", doc.FileName);
        }

        // Transición automática a PERF-REV-AREA cuando n8n descarga el PDF
        var targetEstado = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-REV-AREA");
        bool esEstadoPermitidoParaAutoTransicion = perfil.Estado?.Codigo == "PERF-RES-GEN" || perfil.Estado?.Codigo == "PERF-COR-RRHH";
        if (targetEstado != null && perfil.EstadoId != targetEstado.Id && esEstadoPermitidoParaAutoTransicion)
        {
            int oldEstadoId = perfil.EstadoId;
            var oldEstado = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Id == oldEstadoId);
            if (oldEstado?.Codigo == "PERF-OBS-AREA" || oldEstado?.Codigo == "PERF-COR-RRHH")
            {
                perfil.IncrementarVersion();
            }
            perfil.CambiarEstado(targetEstado.Id, "n8n_automation");
            
            var systemUser = await _dbContext.Set<Usuario>().FirstOrDefaultAsync(u => u.Correo == "system@nacionalseguros.com.bo");
            int systemUserId = systemUser?.Id ?? 16;

            var history = new StateHistory(
                "Perfil",
                perfil.Id,
                oldEstadoId,
                targetEstado.Id,
                systemUserId,
                "Transición automática a En Revisión Área por descarga de documento desde n8n",
                Guid.NewGuid(),
                null,
                "n8n"
            );
            await _dbContext.Set<StateHistory>().AddAsync(history);
            _dbContext.Set<PerfilCargo>().Update(perfil);
            await _dbContext.SaveChangesAsync();
        }

        return NotFound(new ApiErrorDto { Code = "Documento.FileNotFound", Message = "El archivo físico no fue encontrado en el servidor.", CorrelationId = GetCorrelationId() });
    }

    [HttpGet("perfiles/{perfilId:int}/documentos/perfil-estructurado/descarga")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DescargarPdfPerfilEstructurado([FromRoute] int perfilId)
    {
        return DescargarPdfPorPerfilIdInterno(perfilId, "PERFIL_ESTRUCTURADO_PDF");
    }

    [HttpGet("perfiles/{perfilId:int}/documentos/resumen-ejecutivo/descarga")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DescargarPdfResumenEjecutivo([FromRoute] int perfilId)
    {
        return DescargarPdfPorPerfilIdInterno(perfilId, "RESUMEN_EJECUTIVO_PDF");
    }

    [HttpPost("perfiles/observaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearPerfilObservacionInterna([FromBody] CrearPerfilObservacionInternaRequest request)
    {
        if (!UserHasPermission("perfiles.write") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para crear observaciones de perfiles.", CorrelationId = GetCorrelationId() });
        }

        var perfil = await _dbContext.Set<PerfilCargo>()
            .Include(p => p.Solicitud)
            .Include(p => p.Estado)
            .FirstOrDefaultAsync(p => p.Id == request.PerfilCargoId);
        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"El perfil con ID {request.PerfilCargoId} no existe.", CorrelationId = GetCorrelationId() });
        }

        var tipoObs = await _dbContext.Set<TipoObservacion>().FirstOrDefaultAsync(t => t.Id == request.TipoObservacionId);
        if (tipoObs == null || tipoObs.Estado != "Activo" || tipoObs.IsDeleted)
        {
            return BadRequest(new ApiErrorDto { Code = "TipoObservacion.Invalid", Message = "El tipo de observación no existe o no está activo.", CorrelationId = GetCorrelationId() });
        }

        int iteracion = 1;
        var maxIter = await _dbContext.Set<PerfilObservacion>()
            .Where(o => o.PerfilCargoId == request.PerfilCargoId)
            .OrderByDescending(o => o.NumeroIteracion)
            .Select(o => o.NumeroIteracion)
            .FirstOrDefaultAsync();
        if (maxIter > 0)
        {
            iteracion = maxIter + 1;
        }

        int actingUserId = request.UsuarioId ?? perfil.Solicitud.SolicitanteId;
        var actingUser = await _dbContext.Set<Usuario>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == actingUserId);
        string actingUserRole = actingUser?.Roles.FirstOrDefault()?.Nombre ?? "Solicitante";

        var observacion = new PerfilObservacion(
            perfil.Id,
            request.TipoObservacionId,
            request.Comentario,
            actingUserId,
            iteracion
        );

        observacion.SetEstadoObservacion("no atendido");

        await _dbContext.Set<PerfilObservacion>().AddAsync(observacion);

        // Si el perfil está en revisión del área, transitionar automáticamente
        var targetObs = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-OBS-AREA");
        var targetRrhh = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-REV-RRHH");
        if (perfil.Estado?.Codigo == "PERF-REV-AREA" && targetObs != null && targetRrhh != null)
        {
            int oldEstadoId = perfil.EstadoId;
            perfil.CambiarEstado(targetObs.Id, "n8n_automation");
            
            var systemUser = await _dbContext.Set<Usuario>().FirstOrDefaultAsync(u => u.Correo == "system@nacionalseguros.com.bo");
            int systemUserId = systemUser?.Id ?? 16;

            var historyObs = new StateHistory(
                "Perfil",
                perfil.Id,
                oldEstadoId,
                targetObs.Id,
                actingUserId,
                $"Observado por WhatsApp (n8n). Iteración: {iteracion}",
                Guid.NewGuid(),
                null,
                actingUserRole
            );
            await _dbContext.Set<StateHistory>().AddAsync(historyObs);

            perfil.CambiarEstado(targetRrhh.Id, "System");
            var historyRrhh = new StateHistory(
                "Perfil",
                perfil.Id,
                targetObs.Id,
                targetRrhh.Id,
                systemUserId,
                "Retorno automático a Revisión RRHH tras observaciones del Área por WhatsApp",
                Guid.NewGuid(),
                null,
                "System"
            );
            await _dbContext.Set<StateHistory>().AddAsync(historyRrhh);
            _dbContext.Set<PerfilCargo>().Update(perfil);
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Observación de perfil creada exitosamente." });
    }

    [HttpPost("perfiles/{perfilId:int}/decision-area")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarDecisionAreaInterna([FromRoute] int perfilId, [FromBody] DecisionAreaRequest request)
    {
        if (!UserHasPermission("perfiles.write") && !UserHasPermission("solicitudes.write"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Code = "FORBIDDEN", Message = "No tiene permisos para registrar decisiones de área.", CorrelationId = GetCorrelationId() });
        }

        var perfil = await _dbContext.Set<PerfilCargo>()
            .Include(p => p.Estado)
            .Include(p => p.Solicitud)
            .FirstOrDefaultAsync(p => p.Id == perfilId);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"El perfil con ID {perfilId} no existe.", CorrelationId = GetCorrelationId() });
        }

        if (perfil.Estado?.Codigo != "PERF-REV-AREA")
        {
            return BadRequest(new ApiErrorDto { Code = "Perfil.InvalidState", Message = $"El perfil no se encuentra en revisión del área (Estado actual: {perfil.Estado?.Codigo}).", CorrelationId = GetCorrelationId() });
        }

        int actingUserId = request.UsuarioId ?? perfil.Solicitud.SolicitanteId;
        var actingUser = await _dbContext.Set<Usuario>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == actingUserId);
        string actingUserRole = actingUser?.Roles.FirstOrDefault()?.Nombre ?? "Solicitante";

        string decision = request.Decision?.ToUpper() ?? "";
        if (decision != "APROBAR" && decision != "OBSERVAR")
        {
            return BadRequest(new ApiErrorDto { Code = "Decision.Invalid", Message = "La decisión debe ser 'APROBAR' o 'OBSERVAR'.", CorrelationId = GetCorrelationId() });
        }

        if (decision == "APROBAR")
        {
            var targetApr = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-APR-AREA");
            if (targetApr != null)
            {
                int oldEstadoId = perfil.EstadoId;
                perfil.CambiarEstado(targetApr.Id, "n8n_automation");
                
                var historyApr = new StateHistory(
                    "Perfil",
                    perfil.Id,
                    oldEstadoId,
                    targetApr.Id,
                    actingUserId,
                    "Aprobado por el Área Solicitante vía WhatsApp (n8n)",
                    Guid.NewGuid(),
                    null,
                    actingUserRole
                );
                await _dbContext.Set<StateHistory>().AddAsync(historyApr);
            }
        }
        else // OBSERVAR
        {
            int iteracion = await _dbContext.Set<PerfilObservacion>()
                .Where(o => o.PerfilCargoId == perfilId)
                .OrderByDescending(o => o.NumeroIteracion)
                .Select(o => o.NumeroIteracion)
                .FirstOrDefaultAsync() + 1;

            if (request.Observaciones != null && request.Observaciones.Any())
            {
                foreach (var obsItem in request.Observaciones)
                {
                    var tipoObs = await _dbContext.Set<TipoObservacion>().FirstOrDefaultAsync(t => t.Id == obsItem.TipoObservacionId);
                    if (tipoObs == null || tipoObs.Estado != "Activo")
                    {
                        return BadRequest(new ApiErrorDto { Code = "TipoObservacion.Invalid", Message = $"El tipo de observación con ID {obsItem.TipoObservacionId} no es válido o no está activo.", CorrelationId = GetCorrelationId() });
                    }

                    var observacion = new PerfilObservacion(
                        perfil.Id,
                        obsItem.TipoObservacionId,
                        obsItem.Comentario,
                        actingUserId,
                        iteracion
                    );
                    observacion.SetEstadoObservacion("no atendido");
                    await _dbContext.Set<PerfilObservacion>().AddAsync(observacion);
                }
            }

            var targetObs = await _dbContext.Set<Estado>().FirstOrDefaultAsync(e => e.Codigo == "PERF-OBS-AREA");
            if (targetObs != null)
            {
                int oldEstadoId = perfil.EstadoId;
                perfil.CambiarEstado(targetObs.Id, "n8n_automation");

                var historyObs = new StateHistory(
                    "Perfil",
                    perfil.Id,
                    oldEstadoId,
                    targetObs.Id,
                    actingUserId,
                    $"Observado por el Área Solicitante vía WhatsApp (n8n). Iteración: {iteracion}",
                    Guid.NewGuid(),
                    null,
                    actingUserRole
                );
                await _dbContext.Set<StateHistory>().AddAsync(historyObs);
            }
        }

        _dbContext.Set<PerfilCargo>().Update(perfil);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Decisión del área procesada con éxito (Estado: Observado por el Área Solicitante)." });
    }

    [HttpGet("perfiles/{codigoPerfil}/resumen-y-pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerResumenYPdfPorCodigoPerfil([FromRoute] string codigoPerfil)
    {
        var perfil = await _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
            .FirstOrDefaultAsync(p => p.Codigo == codigoPerfil && !p.IsDeleted);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"No se encontró el perfil de cargo con código {codigoPerfil}.", CorrelationId = GetCorrelationId() });
        }

        var resumen = await _dbContext.ResumenEjecutivos
            .FirstOrDefaultAsync(r => r.PerfilCargoId == perfil.Id);

        var doc = await _dbContext.SolicitudDocumentos
            .FirstOrDefaultAsync(d => d.SolicitudId == perfil.SolicitudId && d.TipoDocumento == "PERFIL_ESTRUCTURADO_PDF");

        return Ok(new
        {
            cargo = perfil.Solicitud?.Cargo ?? perfil.Cargo,
            resumen = resumen?.Resumen ?? string.Empty,
            pdfUrl = doc?.PublicUrl ?? string.Empty
        });
    }

    [HttpGet("users/{userId:int}/perfiles-pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPerfilesPendientesUsuario([FromRoute] int userId)
    {
        var targetEstados = new[] { "PERF-REV-RRHH", "PERF-REV-AREA" };

        var perfiles = await _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
                .ThenInclude(s => s.Solicitante)
                    .ThenInclude(u => u.Area)
            .Include(p => p.Estado)
            .Where(p => !p.IsDeleted && 
                        p.Solicitud.SolicitanteId == userId && 
                        targetEstados.Contains(p.Estado.Codigo))
            .ToListAsync();

        var list = perfiles.Select(p => new
        {
            perfilCargoId = p.Id,
            solicitudId = p.SolicitudId,
            codigoPerfil = p.Codigo,
            cargo = p.Solicitud.Cargo,
            areaSolicitante = p.Solicitud.Solicitante?.Area?.Nombre ?? "Sin Área",
            estado = p.Estado.Nombre
        }).ToList();

        return Ok(list);
    }

    [HttpGet("postulantes-externos/filtrados/{perfilCargoId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPostulantesExternosFiltrados([FromRoute] int perfilCargoId)
    {
        var perfil = await _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
                .ThenInclude(s => s.Regional)
            .FirstOrDefaultAsync(p => p.Id == perfilCargoId && !p.IsDeleted);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"No se encontró el perfil de cargo con ID {perfilCargoId}.", CorrelationId = GetCorrelationId() });
        }

        // Obtener el salario del perfil
        decimal? salarioMaximo = null;
        string? salRaw = perfil.Salario;
        if (!string.IsNullOrWhiteSpace(salRaw))
        {
            if (decimal.TryParse(salRaw, out decimal salMax))
            {
                salarioMaximo = salMax;
            }
            else
            {
                var cleanMax = new string(salRaw.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                if (decimal.TryParse(cleanMax, out decimal salCleanMax))
                {
                    salarioMaximo = salCleanMax;
                }
            }
        }

        if (salarioMaximo == null)
        {
            // Intentar desde ResumenEjecutivo
            var resumen = await _dbContext.ResumenEjecutivos
                .FirstOrDefaultAsync(r => r.PerfilCargoId == perfilCargoId);
            if (resumen != null && !string.IsNullOrWhiteSpace(resumen.BandaSalarial))
            {
                if (decimal.TryParse(resumen.BandaSalarial, out decimal salRes))
                {
                    salarioMaximo = salRes;
                }
                else
                {
                    var parts = resumen.BandaSalarial.Split(new[] { '-', 'a' }, StringSplitOptions.RemoveEmptyEntries);
                    var lastPart = parts.LastOrDefault() ?? resumen.BandaSalarial;
                    var cleanRes = new string(lastPart.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                    if (decimal.TryParse(cleanRes, out decimal salCleanRes))
                    {
                        salarioMaximo = salCleanRes;
                    }
                }
            }
        }

        // Si aun es null, usar fallback alto para no bloquear
        decimal limiteSalario = salarioMaximo ?? 999999;

        // Obtener la regional de la solicitud
        string? regionalNombre = perfil.Solicitud?.Regional?.Nombre;

        var postulantes = await _dbContext.PostulantesExternos
            .Where(p => p.PerfilCargoId == perfilCargoId && !p.IsDeleted)
            .ToListAsync();

        var filtrados = postulantes.Where(p => {
            // Filtro Regional: CiudadResidencia coincida con la Regional del perfil
            if (!string.IsNullOrEmpty(regionalNombre) && 
                !string.Equals(p.CiudadResidencia, regionalNombre, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filtro Salarial: PretensionSalarialBs <= limiteSalario OR (PretensionSalarialBs > limiteSalario AND PretensionNegociable == "si")
            if (p.PretensionSalarialBs <= limiteSalario)
            {
                return true;
            }
            return string.Equals(p.PretensionNegociable, "si", StringComparison.OrdinalIgnoreCase);
        }).ToList();

        // Mapear a respuesta con arreglos deserializados para n8n
        var response = filtrados.Select(p => {
            List<string> postgrado = new();
            List<string> maestria = new();
            List<object> exp = new();
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

            try { postgrado = JsonSerializer.Deserialize<List<string>>(p.PostgradoInstitucion ?? "[]") ?? new(); } catch {}
            try { maestria = JsonSerializer.Deserialize<List<string>>(p.MaestriaInstitucion ?? "[]") ?? new(); } catch {}
            try { exp = JsonSerializer.Deserialize<List<object>>(p.ExperienciasLaborales ?? "[]") ?? new(); } catch {}
            try { certs = JsonSerializer.Deserialize<List<string>>(p.Certificaciones ?? "[]") ?? new(); } catch {}
            try { cursos = JsonSerializer.Deserialize<List<string>>(p.CursosComplementarios ?? "[]") ?? new(); } catch {}
            try { idiomas = JsonSerializer.Deserialize<List<string>>(p.Idiomas ?? "[]") ?? new(); } catch {}
            try { sectores = JsonSerializer.Deserialize<List<string>>(p.SectoresExperiencia ?? "[]") ?? new(); } catch {}
            try { hard = JsonSerializer.Deserialize<List<string>>(p.HardSkills ?? "[]") ?? new(); } catch {}
            try { soft = JsonSerializer.Deserialize<List<string>>(p.SoftSkills ?? "[]") ?? new(); } catch {}
            try { herramientas = JsonSerializer.Deserialize<List<string>>(p.HerramientasSistemas ?? "[]") ?? new(); } catch {}
            try { conTecnicos = JsonSerializer.Deserialize<List<string>>(p.ConocimientosTecnicos ?? "[]") ?? new(); } catch {}
            try { funciones = JsonSerializer.Deserialize<List<string>>(p.FuncionesRelevantes ?? "[]") ?? new(); } catch {}
            try { logros = JsonSerializer.Deserialize<List<string>>(p.LogrosRelevantes ?? "[]") ?? new(); } catch {}

            return new {
                postulanteExternoId = p.Id,
                codigoPostulanteExterno = p.CodigoPostulanteExterno,
                perfilCargoId = p.PerfilCargoId,
                nombreCargoPostulado = p.NombreCargoPostulado,
                fechaPostulacion = p.FechaPostulacion,
                pretensionSalarialBs = p.PretensionSalarialBs,
                pretensionNegociable = p.PretensionNegociable,
                disponibilidadIncorporacion = p.DisponibilidadIncorporacion,
                motivacionPostulacion = p.MotivacionPostulacion,
                nombresApellidos = p.NombresApellidos,
                edad = p.Edad,
                ciudadResidencia = p.CiudadResidencia,
                direccion = p.Direccion,
                numeroCelular = p.NumeroCelular,
                correoElectronico = p.CorreoElectronico,
                ciIdentidad = p.CiIdentidad,
                estadoCivil = p.EstadoCivil,
                numeroHijos = p.NumeroHijos,
                colegio = p.Colegio,
                carreraInstitucionUniversitaria = p.CarreraInstitucionUniversitaria,
                estadoAcademico = p.EstadoAcademico,
                postgradoInstitucion = postgrado,
                maestriaInstitucion = maestria,
                experienciasLaborales = exp,
                seniority = p.Seniority,
                certificaciones = certs,
                cursosComplementarios = cursos,
                idiomas = idiomas,
                experienciaTotalAnios = p.ExperienciaTotalAnios,
                experienciaLiderazgoAnios = p.ExperienciaLiderazgoAnios,
                sectoresExperiencia = sectores,
                hardSkills = hard,
                softSkills = soft,
                herramientasSistemas = herramientas,
                conocimientosTecnicos = conTecnicos,
                funcionesRelevantes = funciones,
                logrosRelevantes = logros
            };
        }).ToList();

        return Ok(response);
    }

    [HttpGet("postulantes-internos/filtrados/{perfilCargoId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPostulantesInternosFiltrados([FromRoute] int perfilCargoId)
    {
        var perfil = await _dbContext.PerfilesCargo
            .Include(p => p.Solicitud)
            .FirstOrDefaultAsync(p => p.Id == perfilCargoId && !p.IsDeleted);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto { Code = "Perfil.NotFound", Message = $"No se encontró el perfil de cargo con ID {perfilCargoId}.", CorrelationId = GetCorrelationId() });
        }

        // Obtener el salario del perfil
        decimal? salarioMaximo = null;
        string? salRawExt = perfil.Salario;
        if (!string.IsNullOrWhiteSpace(salRawExt))
        {
            if (decimal.TryParse(salRawExt, out decimal salMax))
            {
                salarioMaximo = salMax;
            }
            else
            {
                var cleanMax = new string(salRawExt.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                if (decimal.TryParse(cleanMax, out decimal salCleanMax))
                {
                    salarioMaximo = salCleanMax;
                }
            }
        }

        if (salarioMaximo == null)
        {
            // Intentar desde ResumenEjecutivo
            var resumen = await _dbContext.ResumenEjecutivos
                .FirstOrDefaultAsync(r => r.PerfilCargoId == perfilCargoId);
            if (resumen != null && !string.IsNullOrWhiteSpace(resumen.BandaSalarial))
            {
                if (decimal.TryParse(resumen.BandaSalarial, out decimal salRes))
                {
                    salarioMaximo = salRes;
                }
                else
                {
                    var parts = resumen.BandaSalarial.Split(new[] { '-', 'a' }, StringSplitOptions.RemoveEmptyEntries);
                    var lastPart = parts.LastOrDefault() ?? resumen.BandaSalarial;
                    var cleanRes = new string(lastPart.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                    if (decimal.TryParse(cleanRes, out decimal salCleanRes))
                    {
                        salarioMaximo = salCleanRes;
                    }
                }
            }
        }

        // Si aun es null, usar fallback alto para no bloquear
        decimal limiteSalario = salarioMaximo ?? 999999;

        var postulantes = await _dbContext.PostulantesInternos
            .Where(p => p.PerfilCargoId == perfilCargoId && !p.IsDeleted)
            .ToListAsync();

        var filtrados = postulantes.Where(p => {
            // Filtro Regional: Internos deben tener DisponibleCambioRegional == "si"
            if (!string.Equals(p.DisponibleCambioRegional, "si", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filtro Salarial: PretensionSalarialBs <= limiteSalario OR (PretensionSalarialBs > limiteSalario AND PretensionNegociable == "si")
            if (p.PretensionSalarialBs <= limiteSalario)
            {
                return true;
            }
            return string.Equals(p.PretensionNegociable, "si", StringComparison.OrdinalIgnoreCase);
        }).ToList();

        // Mapear a respuesta con arreglos deserializados para n8n
        var response = filtrados.Select(p => {
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

            try { postgrado = JsonSerializer.Deserialize<List<string>>(p.PostgradoInstitucion ?? "[]") ?? new(); } catch {}
            try { maestria = JsonSerializer.Deserialize<List<string>>(p.MaestriaInstitucion ?? "[]") ?? new(); } catch {}
            try { certs = JsonSerializer.Deserialize<List<string>>(p.Certificaciones ?? "[]") ?? new(); } catch {}
            try { cursos = JsonSerializer.Deserialize<List<string>>(p.CursosComplementarios ?? "[]") ?? new(); } catch {}
            try { idiomas = JsonSerializer.Deserialize<List<string>>(p.Idiomas ?? "[]") ?? new(); } catch {}
            try { sectores = JsonSerializer.Deserialize<List<string>>(p.SectoresExperienciaJson ?? "[]") ?? new(); } catch {}
            try { funciones = JsonSerializer.Deserialize<List<string>>(p.FuncionesActualesJson ?? "[]") ?? new(); } catch {}
            try { hard = JsonSerializer.Deserialize<List<string>>(p.HardSkillsJson ?? "[]") ?? new(); } catch {}
            try { soft = JsonSerializer.Deserialize<List<string>>(p.SoftSkillsJson ?? "[]") ?? new(); } catch {}
            try { herramientas = JsonSerializer.Deserialize<List<string>>(p.HerramientasSistemasJson ?? "[]") ?? new(); } catch {}
            try { conTecnicos = JsonSerializer.Deserialize<List<string>>(p.ConocimientosTecnicosJson ?? "[]") ?? new(); } catch {}
            try { logros = JsonSerializer.Deserialize<List<string>>(p.LogrosRelevantesJson ?? "[]") ?? new(); } catch {}

            return new {
                postulanteInternoId = p.Id,
                codigoPostulanteInterno = p.CodigoPostulanteInterno,
                perfilCargoId = p.PerfilCargoId,
                nombreCargoPostulado = p.NombreCargoPostulado,
                fechaPostulacion = p.FechaPostulacion,
                pretensionSalarialBs = p.PretensionSalarialBs,
                pretensionNegociable = p.PretensionNegociable,
                vinculoGrupoNacional = p.VinculoGrupoNacional,
                detalleVinculoGrupo = p.DetalleVinculoGrupo,
                vinculoSectorFinanciero = p.VinculoSectorFinanciero,
                disponibilidadIncorporacion = p.DisponibilidadIncorporacion,
                disponibleCambioRegional = p.DisponibleCambioRegional,
                nombresApellidos = p.NombresApellidos,
                edad = p.Edad,
                ciudadResidencia = p.CiudadResidencia,
                direccion = p.Direccion,
                numeroCelular = p.NumeroCelular,
                correoElectronico = p.CorreoElectronico,
                ciIdentidad = p.CiIdentidad,
                estadoCivil = p.EstadoCivil,
                numeroHijos = p.NumeroHijos,
                colegio = p.Colegio,
                carreraInstitucionUniversitaria = p.CarreraInstitucionUniversitaria,
                estadoAcademico = p.EstadoAcademico,
                postgradoInstitucion = postgrado,
                maestriaInstitucion = maestria,
                empresaActual = p.EmpresaActual,
                areaActualTrabajo = p.AreaActualTrabajo,
                supervisorNombreCargo = p.SupervisorNombreCargo,
                cargoActual = p.CargoActual,
                motivacionPostulacion = p.MotivacionPostulacion,
                fechaIngresoCompania = p.FechaIngresoCompania,
                seniorityActual = p.SeniorityActual,
                certificaciones = certs,
                cursosComplementarios = cursos,
                idiomas = idiomas,
                experienciaTotalAnios = p.ExperienciaTotalAnios,
                experienciaRelevanteAnios = p.ExperienciaRelevanteAnios,
                sectoresExperienciaJson = sectores,
                funcionesActualesJson = funciones,
                hardSkillsJson = hard,
                softSkillsJson = soft,
                herramientasSistemasJson = herramientas,
                conocimientosTecnicosJson = conTecnicos,
                logrosRelevantesJson = logros
            };
        }).ToList();

        return Ok(response);
    }

    [HttpPost("postulantes-externos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarPostulanteExternoInterno([FromBody] PostulanteExternoSaveDto dto)
    {
        if (dto == null) return BadRequest(new ApiErrorDto { Code = "BAD_REQUEST", Message = "Los datos de postulación son requeridos.", CorrelationId = GetCorrelationId() });

        string userEmail = "n8n-system@nacionalseguros.com.bo";

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

        _dbContext.PostulantesExternos.Add(entity);
        await _dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new {
            postulanteExternoId = entity.Id,
            codigoPostulanteExterno = entity.CodigoPostulanteExterno,
            perfilCargoId = entity.PerfilCargoId,
            nombresApellidos = entity.NombresApellidos,
            fechaPostulacion = entity.FechaPostulacion
        });
    }

    [HttpPost("postulantes-internos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarPostulanteInternoInterno([FromBody] PostulanteInternoSaveDto dto)
    {
        if (dto == null) return BadRequest(new ApiErrorDto { Code = "BAD_REQUEST", Message = "Los datos de postulación son requeridos.", CorrelationId = GetCorrelationId() });

        string userEmail = "n8n-system@nacionalseguros.com.bo";

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

        _dbContext.PostulantesInternos.Add(entity);
        await _dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new {
            postulanteInternoId = entity.Id,
            codigoPostulanteInterno = entity.CodigoPostulanteInterno,
            perfilCargoId = entity.PerfilCargoId,
            nombresApellidos = entity.NombresApellidos,
            fechaPostulacion = entity.FechaPostulacion
        });
    }

    [HttpGet("matching-ejecuciones/{perfilCargoId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerMatchingEjecucionesPorPerfil([FromRoute] int perfilCargoId)
    {
        var ejecuciones = await _dbContext.MatchingEjecuciones
            .Where(e => e.PerfilCargoId == perfilCargoId)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();

        var response = ejecuciones.Select(entity => {
            object fuentes = new List<object>();
            object descartes = new List<object>();
            object parametros = new object();
            try { fuentes = JsonSerializer.Deserialize<object>(entity.FuentesConsultadasJson ?? "[]") ?? new List<object>(); } catch { }
            try { descartes = JsonSerializer.Deserialize<object>(entity.ResumenDescartesJson ?? "[]") ?? new List<object>(); } catch { }
            try { parametros = JsonSerializer.Deserialize<object>(entity.ParametrosMatchingJson ?? "{}") ?? new object(); } catch { }

            return new {
                matchingEjecucionId = entity.Id,
                codigoMatching = entity.CodigoMatching,
                perfilCargoId = entity.PerfilCargoId,
                perfilEstructuradoId = entity.PerfilEstructuradoId,
                versionPerfil = entity.VersionPerfil,
                estadoId = entity.EstadoId,
                totalEvaluados = entity.TotalEvaluados,
                totalInternosEvaluados = entity.TotalInternosEvaluados,
                totalHistoricosEvaluados = entity.TotalHistoricosEvaluados,
                totalMatchAlto = entity.TotalMatchAlto,
                totalMatchMedio = entity.TotalMatchMedio,
                totalMatchBajo = entity.TotalMatchBajo,
                totalDescartados = entity.TotalDescartados,
                totalPotenciales = entity.TotalPotenciales,
                compatibilidadPromedio = entity.CompatibilidadPromedio,
                estrategiaRecomendada = entity.EstrategiaRecomendada,
                nivelConfianza = entity.NivelConfianza,
                justificacion = entity.Justificacion,
                fuentesConsultadas = fuentes,
                resumenDescartes = descartes,
                parametrosMatching = parametros,
                fechaInicio = entity.FechaInicio,
                fechaFin = entity.FechaFin,
                createdDate = entity.CreatedDate,
                mensajeError = entity.MensajeError
            };
        }).ToList();

        return Ok(response);
    }

    [HttpPost("matching-ejecuciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearMatchingEjecucionInterna([FromBody] CrearMatchingEjecucionRequest request)
    {
        if (request == null) return BadRequest("Los datos son requeridos.");

        // Validar Perfil de Cargo
        var perfil = await _dbContext.PerfilesCargo
            .FirstOrDefaultAsync(p => p.Id == request.PerfilCargoId && !p.IsDeleted);

        if (perfil == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "PerfilCargo.NotFound",
                Message = $"No se encontró el perfil de cargo con ID {request.PerfilCargoId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Obtener PerfilEstructurado correspondiente a la Solicitud del PerfilCargo
        var perfilEstructurado = await _dbContext.PerfilEstructurados
            .FirstOrDefaultAsync(pe => pe.SolicitudId == perfil.SolicitudId);

        if (perfilEstructurado == null)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = "PerfilEstructurado.NotFound",
                Message = $"No se encontró un perfil estructurado relacionado al perfil de cargo {request.PerfilCargoId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        var fuentesJson = request.FuentesConsultadas != null ? JsonSerializer.Serialize(request.FuentesConsultadas) : "[]";
        var descartesJson = request.ResumenDescartes != null ? JsonSerializer.Serialize(request.ResumenDescartes) : "[]";
        var parametrosJson = request.ParametrosMatching != null ? JsonSerializer.Serialize(request.ParametrosMatching) : "{}";

        // Verificar si ya existe una ejecución de matching para este PerfilCargoId (Upsert)
        var entity = await _dbContext.MatchingEjecuciones
            .FirstOrDefaultAsync(e => e.PerfilCargoId == request.PerfilCargoId);

        if (entity != null)
        {
            var entry = _dbContext.Entry(entity);
            entry.Property(p => p.PerfilEstructuradoId).CurrentValue = perfilEstructurado.Id;
            entry.Property(p => p.VersionPerfil).CurrentValue = entity.VersionPerfil + 1;
            entry.Property(p => p.EstadoId).CurrentValue = request.EstadoId;
            entry.Property(p => p.TotalEvaluados).CurrentValue = request.TotalEvaluados;
            entry.Property(p => p.TotalInternosEvaluados).CurrentValue = request.TotalInternosEvaluados;
            entry.Property(p => p.TotalHistoricosEvaluados).CurrentValue = request.TotalHistoricosEvaluados;
            entry.Property(p => p.TotalMatchAlto).CurrentValue = request.TotalMatchAlto;
            entry.Property(p => p.TotalMatchMedio).CurrentValue = request.TotalMatchMedio;
            entry.Property(p => p.TotalMatchBajo).CurrentValue = request.TotalMatchBajo;
            entry.Property(p => p.TotalDescartados).CurrentValue = request.TotalDescartados;
            entry.Property(p => p.TotalPotenciales).CurrentValue = request.TotalPotenciales;
            entry.Property(p => p.CompatibilidadPromedio).CurrentValue = request.CompatibilidadPromedio;
            entry.Property(p => p.EstrategiaRecomendada).CurrentValue = request.EstrategiaRecomendada;
            entry.Property(p => p.NivelConfianza).CurrentValue = request.NivelConfianza;
            entry.Property(p => p.Justificacion).CurrentValue = request.Justificacion;
            entry.Property(p => p.FuentesConsultadasJson).CurrentValue = fuentesJson;
            entry.Property(p => p.ResumenDescartesJson).CurrentValue = descartesJson;
            entry.Property(p => p.ParametrosMatchingJson).CurrentValue = parametrosJson;
            entry.Property(p => p.FechaInicio).CurrentValue = request.FechaInicio == default ? DateTime.UtcNow : request.FechaInicio;
            entry.Property(p => p.FechaFin).CurrentValue = request.FechaFin;
            entry.Property(p => p.MensajeError).CurrentValue = request.MensajeError;

            _dbContext.MatchingEjecuciones.Update(entity);
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
                perfilCargoId: request.PerfilCargoId,
                perfilEstructuradoId: perfilEstructurado.Id,
                versionPerfil: 1,
                estadoId: request.EstadoId,
                totalEvaluados: request.TotalEvaluados,
                totalInternosEvaluados: request.TotalInternosEvaluados,
                totalHistoricosEvaluados: request.TotalHistoricosEvaluados,
                totalMatchAlto: request.TotalMatchAlto,
                totalMatchMedio: request.TotalMatchMedio,
                totalMatchBajo: request.TotalMatchBajo,
                totalDescartados: request.TotalDescartados,
                totalPotenciales: request.TotalPotenciales,
                fuentesConsultadasJson: fuentesJson,
                resumenDescartesJson: descartesJson,
                parametrosMatchingJson: parametrosJson,
                fechaInicio: request.FechaInicio == default ? DateTime.UtcNow : request.FechaInicio,
                compatibilidadPromedio: request.CompatibilidadPromedio,
                estrategiaRecomendada: request.EstrategiaRecomendada,
                nivelConfianza: request.NivelConfianza,
                justificacion: request.Justificacion,
                fechaFin: request.FechaFin,
                mensajeError: request.MensajeError
            );
            entity.SetCodigoMatching(codigoMatching);

            _dbContext.MatchingEjecuciones.Add(entity);
        }

        await _dbContext.SaveChangesAsync();

        object fuentes = new List<object>();
        object descartes = new List<object>();
        object parametros = new object();
        try { fuentes = JsonSerializer.Deserialize<object>(entity.FuentesConsultadasJson ?? "[]") ?? new List<object>(); } catch { }
        try { descartes = JsonSerializer.Deserialize<object>(entity.ResumenDescartesJson ?? "[]") ?? new List<object>(); } catch { }
        try { parametros = JsonSerializer.Deserialize<object>(entity.ParametrosMatchingJson ?? "{}") ?? new object(); } catch { }

        var response = new {
            matchingEjecucionId = entity.Id,
            codigoMatching = entity.CodigoMatching,
            perfilCargoId = entity.PerfilCargoId,
            perfilEstructuradoId = entity.PerfilEstructuradoId,
            versionPerfil = entity.VersionPerfil,
            estadoId = entity.EstadoId,
            totalEvaluados = entity.TotalEvaluados,
            totalInternosEvaluados = entity.TotalInternosEvaluados,
            totalHistoricosEvaluados = entity.TotalHistoricosEvaluados,
            totalMatchAlto = entity.TotalMatchAlto,
            totalMatchMedio = entity.TotalMatchMedio,
            totalMatchBajo = entity.TotalMatchBajo,
            totalDescartados = entity.TotalDescartados,
            totalPotenciales = entity.TotalPotenciales,
            compatibilidadPromedio = entity.CompatibilidadPromedio,
            estrategiaRecomendada = entity.EstrategiaRecomendada,
            nivelConfianza = entity.NivelConfianza,
            justificacion = entity.Justificacion,
            fuentesConsultadas = fuentes,
            resumenDescartes = descartes,
            parametrosMatching = parametros,
            fechaInicio = entity.FechaInicio,
            fechaFin = entity.FechaFin,
            createdDate = entity.CreatedDate,
            mensajeError = entity.MensajeError
        };

        return Created("", response);
    }

    [HttpGet("matching-resultados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerMatchingResultado([FromQuery] int matchingEjecucionId, [FromQuery] int postulanteId, [FromQuery] string origen)
    {
        if (string.IsNullOrWhiteSpace(origen)) return BadRequest("El origen es requerido.");

        var entity = await _dbContext.MatchingResultados.AsNoTracking()
            .FirstOrDefaultAsync(r => r.MatchingEjecucionId == matchingEjecucionId &&
                                      r.PostulanteId == postulanteId &&
                                      r.Origen == origen);

        if (entity == null)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingResultado.NotFound",
                Message = "No se encontró el resultado de matching con los parámetros provistos.",
                CorrelationId = GetCorrelationId()
            });
        }

        object fortalezas = new List<object>();
        object brechas = new List<object>();
        object desglose = new List<object>();
        try { fortalezas = JsonSerializer.Deserialize<object>(entity.FortalezasJson ?? "[]") ?? new List<object>(); } catch { }
        try { brechas = JsonSerializer.Deserialize<object>(entity.BrechasJson ?? "[]") ?? new List<object>(); } catch { }
        try { desglose = JsonSerializer.Deserialize<object>(entity.DesgloseCriteriosJson ?? "[]") ?? new List<object>(); } catch { }

        var response = new {
            matchingResultadoId = entity.Id,
            matchingEjecucionId = entity.MatchingEjecucionId,
            postulanteId = entity.PostulanteId,
            origen = entity.Origen,
            porcentajeMatching = entity.PorcentajeMatching,
            clasificacion = entity.Clasificacion,
            posicionRanking = entity.PosicionRanking,
            puntajeObtenido = entity.PuntajeObtenido,
            puntajeMaximo = entity.PuntajeMaximo,
            fortalezas = fortalezas,
            brechas = brechas,
            desgloseCriterios = desglose,
            tipoDescarte = entity.TipoDescarte,
            motivoExclusion = entity.MotivoExclusion,
            createdDate = entity.CreatedDate
        };

        return Ok(response);
    }

    [HttpPost("matching-resultados")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearMatchingResultadoInterno([FromBody] CrearMatchingResultadoRequest request)
    {
        if (request == null) return BadRequest("Los datos son requeridos.");

        // Validar MatchingEjecucion
        var ejecucionExists = await _dbContext.MatchingEjecuciones.AnyAsync(e => e.Id == request.MatchingEjecucionId);
        if (!ejecucionExists)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {request.MatchingEjecucionId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Validar Postulante y Origen
        if (string.Equals(request.Origen, "BD_INTERNA", StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _dbContext.PostulantesInternos.AnyAsync(p => p.Id == request.PostulanteId);
            if (!exists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Postulante.NotFound",
                    Message = $"No se encontró el postulante interno con ID {request.PostulanteId}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }
        else if (string.Equals(request.Origen, "BD_EXTERNA_HISTORICA", StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _dbContext.PostulantesExternos.AnyAsync(p => p.Id == request.PostulanteId);
            if (!exists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Postulante.NotFound",
                    Message = $"No se encontró el postulante externo con ID {request.PostulanteId}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }
        else if (string.Equals(request.Origen, "POOL_MANUAL", StringComparison.OrdinalIgnoreCase) ||
                 (request.Origen ?? "").ToUpper().Contains("POOL") ||
                 (request.Origen ?? "").ToUpper().Contains("MANUAL"))
        {
            var existsInt = await _dbContext.PostulantesInternos.AnyAsync(p => p.Id == request.PostulanteId);
            var existsExt = await _dbContext.PostulantesExternos.AnyAsync(p => p.Id == request.PostulanteId);
            if (!existsInt && !existsExt)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Postulante.NotFound",
                    Message = $"No se encontró el postulante con ID {request.PostulanteId}.",
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

        var fortalezasJson = request.Fortalezas != null ? JsonSerializer.Serialize(request.Fortalezas) : "[]";
        var brechasJson = request.Brechas != null ? JsonSerializer.Serialize(request.Brechas) : "[]";
        var desgloseJson = request.DesgloseCriterios != null ? JsonSerializer.Serialize(request.DesgloseCriterios) : "[]";

        // Implementar un Upsert para evitar duplicados si ya existe un resultado con el mismo MatchingEjecucionId y PostulanteId y Origen
        var entity = await _dbContext.MatchingResultados
            .FirstOrDefaultAsync(r => r.MatchingEjecucionId == request.MatchingEjecucionId &&
                                      r.PostulanteId == request.PostulanteId &&
                                      r.Origen == request.Origen);

        if (entity != null)
        {
            entity.Actualizar(
                porcentajeMatching: request.PorcentajeMatching,
                clasificacion: request.Clasificacion,
                posicionRanking: request.PosicionRanking,
                puntajeObtenido: request.PuntajeObtenido,
                puntajeMaximo: request.PuntajeMaximo,
                fortalezasJson: fortalezasJson,
                brechasJson: brechasJson,
                desgloseCriteriosJson: desgloseJson,
                tipoDescarte: request.TipoDescarte,
                motivoExclusion: request.MotivoExclusion
            );
            _dbContext.MatchingResultados.Update(entity);
        }
        else
        {
            entity = new MatchingResultado(
                matchingEjecucionId: request.MatchingEjecucionId,
                postulanteId: request.PostulanteId,
                origen: request.Origen ?? "POOL_MANUAL",
                porcentajeMatching: request.PorcentajeMatching,
                clasificacion: request.Clasificacion,
                posicionRanking: request.PosicionRanking,
                puntajeObtenido: request.PuntajeObtenido,
                puntajeMaximo: request.PuntajeMaximo,
                fortalezasJson: fortalezasJson,
                brechasJson: brechasJson,
                desgloseCriteriosJson: desgloseJson,
                tipoDescarte: request.TipoDescarte,
                motivoExclusion: request.MotivoExclusion
            );
            _dbContext.MatchingResultados.Add(entity);
        }

        await _dbContext.SaveChangesAsync();

        object resultFortalezas = new List<object>();
        object resultBrechas = new List<object>();
        object resultDesglose = new List<object>();
        try { resultFortalezas = JsonSerializer.Deserialize<object>(entity.FortalezasJson ?? "[]") ?? new List<object>(); } catch { }
        try { resultBrechas = JsonSerializer.Deserialize<object>(entity.BrechasJson ?? "[]") ?? new List<object>(); } catch { }
        try { resultDesglose = JsonSerializer.Deserialize<object>(entity.DesgloseCriteriosJson ?? "[]") ?? new List<object>(); } catch { }

        var response = new {
            matchingResultadoId = entity.Id,
            matchingEjecucionId = entity.MatchingEjecucionId,
            postulanteId = entity.PostulanteId,
            origen = entity.Origen,
            porcentajeMatching = entity.PorcentajeMatching,
            clasificacion = entity.Clasificacion,
            posicionRanking = entity.PosicionRanking,
            puntajeObtenido = entity.PuntajeObtenido,
            puntajeMaximo = entity.PuntajeMaximo,
            fortalezas = resultFortalezas,
            brechas = resultBrechas,
            desgloseCriterios = resultDesglose,
            tipoDescarte = entity.TipoDescarte,
            motivoExclusion = entity.MotivoExclusion,
            createdDate = entity.CreatedDate
        };

        return Created("", response);
    }

    [HttpPost("matching-ejecuciones/{matchingEjecucionId:int}/estrategia-interna")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearEstrategiaInternaInterno([FromRoute] int matchingEjecucionId, [FromBody] CrearEstrategiaInternaRequest request)
    {
        if (request == null) return BadRequest("Los datos son requeridos.");
        if (matchingEjecucionId <= 0) return BadRequest("El ID de ejecución de matching debe ser mayor a 0.");

        // Validar MatchingEjecucion
        var ejecucionExists = await _dbContext.MatchingEjecuciones.AnyAsync(e => e.Id == matchingEjecucionId);
        if (!ejecucionExists)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {matchingEjecucionId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Validar EstadoId si se envía
        if (request.EstadoId.HasValue)
        {
            var estadoExists = await _dbContext.Estados.AnyAsync(e => e.Id == request.EstadoId.Value);
            if (!estadoExists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Estado.NotFound",
                    Message = $"No se encontró el estado con ID {request.EstadoId.Value}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }

        var planAccionStr = request.PlanAccion != null ? (request.PlanAccion is string s1 ? s1 : JsonSerializer.Serialize(request.PlanAccion)) : "{}";
        var planEvaluacionStr = request.PlanEvaluacion != null ? (request.PlanEvaluacion is string s2 ? s2 : JsonSerializer.Serialize(request.PlanEvaluacion)) : "{}";

        // Implementar un Upsert para evitar duplicados debido al índice único
        var entity = await _dbContext.EstrategiaInternas
            .FirstOrDefaultAsync(e => e.MatchingEjecucionId == matchingEjecucionId);

        if (entity != null)
        {
            entity.Actualizar(
                prioridad: request.Prioridad,
                justificacion: request.Justificacion,
                planAccion: planAccionStr,
                planEvaluacion: planEvaluacionStr,
                mensajeContingencia: request.MensajeContingencia,
                conclusion: request.Conclusion,
                estadoId: request.EstadoId
            );
            _dbContext.EstrategiaInternas.Update(entity);
        }
        else
        {
            entity = new EstrategiaInterna(matchingEjecucionId);
            entity.Actualizar(
                prioridad: request.Prioridad,
                justificacion: request.Justificacion,
                planAccion: planAccionStr,
                planEvaluacion: planEvaluacionStr,
                mensajeContingencia: request.MensajeContingencia,
                conclusion: request.Conclusion,
                estadoId: request.EstadoId
            );
            _dbContext.EstrategiaInternas.Add(entity);
        }

        await _dbContext.SaveChangesAsync();

        object resultPlanAccion = new object();
        object resultPlanEvaluacion = new object();
        try { resultPlanAccion = JsonSerializer.Deserialize<object>(entity.PlanAccion ?? "{}") ?? new object(); } catch { }
        try { resultPlanEvaluacion = JsonSerializer.Deserialize<object>(entity.PlanEvaluacion ?? "{}") ?? new object(); } catch { }

        var response = new {
            estrategiaInternaId = entity.Id,
            matchingEjecucionId = entity.MatchingEjecucionId,
            prioridad = entity.Prioridad,
            justificacion = entity.Justificacion,
            planAccion = resultPlanAccion,
            planEvaluacion = resultPlanEvaluacion,
            mensajeContingencia = entity.MensajeContingencia,
            conclusion = entity.Conclusion,
            estadoId = entity.EstadoId,
            fechaCreacion = entity.FechaCreacion,
            fechaModificacion = entity.FechaModificacion
        };

        return Created("", response);
    }

    [HttpPost("matching-ejecuciones/{matchingEjecucionId:int}/estrategia-externa")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearEstrategiaExternaInterno([FromRoute] int matchingEjecucionId, [FromBody] CrearEstrategiaExternaRequest request)
    {
        if (request == null) return BadRequest("Los datos son requeridos.");
        if (matchingEjecucionId <= 0) return BadRequest("El ID de ejecución de matching debe ser mayor a 0.");

        // Validar MatchingEjecucion
        var ejecucionExists = await _dbContext.MatchingEjecuciones.AnyAsync(e => e.Id == matchingEjecucionId);
        if (!ejecucionExists)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {matchingEjecucionId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Validar EstadoId si se envía
        if (request.EstadoId.HasValue)
        {
            var estadoExists = await _dbContext.Estados.AnyAsync(e => e.Id == request.EstadoId.Value);
            if (!estadoExists)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "Estado.NotFound",
                    Message = $"No se encontró el estado con ID {request.EstadoId.Value}.",
                    CorrelationId = GetCorrelationId()
                });
            }
        }

        var criteriosDificilesStr = request.CriteriosDificiles != null ? (request.CriteriosDificiles is string s1 ? s1 : JsonSerializer.Serialize(request.CriteriosDificiles)) : "{}";
        var publicoObjetivoStr = request.PublicoObjetivo != null ? (request.PublicoObjetivo is string s2 ? s2 : JsonSerializer.Serialize(request.PublicoObjetivo)) : "{}";
        var canalesSugeridosStr = request.CanalesSugeridos != null ? (request.CanalesSugeridos is string s3 ? s3 : JsonSerializer.Serialize(request.CanalesSugeridos)) : "{}";
        var planAccionStr = request.PlanAccion != null ? (request.PlanAccion is string s4 ? s4 : JsonSerializer.Serialize(request.PlanAccion)) : "{}";
        var briefingEditableStr = request.BriefingEditable != null ? (request.BriefingEditable is string s5 ? s5 : JsonSerializer.Serialize(request.BriefingEditable)) : "{}";

        // Implementar un Upsert para evitar duplicados debido al índice único
        var entity = await _dbContext.EstrategiaExternas
            .FirstOrDefaultAsync(e => e.MatchingEjecucionId == matchingEjecucionId);

        if (entity != null)
        {
            entity.Actualizar(
                prioridad: request.Prioridad,
                justificacion: request.Justificacion,
                criteriosDificiles: criteriosDificilesStr,
                publicoObjetivo: publicoObjetivoStr,
                canalesSugeridos: canalesSugeridosStr,
                planAccion: planAccionStr,
                briefingEditable: briefingEditableStr,
                conclusion: request.Conclusion,
                estadoId: request.EstadoId
            );
            _dbContext.EstrategiaExternas.Update(entity);
        }
        else
        {
            entity = new EstrategiaExterna(matchingEjecucionId);
            entity.Actualizar(
                prioridad: request.Prioridad,
                justificacion: request.Justificacion,
                criteriosDificiles: criteriosDificilesStr,
                publicoObjetivo: publicoObjetivoStr,
                canalesSugeridos: canalesSugeridosStr,
                planAccion: planAccionStr,
                briefingEditable: briefingEditableStr,
                conclusion: request.Conclusion,
                estadoId: request.EstadoId
            );
            _dbContext.EstrategiaExternas.Add(entity);
        }

        await _dbContext.SaveChangesAsync();

        object resultCriteriosDificiles = new object();
        object resultPublicoObjetivo = new object();
        object resultCanalesSugeridos = new object();
        object resultPlanAccion = new object();
        object resultBriefingEditable = new object();

        try { resultCriteriosDificiles = JsonSerializer.Deserialize<object>(entity.CriteriosDificiles ?? "{}") ?? new object(); } catch { }
        try { resultPublicoObjetivo = JsonSerializer.Deserialize<object>(entity.PublicoObjetivo ?? "{}") ?? new object(); } catch { }
        try { resultCanalesSugeridos = JsonSerializer.Deserialize<object>(entity.CanalesSugeridos ?? "{}") ?? new object(); } catch { }
        try { resultPlanAccion = JsonSerializer.Deserialize<object>(entity.PlanAccion ?? "{}") ?? new object(); } catch { }
        try { resultBriefingEditable = JsonSerializer.Deserialize<object>(entity.BriefingEditable ?? "{}") ?? new object(); } catch { }

        var response = new {
            estrategiaExternaId = entity.Id,
            matchingEjecucionId = entity.MatchingEjecucionId,
            prioridad = entity.Prioridad,
            justificacion = entity.Justificacion,
            criteriosDificiles = resultCriteriosDificiles,
            publicoObjetivo = resultPublicoObjetivo,
            canalesSugeridos = resultCanalesSugeridos,
            planAccion = resultPlanAccion,
            briefingEditable = resultBriefingEditable,
            conclusion = entity.Conclusion,
            estadoId = entity.EstadoId,
            fechaCreacion = entity.FechaCreacion,
            fechaModificacion = entity.FechaModificacion
        };

        return Created("", response);
    }

    [HttpGet("matching-ejecuciones/{matchingEjecucionId:int}/resultados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerResultadosPorEjecucionId([FromRoute] int matchingEjecucionId)
    {
        if (matchingEjecucionId <= 0) return BadRequest("El ID de ejecución de matching debe ser mayor a 0.");

        // Validar si existe la ejecución de matching
        var ejecucionExists = await _dbContext.MatchingEjecuciones.AnyAsync(e => e.Id == matchingEjecucionId);
        if (!ejecucionExists)
        {
            return NotFound(new ApiErrorDto
            {
                Code = "MatchingEjecucion.NotFound",
                Message = $"No se encontró la ejecución de matching con ID {matchingEjecucionId}.",
                CorrelationId = GetCorrelationId()
            });
        }

        var resultados = await _dbContext.MatchingResultados.AsNoTracking()
            .Where(r => r.MatchingEjecucionId == matchingEjecucionId)
            .ToListAsync();

        var responseList = resultados.Select(entity => {
            object resultFortalezas = new List<object>();
            object resultBrechas = new List<object>();
            object resultDesglose = new List<object>();
            try { resultFortalezas = JsonSerializer.Deserialize<object>(entity.FortalezasJson ?? "[]") ?? new List<object>(); } catch { }
            try { resultBrechas = JsonSerializer.Deserialize<object>(entity.BrechasJson ?? "[]") ?? new List<object>(); } catch { }
            try { resultDesglose = JsonSerializer.Deserialize<object>(entity.DesgloseCriteriosJson ?? "[]") ?? new List<object>(); } catch { }

            return new {
                matchingResultadoId = entity.Id,
                matchingEjecucionId = entity.MatchingEjecucionId,
                postulanteId = entity.PostulanteId,
                origen = entity.Origen,
                porcentajeMatching = entity.PorcentajeMatching,
                clasificacion = entity.Clasificacion,
                posicionRanking = entity.PosicionRanking,
                puntajeObtenido = entity.PuntajeObtenido,
                puntajeMaximo = entity.PuntajeMaximo,
                fortalezas = resultFortalezas,
                brechas = resultBrechas,
                desgloseCriterios = resultDesglose,
                tipoDescarte = entity.TipoDescarte,
                motivoExclusion = entity.MotivoExclusion,
                createdDate = entity.CreatedDate
            };
        }).ToList();

        return Ok(responseList);
    }

    [HttpGet("areas-cargo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerAreasCargoInterno([FromQuery] bool? soloActivos = null)
    {
        var query = _dbContext.AreasCargo.Where(a => !a.IsDeleted);
        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(a => a.Estado == "Activo");
        }

        var list = await query
            .OrderBy(a => a.Id)
            .Select(a => new {
                id = a.Id,
                codigo = a.Codigo,
                nombre = a.Nombre,
                descripcion = a.Descripcion,
                estado = a.Estado,
                isDeleted = a.IsDeleted
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("cargos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCargosInterno([FromQuery] int? areaCargoId = null, [FromQuery] bool? soloActivos = null)
    {
        var query = _dbContext.Cargos.Include(c => c.AreaCargo).Where(c => !c.IsDeleted);
        if (areaCargoId.HasValue && areaCargoId.Value > 0)
        {
            query = query.Where(c => c.AreaCargoId == areaCargoId.Value);
        }
        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(c => c.Estado == "Activo");
        }

        var list = await query
            .OrderBy(c => c.Id)
            .Select(c => new {
                id = c.Id,
                areaCargoId = c.AreaCargoId,
                areaCargoNombre = c.AreaCargo.Nombre,
                codigo = c.Codigo,
                nombre = c.Nombre,
                descripcion = c.Descripcion,
                estado = c.Estado,
                isDeleted = c.IsDeleted
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("areas-cargo/{areaCargoId:int}/cargos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCargosPorAreaCargoInterno([FromRoute] int areaCargoId, [FromQuery] bool? soloActivos = null)
    {
        var query = _dbContext.Cargos.Include(c => c.AreaCargo)
            .Where(c => !c.IsDeleted && c.AreaCargoId == areaCargoId);

        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(c => c.Estado == "Activo");
        }

        var list = await query
            .OrderBy(c => c.Id)
            .Select(c => new {
                id = c.Id,
                areaCargoId = c.AreaCargoId,
                areaCargoNombre = c.AreaCargo.Nombre,
                codigo = c.Codigo,
                nombre = c.Nombre,
                descripcion = c.Descripcion,
                estado = c.Estado,
                isDeleted = c.IsDeleted
            })
            .ToListAsync();

        return Ok(list);
    }
}

public class CrearObservacionRequest
{
    public int SolicitudId { get; set; }
    public string Texto { get; set; } = string.Empty;
}

public class ValidarCanalRequest
{
    public string ChannelType { get; set; } = "WhatsApp";
    public string ChannelIdentifier { get; set; } = string.Empty;
}

public class GuardarPdfForm
{
    public IFormFile? File { get; set; }
    public string? PublicUrl { get; set; }
}

public class CrearPerfilObservacionInternaRequest
{
    public int PerfilCargoId { get; set; }
    public int TipoObservacionId { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
}

public class DecisionAreaRequest
{
    public string Decision { get; set; } = string.Empty;
    public List<ObservacionItem>? Observaciones { get; set; }
    public int? UsuarioId { get; set; }
}

public class ObservacionItem
{
    public int TipoObservacionId { get; set; }
    public string Comentario { get; set; } = string.Empty;
}

public class CrearMatchingEjecucionRequest
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

public class CrearMatchingResultadoRequest
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

public class CrearEstrategiaInternaRequest
{
    public string? Prioridad { get; set; }
    public string? Justificacion { get; set; }
    public object? PlanAccion { get; set; }
    public object? PlanEvaluacion { get; set; }
    public string? MensajeContingencia { get; set; }
    public string? Conclusion { get; set; }
    public int? EstadoId { get; set; }
}

public class CrearEstrategiaExternaRequest
{
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
