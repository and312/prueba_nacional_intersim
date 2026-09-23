using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Perfiles.Commands;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1/tipos-observacion")]
[Authorize]
public class TiposObservacionController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITipoObservacionRepository _tipoObservacionRepository;

    public TiposObservacionController(ISender sender, ITipoObservacionRepository tipoObservacionRepository)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _tipoObservacionRepository = tipoObservacionRepository ?? throw new ArgumentNullException(nameof(tipoObservacionRepository));
    }

    private string GetUserEmail()
    {
        return User.Identity?.Name ??
               User.FindFirst(ClaimTypes.Email)?.Value ??
               User.FindFirst("email")?.Value ??
               "SYSTEM";
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TipoObservacionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool? soloActivos)
    {
        var items = await _tipoObservacionRepository.GetAllAsync();
        if (soloActivos == true)
        {
            items = items.Where(t => t.Estado == "Activo");
        }
        var response = items.Select(t => new TipoObservacionResponseDto(
            t.Id,
            t.Codigo,
            t.Nombre,
            t.Descripcion,
            t.Estado
        ));
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] TipoObservacionCreateRequest request)
    {
        var command = new CrearTipoObservacionCommand(request.Codigo, request.Nombre, request.Descripcion, GetUserEmail());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] TipoObservacionUpdateRequest request)
    {
        var command = new ActualizarTipoObservacionCommand(id, request.Nombre, request.Descripcion, GetUserEmail());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPatch("{id:int}/activar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activar([FromRoute] int id)
    {
        var command = new ActivarTipoObservacionCommand(id, GetUserEmail());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }

    [HttpPatch("{id:int}/inactivar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inactivar([FromRoute] int id)
    {
        var command = new InactivarTipoObservacionCommand(id, GetUserEmail());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto { Code = result.Error.Code, Message = result.Error.Message });
        }

        return Ok();
    }
}
