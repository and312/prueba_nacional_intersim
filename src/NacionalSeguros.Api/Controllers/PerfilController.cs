using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;
using NacionalSeguros.Application.Perfiles.Commands.GenerarPerfil;
using NacionalSeguros.Application.Perfiles.Commands.ObservarPerfilCargo;
using NacionalSeguros.Application.Perfiles.Queries.BuscarPerfiles;
using NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfil;
using NacionalSeguros.Application.Perfiles.Queries.ObtenerPerfilPorId;
using NacionalSeguros.Application.Perfiles.Commands.CorregirPerfil;
using NacionalSeguros.Application.Perfiles.Commands.AprobarPerfilRRHH;
using NacionalSeguros.Application.Perfiles.Commands.EnviarPerfilArea;
using NacionalSeguros.Application.Perfiles.Queries.GetPerfilTrazabilidad;
using NacionalSeguros.Application.Perfiles.Commands.ActualizarPerfilSeccion;
using NacionalSeguros.Application.Perfiles.Commands.RegenerarPdf;
using NacionalSeguros.Application.Perfiles.Queries.GetPerfilAuditoria;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using Microsoft.Extensions.Configuration;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class PerfilController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public PerfilController(ISender sender, IConfiguration configuration)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [Authorize]
    [HttpPost("perfiles/generar")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Generar([FromBody] GenerarPerfilRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new GenerarPerfilCommand(request.SolicitudId, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar iniciar la generacion de perfil.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Accepted();
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("perfiles-legacy/{id:int}")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var query = new ObtenerPerfilPorIdQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontro el perfil de cargo con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("perfiles/{id:int}/aprobar")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Aprobar([FromRoute] int id)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new AprobarPerfilCargoCommand(id, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar aprobar el perfil de cargo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("perfiles/{id:int}/observar")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Observar([FromRoute] int id, [FromBody] ObservarPerfilRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new ObservarPerfilCargoCommand(id, request.Justificacion, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar observar el perfil de cargo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("callbacks/perfil-creacion")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CallbackPerfilCreacion([FromBody] PerfilCreacionCallbackRequest request)
    {
        // Validar X-API-Key para la integracion de n8n
        var expectedKey = _configuration["IntegrationSettings:N8nApiKey"] ?? "n8n_secret_key_123";
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKey) || apiKey != expectedKey)
        {
            return Unauthorized(new ApiErrorDto
            {
                Code = "UNAUTHORIZED_CALLBACK",
                Message = "La cabecera X-API-Key es requerida o no es valida.",
                Detail = "Solo los agentes de IA de n8n estan autorizados para invocar este callback.",
                CorrelationId = GetCorrelationId()
            });
        }

        var command = new CrearPerfilCargoCommand(request, "AgentePerfil");
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar crear el perfil de cargo via callback.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("perfiles-buscar-legacy")]
    [ProducesResponseType(typeof(IEnumerable<PerfilResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BuscarPerfiles([FromQuery] string? q)
    {
        var query = new BuscarPerfilesQuery(q);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al listar los perfiles de cargo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("solicitudes/{solicitudId:int}/perfil")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerPorSolicitudId([FromRoute] int solicitudId)
    {
        var query = new ObtenerPerfilQuery(solicitudId);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontro el perfil de cargo de la solicitud {solicitudId}.",
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

    [Authorize]
    [HttpPut("perfiles/{id:int}/corregir")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Corregir([FromRoute] int id, [FromBody] CorregirPerfilRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new CorregirPerfilCommand(id, request.Cargo, request.Descripcion, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar corregir el perfil de cargo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPut("perfiles/{id:int}/aprobar-rrhh")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AprobarRRHH([FromRoute] int id, [FromBody] AprobarPerfilRRHHRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new AprobarPerfilRRHHCommand(id, request.Comentario, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar aprobar el perfil de cargo por RRHH.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPut("perfiles/{id:int}/enviar-area")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnviarArea([FromRoute] int id, [FromBody] EnviarPerfilAreaRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new EnviarPerfilAreaCommand(id, request.Comentario, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar enviar el perfil de cargo al area.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("perfiles/{id:int}/trazabilidad")]
    [ProducesResponseType(typeof(IEnumerable<StateHistoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTrazabilidad([FromRoute] int id)
    {
        var query = new GetPerfilTrazabilidadQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar obtener la trazabilidad del perfil.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH")]
    [HttpPut("perfiles/{id:int}/secciones/{numeroSeccion:int}")]
    [ProducesResponseType(typeof(PerfilResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActualizarSeccion([FromRoute] int id, [FromRoute] int numeroSeccion, [FromBody] ActualizarSeccionRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new ActualizarPerfilSeccionCommand(id, numeroSeccion, request.Contenido, userEmail, request.Motivo);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar actualizar la sección del perfil.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("perfiles/{id:int}/auditoria")]
    [ProducesResponseType(typeof(IEnumerable<PerfilAuditoriaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditoria([FromRoute] int id)
    {
        var query = new GetPerfilAuditoriaQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al obtener la auditoría del perfil.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("perfiles/{id:int}/pdf/regenerar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegenerarPdf([FromRoute] int id)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new RegenerarPdfCommand(id, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar regenerar el PDF del perfil.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }
}

public class GenerarPerfilRequest
{
    public int SolicitudId { get; set; }
}

public class ObservarPerfilRequest
{
    public string Justificacion { get; set; } = string.Empty;
}

public class PerfilCreacionCallbackRequest : NacionalSeguros.Contracts.Requests.PerfilEstructuradoInputDto
{
}

public class CorregirPerfilRequest
{
    public string Cargo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class AprobarPerfilRRHHRequest
{
    public string? Comentario { get; set; }
}

public class EnviarPerfilAreaRequest
{
    public string? Comentario { get; set; }
}

public class ActualizarSeccionRequest
{
    public string Contenido { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}
