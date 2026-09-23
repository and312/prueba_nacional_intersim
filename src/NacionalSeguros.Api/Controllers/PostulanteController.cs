using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NacionalSeguros.Application.Postulantes.Commands.RegistrarPostulante;
using NacionalSeguros.Application.Postulantes.Commands.TransitarPostulante;
using NacionalSeguros.Application.Postulantes.Queries.ListarPostulantes;
using NacionalSeguros.Application.Postulantes.Queries.ObtenerExpediente;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1/postulantes")]
public class PostulanteController : ControllerBase
{
    private readonly ISender _sender;

    public PostulanteController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpPost("registro")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(int), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Registrar([FromForm] PostulanteRegistroRequestDto request)
    {
        if (request.CvFile == null || request.CvFile.Length == 0)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = "CvFile.Empty",
                Message = "El archivo del CV es requerido y no puede estar vacío.",
                Detail = "Validación de entrada fallida.",
                CorrelationId = GetCorrelationId()
            });
        }

        using var ms = new MemoryStream();
        await request.CvFile.CopyToAsync(ms);
        byte[] cvBytes = ms.ToArray();

        string userEmail = "candidato.externo@nacionalseguros.com.bo";
        var command = new RegistrarPostulanteCommand(
            request.VacanteId,
            request.Correo,
            cvBytes,
            request.CvFile.FileName,
            userEmail
        );
        
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar registrar la postulación del candidato.",
                CorrelationId = GetCorrelationId()
            });
        }

        Response.Headers.Append("X-Correlation-ID", GetCorrelationId());
        return Accepted(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedPostulantesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? vacanteId = null)
    {
        var query = new ListarPostulantesQuery(pageNumber, pageSize, vacanteId);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al listar los postulantes.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PostulanteExpedienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerExpediente([FromRoute] int id)
    {
        var query = new ObtenerExpedienteQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontró el expediente del postulante con ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador")]
    [HttpPut("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TransitarEstado(
        [FromRoute] int id,
        [FromQuery] int vacanteId,
        [FromBody] PostulanteEstadoRequestDto request)
    {
        string userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system@nacionalseguros.com.bo";
        var command = new TransitarPostulanteCommand(
            id,
            vacanteId,
            request.NuevoEstadoId,
            request.MotivoDescarteCodigo,
            request.JustificacionText,
            userEmail
        );
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar transitar el estado del postulante.",
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

public class PostulanteRegistroRequestDto
{
    public int VacanteId { get; set; }
    public string Correo { get; set; } = string.Empty;
    public IFormFile CvFile { get; set; } = null!;
}
