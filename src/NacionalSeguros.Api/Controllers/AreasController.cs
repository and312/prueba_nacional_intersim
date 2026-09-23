using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Security.Commands.Areas;
using NacionalSeguros.Application.Security.Queries.Areas;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/areas")]
public class AreasController : ControllerBase
{
    private readonly ISender _sender;

    public AreasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<AreaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var query = new ObtenerAreasQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener las áreas.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AreaResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] AreaCreateRequest request)
    {
        var command = new CrearAreaCommand(request.Codigo, request.Nombre, request.GerenciaId, request.Responsable);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo crear el área.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(Listar), new { }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AreaResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] AreaUpdateRequest request)
    {
        var command = new ActualizarAreaCommand(id, request.Nombre, request.GerenciaId, request.Responsable, request.Estado);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo actualizar el área.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var command = new EliminarAreaCommand(id);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo eliminar el área. Verifique si tiene usuarios asociados.",
                CorrelationId = GetCorrelationId()
            });
        }

        return NoContent();
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

public class AreaCreateRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int GerenciaId { get; set; }
    public string? Responsable { get; set; }
}

public class AreaUpdateRequest
{
    public string Nombre { get; set; } = string.Empty;
    public int GerenciaId { get; set; }
    public string? Responsable { get; set; }
    public string Estado { get; set; } = "Activo";
}
