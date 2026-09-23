using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text.Json;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Application.PerfilesEstructurados.Queries;

public record ListarPerfilesEstructuradosQuery() : IRequest<Result<List<PerfilEstructuradoResponseDto>>>;

public record ObtenerPerfilEstructuradoPorIdQuery(int Id) : IRequest<Result<PerfilEstructuradoResponseDto>>;

public record ObtenerPerfilEstructuradoPorSolicitudIdQuery(int SolicitudId) : IRequest<Result<PerfilEstructuradoResponseDto>>;

public class PerfilEstructuradoQueriesHandler
    : IRequestHandler<ListarPerfilesEstructuradosQuery, Result<List<PerfilEstructuradoResponseDto>>>,
      IRequestHandler<ObtenerPerfilEstructuradoPorIdQuery, Result<PerfilEstructuradoResponseDto>>,
      IRequestHandler<ObtenerPerfilEstructuradoPorSolicitudIdQuery, Result<PerfilEstructuradoResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilRepository;

    public PerfilEstructuradoQueriesHandler(IPerfilCargoRepository perfilRepository)
    {
        _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));
    }

    public async Task<Result<List<PerfilEstructuradoResponseDto>>> Handle(ListarPerfilesEstructuradosQuery request, CancellationToken cancellationToken)
    {
        var list = await _perfilRepository.ListEstructuradosAsync(cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        var dtos = list.Select(pe => new PerfilEstructuradoResponseDto(
            pe.Id,
            pe.SolicitudId,
            pe.ObjetivoPrincipalCargo,
            pe.PerfilIdealCandidato,
            pe.PerfilTipoAltoAjuste,
            pe.EstadoGeneracion,
            JsonSerializer.Deserialize<object>(pe.DatosGeneralesCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.PerfilRequerido, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.HerramientasSistemas, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FiltrosClaveSeleccion, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.ConocimientosTecnicosRequeridos, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FuncionesPrincipalesCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.CompetenciasClave, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.IndicadoresExitoCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.MatrizPonderacion, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FuentesUtilizadas, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.Alertas, options) ?? new object(),
            pe.CreatedBy,
            pe.CreatedDate,
            pe.ModifiedBy,
            pe.ModifiedDate
        )).ToList();

        return Result.Success(dtos);
    }

    public async Task<Result<PerfilEstructuradoResponseDto>> Handle(ObtenerPerfilEstructuradoPorIdQuery request, CancellationToken cancellationToken)
    {
        var pe = await _perfilRepository.GetEstructuradoByIdAsync(request.Id, cancellationToken);
        if (pe == null)
        {
            return Result.Failure<PerfilEstructuradoResponseDto>(new Error("PerfilEstructurado.NotFound", $"No se encontró el Perfil Estructurado con ID {request.Id}."));
        }

        var pc = await _perfilRepository.GetLatestBySolicitudIdAsync(pe.SolicitudId);
        int? version = pc?.Version;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dto = new PerfilEstructuradoResponseDto(
            pe.Id,
            pe.SolicitudId,
            pe.ObjetivoPrincipalCargo,
            pe.PerfilIdealCandidato,
            pe.PerfilTipoAltoAjuste,
            pe.EstadoGeneracion,
            JsonSerializer.Deserialize<object>(pe.DatosGeneralesCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.PerfilRequerido, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.HerramientasSistemas, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FiltrosClaveSeleccion, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.ConocimientosTecnicosRequeridos, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FuncionesPrincipalesCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.CompetenciasClave, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.IndicadoresExitoCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.MatrizPonderacion, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FuentesUtilizadas, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.Alertas, options) ?? new object(),
            pe.CreatedBy,
            pe.CreatedDate,
            pe.ModifiedBy,
            pe.ModifiedDate,
            version
        );

        return Result.Success(dto);
    }

    public async Task<Result<PerfilEstructuradoResponseDto>> Handle(ObtenerPerfilEstructuradoPorSolicitudIdQuery request, CancellationToken cancellationToken)
    {
        var pe = await _perfilRepository.GetEstructuradoBySolicitudIdAsync(request.SolicitudId, cancellationToken);
        if (pe == null)
        {
            return Result.Failure<PerfilEstructuradoResponseDto>(new Error("PerfilEstructurado.NotFound", $"No se encontró el Perfil Estructurado para la solicitud con ID {request.SolicitudId}."));
        }

        var pc = await _perfilRepository.GetLatestBySolicitudIdAsync(request.SolicitudId);
        int? version = pc?.Version;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dto = new PerfilEstructuradoResponseDto(
            pe.Id,
            pe.SolicitudId,
            pe.ObjetivoPrincipalCargo,
            pe.PerfilIdealCandidato,
            pe.PerfilTipoAltoAjuste,
            pe.EstadoGeneracion,
            JsonSerializer.Deserialize<object>(pe.DatosGeneralesCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.PerfilRequerido, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.HerramientasSistemas, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FiltrosClaveSeleccion, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.ConocimientosTecnicosRequeridos, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FuncionesPrincipalesCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.CompetenciasClave, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.IndicadoresExitoCargo, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.MatrizPonderacion, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.FuentesUtilizadas, options) ?? new object(),
            JsonSerializer.Deserialize<object>(pe.Alertas, options) ?? new object(),
            pe.CreatedBy,
            pe.CreatedDate,
            pe.ModifiedBy,
            pe.ModifiedDate,
            version
        );

        return Result.Success(dto);
    }
}
