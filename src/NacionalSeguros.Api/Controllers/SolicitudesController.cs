using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Solicitudes.Commands.ActualizarSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.CancelarSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.CrearSolicitud;
using NacionalSeguros.Application.Solicitudes.Commands.TransitarSolicitud;
using NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudById;
using NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudStateHistory;
using NacionalSeguros.Application.Solicitudes.Queries.ListSolicitudesKanban;
using NacionalSeguros.Application.Solicitudes.Commands.EnviarSolicitudARRHH;
using NacionalSeguros.Application.Solicitudes.Queries.GetWhatsAppCostCheck;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public class SolicitudesController : ControllerBase
{
    private readonly ISender _sender;

    public SolicitudesController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    private string GetUserEmail()
    {
        return User.FindFirst("email")?.Value ??
               User.FindFirst(ClaimTypes.Email)?.Value ??
               (User.Identity?.Name != null && User.Identity.Name.Contains("@") ? User.Identity.Name : null) ??
               "system@nacionalseguros.com.bo";
    }

    [HttpPost]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Crear([FromBody] SolicitudCreateDto request)
    {
        string userEmail = GetUserEmail();
        var command = new CrearSolicitudCommand(request, userEmail, request.UsuarioId);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar crear la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value.SolicitudId }, result.Value);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        string userEmail = GetUserEmail();
        var query = new GetSolicitudByIdQuery(id, userEmail);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontró la solicitud con el ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] SolicitudUpdateDto request)
    {
        string userEmail = GetUserEmail();
        var command = new ActualizarSolicitudCommand(id, request, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar actualizar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/enviar")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnviarARRhh([FromRoute] int id)
    {
        string userEmail = GetUserEmail();
        var command = new EnviarSolicitudARRHHCommand(id, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Solicitud.NotFound")
            {
                return NotFound(new ApiErrorDto
                {
                    Code = result.Error.Code,
                    Message = result.Error.Message,
                    Detail = "La solicitud no existe.",
                    CorrelationId = GetCorrelationId()
                });
            }

            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar enviar la solicitud a RRHH.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/transicion")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TransitarEstado(
        [FromRoute] int id, 
        [FromBody] TransicionEstadoRequest request)
    {
        string userEmail = GetUserEmail();
        var command = new TransitarSolicitudEstadoCommand(
            id, 
            request.NuevoEstadoCodigo, 
            request.Comentario, 
            request.DecisorId, 
            userEmail,
            request.SeMostroAdvertencia,
            request.UsuarioConfirmoEnvio);
            
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al transitar el estado de la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}/whatsapp-cost-check")]
    [ProducesResponseType(typeof(WhatsAppCostCheckResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWhatsAppCostCheck([FromRoute] int id)
    {
        var query = new GetWhatsAppCostCheckQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al verificar costo de WhatsApp.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/aprobar")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Aprobar([FromRoute] int id, [FromBody] AprobarSolicitudRequest? request)
    {
        string userEmail = GetUserEmail();
        var command = new TransitarSolicitudEstadoCommand(
            id,
            "SOL-APR",
            request?.Observaciones,
            null,
            userEmail);

        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar aprobar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/rechazar")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Rechazar([FromRoute] int id, [FromBody] RechazarSolicitudRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Justificacion) || request.Justificacion.Length < 10)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = "Validation.Error",
                Message = "La justificación de rechazo es requerida y debe tener al menos 10 caracteres.",
                Detail = "Error de validación.",
                CorrelationId = GetCorrelationId()
            });
        }

        string userEmail = GetUserEmail();
        var command = new TransitarSolicitudEstadoCommand(
            id,
            "SOL-RECH",
            request.Justificacion,
            null,
            userEmail);

        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar rechazar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(SolicitudResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancelar([FromRoute] int id, [FromBody] CancelarSolicitudRequest request)
    {
        string userEmail = GetUserEmail();
        var command = new CancelarSolicitudCommand(id, request.Motivo, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al cancelar la solicitud.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedSolicitudesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarKanban(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? estadoId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool soloMisSolicitudes = false)
    {
        string userEmail = GetUserEmail();
        var query = new ListSolicitudesKanbanQuery(pageNumber, pageSize, estadoId, search, userEmail, soloMisSolicitudes);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al listar las solicitudes.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}/historial")]
    [ProducesResponseType(typeof(List<StateHistoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerHistorial([FromRoute] int id)
    {
        var query = new GetSolicitudStateHistoryQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al obtener el historial de estados.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
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

public class TransicionEstadoRequest
{
    public string NuevoEstadoCodigo { get; set; } = string.Empty;
    public string? Comentario { get; set; }
    public int? DecisorId { get; set; }
    public bool? SeMostroAdvertencia { get; set; }
    public bool? UsuarioConfirmoEnvio { get; set; }
}

public class CancelarSolicitudRequest
{
    public string Motivo { get; set; } = string.Empty;
}

public class AprobarSolicitudRequest
{
    public string? Observaciones { get; set; }
}

public class RechazarSolicitudRequest
{
    public string Justificacion { get; set; } = string.Empty;
}
