using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Shared.Primitives;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using System.Collections.Generic;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Application.PerfilesEstructurados.Commands.ActualizarPerfilEstructurado;

public record ActualizarPerfilEstructuradoCommand(
    int? PerfilEstructuradoId,
    int? SolicitudId,
    PerfilEstructuradoInputDto Dto,
    string ModifiedBy
) : IRequest<Result<PerfilEstructuradoResponseDto>>;

public class ActualizarPerfilEstructuradoCommandHandler : IRequestHandler<ActualizarPerfilEstructuradoCommand, Result<PerfilEstructuradoResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilRepository;
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPerfilEstructuradoCommandHandler(
        IPerfilCargoRepository perfilRepository,
        ISolicitudRepository solicitudRepository,
        IUnitOfWork unitOfWork)
    {
        _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PerfilEstructuradoResponseDto>> Handle(ActualizarPerfilEstructuradoCommand request, CancellationToken cancellationToken)
    {
        PerfilEstructurado? pe = null;

        if (request.PerfilEstructuradoId.HasValue && request.PerfilEstructuradoId.Value > 0)
        {
            pe = await _perfilRepository.GetEstructuradoByIdAsync(request.PerfilEstructuradoId.Value, cancellationToken);
        }

        if (pe == null && request.SolicitudId.HasValue && request.SolicitudId.Value > 0)
        {
            pe = await _perfilRepository.GetEstructuradoBySolicitudIdAsync(request.SolicitudId.Value, cancellationToken);
        }

        if (pe == null)
        {
            return Result.Failure<PerfilEstructuradoResponseDto>(new Error("PerfilEstructurado.NotFound", "No se encontró el Perfil Estructurado para actualizar."));
        }

        string datosGeneralesCargoJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.DatosGeneralesCargo);
        string perfilRequeridoJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.PerfilRequerido);
        string herramientasSistemasJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.HerramientasSistemas);
        string filtrosClaveSeleccionJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.FiltrosClaveSeleccion);
        string conocimientosTecnicosRequeridosJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.ConocimientosTecnicosRequeridos);
        string funcionesPrincipalesCargoJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.FuncionesPrincipalesCargo);
        string competenciasClaveJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.CompetenciasClave);
        string indicadoresExitoCargoJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.IndicadoresExitoCargo);
        string matrizPonderacionJson = JsonSerializer.Serialize(request.Dto.PerfilEstructurado.MatrizPonderacion);
        string fuentesUtilizadasJson = JsonSerializer.Serialize(request.Dto.FuentesUtilizadas);
        string alertasJson = JsonSerializer.Serialize(request.Dto.Alertas);

        pe.Actualizar(
            objetivoPrincipalCargo: request.Dto.PerfilEstructurado.ObjetivoPrincipalCargo,
            perfilIdealCandidato: request.Dto.PerfilEstructurado.PerfilIdealCandidato,
            perfilTipoAltoAjuste: request.Dto.PerfilEstructurado.PerfilTipoAltoAjuste,
            estadoGeneracion: request.Dto.EstadoGeneracion,
            datosGeneralesCargo: datosGeneralesCargoJson,
            perfilRequerido: perfilRequeridoJson,
            herramientasSistemas: herramientasSistemasJson,
            filtrosClaveSeleccion: filtrosClaveSeleccionJson,
            conocimientosTecnicosRequeridos: conocimientosTecnicosRequeridosJson,
            funcionesPrincipalesCargo: funcionesPrincipalesCargoJson,
            competenciasClave: competenciasClaveJson,
            indicadoresExitoCargo: indicadoresExitoCargoJson,
            matrizPonderacion: matrizPonderacionJson,
            fuentesUtilizadas: fuentesUtilizadasJson,
            alertas: alertasJson,
            modifiedBy: request.ModifiedBy
        );

        _perfilRepository.UpdateEstructurado(pe);

        var solicitud = await _solicitudRepository.GetByIdAsync(pe.SolicitudId);
        await TransitionPerfilCargoStateAsync(pe.SolicitudId, solicitud?.Cargo ?? string.Empty, request.ModifiedBy, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            pe.ModifiedDate
        );

        return Result.Success(dto);
    }

    private async Task TransitionPerfilCargoStateAsync(int solicitudId, string cargo, string user, CancellationToken cancellationToken)
    {
        var perfilCargo = await _perfilRepository.GetLatestBySolicitudIdAsync(solicitudId);
        var targetEstado = await _perfilRepository.GetEstadoByCodigoAsync("PERF-REV-RRHH");
        var pendingEstado = await _perfilRepository.GetEstadoByCodigoAsync("PERF-PEN-GEN");
        var oldPendingEstado = await _perfilRepository.GetEstadoByCodigoAsync("PendienteGeneracionPerfil");

        if (targetEstado != null)
        {
            if (perfilCargo != null)
            {
                bool isPending = perfilCargo.EstadoId == pendingEstado?.Id || 
                                 (oldPendingEstado != null && perfilCargo.EstadoId == oldPendingEstado.Id);

                if (perfilCargo.EstadoId != targetEstado.Id && isPending)
                {
                    int oldEstadoId = perfilCargo.EstadoId;
                    perfilCargo.CambiarEstado(targetEstado.Id, user);
                    _perfilRepository.Update(perfilCargo);
                    
                    var history = new StateHistory(
                        "Perfil",
                        perfilCargo.Id,
                        oldEstadoId,
                        targetEstado.Id,
                        1, // System admin ID
                        "Actualización de estado por generación de Perfil Estructurado",
                        Guid.NewGuid(),
                        null,
                        "System"
                    );
                    await _perfilRepository.AddStateHistoryAsync(history);
                }
            }
            else
            {
                perfilCargo = new PerfilCargo(
                    solicitudId: solicitudId,
                    cargo: cargo,
                    descripcion: "{}",
                    version: 1,
                    estadoId: targetEstado.Id,
                    createdBy: user
                );
                await _perfilRepository.AddAsync(perfilCargo);
                var solicitudForCodigo = await _solicitudRepository.GetByIdAsync(solicitudId);
                if (solicitudForCodigo != null)
                    perfilCargo.SetCodigo($"PRF-{solicitudForCodigo.Codigo}");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var history = new StateHistory(
                    "Perfil",
                    perfilCargo.Id,
                    0,
                    targetEstado.Id,
                    1,
                    "Creación de perfil y estado inicial por generación de Perfil Estructurado",
                    Guid.NewGuid(),
                    null,
                    "System"
                );
                await _perfilRepository.AddStateHistoryAsync(history);
            }
        }
    }
}
