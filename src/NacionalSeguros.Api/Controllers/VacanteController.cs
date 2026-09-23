using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Vacantes.Commands.CancelarVacante;
using NacionalSeguros.Application.Vacantes.Commands.CerrarVacante;
using NacionalSeguros.Application.Vacantes.Commands.CrearVacante;
using NacionalSeguros.Application.Vacantes.Commands.PublicarVacante;
using NacionalSeguros.Application.Vacantes.Commands.ActualizarVacante;
using NacionalSeguros.Application.Vacantes.Commands.PausarVacante;
using NacionalSeguros.Application.Vacantes.Commands.ReanudarVacante;
using NacionalSeguros.Application.Vacantes.Queries.BuscarVacantes;
using NacionalSeguros.Application.Vacantes.Queries.ObtenerVacantePorId;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1/vacantes")]
public class VacanteController : ControllerBase
{
    private readonly ISender _sender;

    public VacanteController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    [Authorize(Roles = "Administrador,RRHH")]
    [HttpPost]
    [ProducesResponseType(typeof(VacanteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Crear([FromBody] VacanteCreateDto request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new CrearVacanteCommand(request, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar registrar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value.VacanteId }, result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador,Decisor")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedVacantesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new BuscarVacantesQuery(pageNumber, pageSize);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al listar las vacantes.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador,Decisor")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VacanteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var query = new ObtenerVacantePorIdQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontro la vacante con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpPost("{id:int}/publicar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Publicar([FromRoute] int id, [FromBody] PublicarVacanteRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new PublicarVacanteCommand(id, request.CanalIds, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar publicar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [Authorize(Roles = "Administrador,RRHH")]
    [HttpPost("{id:int}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cerrar([FromRoute] int id, [FromBody] CerrarVacanteRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new CerrarVacanteCommand(id, request.PostulanteContratadoId, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar cerrar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [Authorize(Roles = "Administrador,RRHH,Decisor")]
    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancelar([FromRoute] int id, [FromBody] CancelarVacanteRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new CancelarVacanteCommand(id, request.MotivoCancelacionCodigo, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar cancelar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [Authorize(Roles = "Administrador,RRHH")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] ActualizarVacanteRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new ActualizarVacanteCommand(id, request.BandaSalarialMin, request.BandaSalarialMax, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar actualizar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpPost("{id:int}/pausar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Pausar([FromRoute] int id, [FromBody] PausarVacanteRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new PausarVacanteCommand(id, request.Justificacion, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar pausar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpPost("{id:int}/reanudar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reanudar([FromRoute] int id, [FromBody] ReanudarVacanteRequest request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new ReanudarVacanteCommand(id, request.Justificacion, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar reanudar la vacante.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
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

public class PublicarVacanteRequest
{
    public List<int> CanalIds { get; set; } = new();
}

public class CerrarVacanteRequest
{
    public int PostulanteContratadoId { get; set; }
}

public class CancelarVacanteRequest
{
    public string MotivoCancelacionCodigo { get; set; } = string.Empty;
}

public class ActualizarVacanteRequest
{
    public decimal BandaSalarialMin { get; set; }
    public decimal BandaSalarialMax { get; set; }
}

public class PausarVacanteRequest
{
    public string Justificacion { get; set; } = string.Empty;
}

public class ReanudarVacanteRequest
{
    public string Justificacion { get; set; } = string.Empty;
}
