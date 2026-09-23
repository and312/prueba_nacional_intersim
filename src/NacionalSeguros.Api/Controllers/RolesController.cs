using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Security.Commands.Roles;
using NacionalSeguros.Application.Security.Queries.ObtenerRoles;
using NacionalSeguros.Application.Security.Queries.ObtenerPermisos;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RolResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var query = new ObtenerRolesQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los roles.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("permisos")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPermisos()
    {
        var query = new ObtenerPermisosQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los permisos del sistema.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RolResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] RolCreateUpdateRequest request)
    {
        var command = new CrearRolCommand(request.Nombre, request.Descripcion, request.PermisoCodigos);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo crear el rol.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(Listar), new { }, result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RolResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] RolCreateUpdateRequest request)
    {
        var command = new ActualizarRolCommand(id, request.Nombre, request.Descripcion, request.PermisoCodigos);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo actualizar el rol.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/duplicate")]
    [ProducesResponseType(typeof(RolResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Duplicar([FromRoute] int id, [FromBody] RolDuplicateRequest request)
    {
        var command = new DuplicarRolCommand(id, request.NuevoNombre);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo duplicar el rol.",
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

public class RolCreateUpdateRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<string> PermisoCodigos { get; set; } = new();
}

public class RolDuplicateRequest
{
    public string NuevoNombre { get; set; } = string.Empty;
}
