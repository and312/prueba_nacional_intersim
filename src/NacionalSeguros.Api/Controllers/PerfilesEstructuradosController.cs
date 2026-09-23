using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.PerfilesEstructurados.Commands.CrearPerfilEstructurado;
using NacionalSeguros.Application.PerfilesEstructurados.Commands.ActualizarPerfilEstructurado;
using NacionalSeguros.Application.PerfilesEstructurados.Queries;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1/perfiles-estructurados")]
[Authorize]
public class PerfilesEstructuradosController : ControllerBase
{
    private readonly ISender _sender;

    public PerfilesEstructuradosController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ??
               User.Identity?.Name ??
               "backoffice@nacionalseguros.com.bo";
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(typeof(PerfilEstructuradoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] PerfilEstructuradoInputDto request)
    {
        var command = new CrearPerfilEstructuradoCommand(request, GetCurrentUserEmail());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al crear el perfil estructurado."
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value.PerfilEstructuradoId }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PerfilEstructuradoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var query = new ListarPerfilesEstructuradosQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PerfilEstructuradoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var query = new ObtenerPerfilEstructuradoPorIdQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("solicitud/{solicitudId:int}")]
    [ProducesResponseType(typeof(PerfilEstructuradoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorSolicitudId([FromRoute] int solicitudId)
    {
        var query = new ObtenerPerfilEstructuradoPorSolicitudIdQuery(solicitudId);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,RRHH")]
    [ProducesResponseType(typeof(PerfilEstructuradoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] PerfilEstructuradoInputDto request)
    {
        var command = new ActualizarPerfilEstructuradoCommand(id, id, request, GetCurrentUserEmail());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al actualizar el perfil estructurado."
            });
        }

        return Ok(result.Value);
    }
}
