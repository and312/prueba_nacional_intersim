using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Application.Abstractions.Audit;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.GuardarResumen;

public class GuardarResumenCommandHandler : IRequestHandler<GuardarResumenCommand, Result>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly ISolicitudResumenRepository _solicitudResumenRepository;
    private readonly IParametroRepository _parametroRepository;
    private readonly IMatchingRepository _matchingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public GuardarResumenCommandHandler(
        ISolicitudRepository solicitudRepository,
        ISolicitudResumenRepository solicitudResumenRepository,
        IParametroRepository parametroRepository,
        IMatchingRepository matchingRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _solicitudRepository = solicitudRepository;
        _solicitudResumenRepository = solicitudResumenRepository;
        _parametroRepository = parametroRepository;
        _matchingRepository = matchingRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(GuardarResumenCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        var dto = request.Dto;

        // 1. Obtener parámetros de campos requeridos para calcular completitud
        int camposEsperados;
        int camposDetectados;
        decimal completitudPorcentaje;

        var missingFieldsList = dto.MissingFields ?? new List<string>();

        if (dto.CamposEsperados.HasValue && dto.CamposDetectados.HasValue)
        {
            camposEsperados = dto.CamposEsperados.Value;
            camposDetectados = dto.CamposDetectados.Value;
            completitudPorcentaje = dto.CompletitudPorcentaje ?? (camposEsperados > 0 ? ((decimal)camposDetectados / camposEsperados) * 100 : 0);
        }
        else
        {
            // fallback backward-compatible
            var expectedParams = await _parametroRepository.GetByCatalogoCodigoAsync("CAT-REQ-FIELDS");
            var expectedParamsList = expectedParams.ToList();
            camposEsperados = expectedParamsList.Count > 0 ? expectedParamsList.Count : 13;
            camposDetectados = Math.Max(0, camposEsperados - missingFieldsList.Count);
            completitudPorcentaje = camposEsperados > 0 ? ((decimal)camposDetectados / camposEsperados) * 100 : 0;
        }

        // Validaciones de consistencia
        if (camposEsperados < 0 || camposDetectados < 0 || camposDetectados > camposEsperados)
        {
            await _auditService.LogActionAsync(
                usuarioId: null,
                usuarioNombre: "system@nacionalseguros.com.bo",
                rol: "Sistema",
                modulo: "Solicitudes",
                entidad: "SolicitudResumen",
                entidadId: request.Id,
                accion: "GuardarResumen.ValidationError",
                estadoAnterior: null,
                estadoNuevo: $"Valores inconsistentes. CamposEsperados: {camposEsperados}, CamposDetectados: {camposDetectados}",
                canal: "N8N",
                correlationId: dto.CorrelationId
            );

            return Result.Failure(new Error("ValidationError", $"La cantidad de campos detectados ({camposDetectados}) o esperados ({camposEsperados}) es inválida o inconsistente."));
        }

        // Serializar listas a JSON
        string? camposFaltantesJson = missingFieldsList.Any() ? JsonSerializer.Serialize(missingFieldsList) : null;
        string? inconsistenciasJson = dto.Inconsistencias != null && dto.Inconsistencias.Any() ? JsonSerializer.Serialize(dto.Inconsistencias) : null;

        // 2. Guardar o actualizar en SolicitudResumenes
        var existingResumen = await _solicitudResumenRepository.GetBySolicitudIdAsync(request.Id);

        Guid correlationId = dto.CorrelationId ?? Guid.NewGuid();

        if (existingResumen != null)
        {
            existingResumen.Actualizar(
                dto.ProfileSummary,
                completitudPorcentaje,
                camposDetectados,
                camposEsperados,
                dto.CaptureState,
                dto.CaptureConfidence,
                dto.Recommendation,
                camposFaltantesJson,
                inconsistenciasJson,
                dto.AgentName,
                dto.AgentVersion,
                correlationId
            );
            _solicitudResumenRepository.Update(existingResumen);
        }
        else
        {
            var newResumen = new SolicitudResumen(
                request.Id,
                dto.ProfileSummary,
                completitudPorcentaje,
                camposDetectados,
                camposEsperados,
                dto.CaptureState,
                dto.CaptureConfidence,
                dto.Recommendation,
                camposFaltantesJson,
                inconsistenciasJson,
                dto.AgentName,
                dto.AgentVersion,
                correlationId
            );
            await _solicitudResumenRepository.AddAsync(newResumen);
        }

        // 3. Transitar el estado de la solicitud
        string nuevoEstadoCodigo = "SOL-OBS";
        string comentarioTransicion = "Validación del Agente de Solicitudes";

        if (string.Equals(dto.Recommendation, "APROBAR_REVISION", StringComparison.OrdinalIgnoreCase))
        {
            nuevoEstadoCodigo = solicitud.Estado?.Codigo == "SOL-REG" ? "SOL-PEN" : "SOL-ENV";
            comentarioTransicion = solicitud.Estado?.Codigo == "SOL-REG"
                ? "Aprobado por el Agente de Solicitudes. Listo para ser enviado a revisión."
                : "Aprobado por el Agente de Solicitudes. En revisión por RRHH.";
        }
        else
        {
            comentarioTransicion = $"Observado por el Agente de Solicitudes. Motivo: {dto.ProfileSummary ?? "Datos faltantes o inconsistencias detectadas."}";
        }

        if (solicitud.Estado?.Codigo != nuevoEstadoCodigo)
        {
            var nuevoEstado = await _solicitudRepository.GetEstadoByCodigoAsync(nuevoEstadoCodigo);
            if (nuevoEstado == null)
            {
                return Result.Failure(new Error("Estado.NotFound", $"El estado con código '{nuevoEstadoCodigo}' no existe en el catálogo."));
            }

            try
            {
                solicitud.Transitar(nuevoEstado, null, "system@nacionalseguros.com.bo");
                _unitOfWork.TransitionComment = comentarioTransicion;
                _solicitudRepository.Update(solicitud);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("INVALID_STATE_TRANSITION", ex.Message));
            }
        }

        // 4. Registrar ejecución del agente (AgenteId = 1 es AgenteSolicitud)
        var execution = new AgentExecution(
            agenteId: 1,
            promptVersionId: 1,
            usuarioId: 1, // System
            fechaInicio: DateTime.UtcNow.AddMilliseconds(-250),
            fechaFin: DateTime.UtcNow,
            duracionMs: 250,
            inputJson: JsonSerializer.Serialize(dto),
            outputJson: JsonSerializer.Serialize(new { completitud = completitudPorcentaje, nuevoEstado = nuevoEstadoCodigo, comentario = comentarioTransicion }),
            resultadoStatus: "Success",
            tokensInput: 0,
            tokensOutput: 0,
            costoEstimado: 0.0m,
            correlationId: correlationId
        );

        await _matchingRepository.AddAgentExecutionAsync(execution);

        // Guardar todos los cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
