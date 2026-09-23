using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Matchings.Commands.EjecutarMatching;
using NacionalSeguros.Application.Matchings.Commands.RegistrarScoring;
using NacionalSeguros.Application.Matchings.Queries.ObtenerResultadoMatching;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class MatchingController : ControllerBase
{
    private readonly ISender _sender;

    public MatchingController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpPost("callbacks/scoring")]
    [ProducesResponseType(typeof(ScoringResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CallbackScoring([FromBody] ScoringCallbackRequestDto request)
    {
        // Nota: Este callback es asíncrono e invocado por n8n.
        // Puede estar protegido por ApiKeyAuth en producción. En esta fase se valida a nivel de capa de integración.
        
        var command = new RegistrarScoringCommand(request);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al procesar el callback del scoring del postulante.",
                CorrelationId = GetCorrelationId()
            });
        }

        Response.Headers.Append("X-Correlation-ID", GetCorrelationId());
        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpPost("matchings/evaluar")]
    [ProducesResponseType(typeof(MatchingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluarMatching([FromBody] EvaluarMatchingRequestDto request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new EjecutarMatchingCommand(request.PostulanteId, request.VacanteId, userEmail);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al ejecutar la evaluación de matching del candidato.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpGet("matchings/{postulanteId:int}/{vacanteId:int}")]
    [ProducesResponseType(typeof(MatchingExpedienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerResultado([FromRoute] int postulanteId, [FromRoute] int vacanteId)
    {
        var query = new ObtenerResultadoMatchingQuery(postulanteId, vacanteId);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se encontraron evaluaciones registradas para la relación indicada.",
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
