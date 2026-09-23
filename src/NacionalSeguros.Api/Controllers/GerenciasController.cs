using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Security.Queries.Gerencias;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/gerencias")]
public class GerenciasController : ControllerBase
{
    private readonly ISender _sender;

    public GerenciasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GerenciaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var query = new ObtenerGerenciasQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener las gerencias.",
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
