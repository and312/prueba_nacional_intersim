using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Security.Commands.Usuarios;
using NacionalSeguros.Application.Security.Queries.Usuarios;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")] // Acceso restringido por política (RBAC Matrix)
[Route("api/v1/usuarios")]
public class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedUsuariosResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? areaId = null,
        [FromQuery] int? rolId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? cargo = null,
        [FromQuery] string? gerencia = null)
    {
        var query = new ListarUsuariosQuery(pageNumber, pageSize, search, areaId, rolId, estado, cargo, gerencia);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron listar los usuarios.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] UsuarioCreateDto request)
    {
        var command = new CrearUsuarioCommand(
            request.Correo,
            request.Nombres,
            request.Apellidos,
            request.TipoAutenticacion,
            request.Clave,
            request.AreaId,
            request.Cargo,
            request.Gerencia,
            request.Telefono,
            request.Extension,
            request.Observaciones,
            request.FotografiaUrl,
            request.RolIds);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar crear el usuario.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value.UsuarioId }, result.Value);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var query = new ObtenerUsuarioPorIdQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontró un usuario con el ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] UsuarioUpdateDto request)
    {
        var command = new ActualizarUsuarioCommand(
            id,
            request.Correo,
            request.Nombres,
            request.Apellidos,
            request.AreaId,
            request.Cargo,
            request.Gerencia,
            request.Telefono,
            request.Extension,
            request.Observaciones,
            request.FotografiaUrl,
            request.RolIds,
            request.Estado);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar actualizar los datos del usuario.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado([FromRoute] int id, [FromBody] CambiarEstadoRequest request)
    {
        var command = new CambiarEstadoUsuarioCommand(id, request.Estado);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo cambiar el estado del usuario.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [HttpPost("{id:int}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestablecerPassword([FromRoute] int id, [FromBody] ResetPasswordRequest request)
    {
        var command = new RestablecerPasswordCommand(id, request.NuevaClave);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudo restablecer la contraseña del usuario.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var command = new EliminarUsuarioCommand(id);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se pudo inactivar el usuario con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return NoContent();
    }

    [HttpGet("{id:int}/roles")]
    [ProducesResponseType(typeof(List<RolResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerRoles([FromRoute] int id)
    {
        var query = new ObtenerUsuarioPorIdQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los roles del usuario.",
                CorrelationId = GetCorrelationId()
            });
        }

        // Mapear los roles a DTO de respuesta
        var rolesDto = result.Value.Roles.Select(r => new RolResponseDto
        {
            Nombre = r
        }).ToList();

        return Ok(rolesDto);
    }

    [HttpPut("{id:int}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AsignarRoles([FromRoute] int id, [FromBody] RolesAssignmentRequest request)
    {
        var command = new AsignarRolesUsuarioCommand(id, request.RolIds);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron reasignar los roles del usuario.",
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

public class RolesAssignmentRequest
{
    public List<int> RolIds { get; set; } = new();
}

public class CambiarEstadoRequest
{
    public string Estado { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string NuevaClave { get; set; } = string.Empty;
}
