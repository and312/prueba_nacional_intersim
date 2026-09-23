using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.CrearPerfilCargo;

public class CrearPerfilCargoCommandHandler : IRequestHandler<CrearPerfilCargoCommand, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CrearPerfilCargoCommandHandler(
        IPerfilCargoRepository perfilCargoRepository,
        ISolicitudRepository solicitudRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(CrearPerfilCargoCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.Dto.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Dto.SolicitudId} no existe."));
        }

        var latestPerfil = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(request.Dto.SolicitudId);
        var estadoEnRevision = await _perfilCargoRepository.GetEstadoByCodigoAsync("PERF-REV-RRHH");
        if (estadoEnRevision == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("Estado.NotFound", "El estado 'PERF-REV-RRHH' (En revisión RRHH) no está parametrizado."));
        }

        // 1. REALIZAR EL UPSERT DE PERFIL ESTRUCTURADO EN SU TABLA FÍSICA
        var perfilEstructurado = await _perfilCargoRepository.GetEstructuradoBySolicitudIdAsync(request.Dto.SolicitudId, cancellationToken);

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

        if (perfilEstructurado == null)
        {
            perfilEstructurado = new PerfilEstructurado(
                solicitudId: request.Dto.SolicitudId,
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
                createdBy: request.CreatedBy
            );
            await _perfilCargoRepository.AddEstructuradoAsync(perfilEstructurado, cancellationToken);
        }
        else
        {
            perfilEstructurado.Actualizar(
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
                modifiedBy: request.CreatedBy
            );
            _perfilCargoRepository.UpdateEstructurado(perfilEstructurado);
        }

        // 2. MANTENER LA CREACIÓN/ACTUALIZACIÓN DE PERFIL CARGO PARA RETROCOMPATIBILIDAD
        string profesiogramaRawJson = JsonSerializer.Serialize(request.Dto);

        _unitOfWork.TransitionComment = "Perfil generado automáticamente por Agente IA (n8n)";

        var estadoPendiente = await _perfilCargoRepository.GetEstadoByCodigoAsync("PERF-PEN-GEN");
        var estadoPendienteOld = await _perfilCargoRepository.GetEstadoByCodigoAsync("PendienteGeneracionPerfil");

        bool isPending = latestPerfil != null && 
                         (latestPerfil.EstadoId == estadoPendiente?.Id || 
                          (estadoPendienteOld != null && latestPerfil.EstadoId == estadoPendienteOld.Id));

        if (latestPerfil != null)
        {
            latestPerfil.GenerarPerfil(estadoEnRevision.Id, profesiogramaRawJson);
            latestPerfil.SetJsonOriginalIA(profesiogramaRawJson);
            latestPerfil.SetJsonActual(profesiogramaRawJson);

            // Poblar las secciones (actualizar existentes o agregar si no existen)
            var parsed = ParseDescriptionJson(profesiogramaRawJson);
            foreach (var kvp in SeccionNombres)
            {
                var content = parsed.TryGetValue(kvp.Key, out var val) ? val : (kvp.Key is 6 or 7 or 8 or 9 or 10 or 11 ? "[]" : (kvp.Key == 12 ? "{}" : ""));
                var existingSec = latestPerfil.Secciones.FirstOrDefault(s => s.NumeroSeccion == kvp.Key);
                if (existingSec != null)
                {
                    existingSec.ActualizarContenido(content, request.CreatedBy);
                }
                else
                {
                    var sec = new PerfilSeccion(latestPerfil.Id, kvp.Key, kvp.Value, content, kvp.Key, request.CreatedBy);
                    latestPerfil.Secciones.Add(sec);
                }
            }

            _perfilCargoRepository.Update(latestPerfil);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            var res = await _perfilCargoRepository.GetByIdAsync(latestPerfil.Id);
            var responseDto = _mapper.Map<PerfilResponseDto>(res ?? latestPerfil);
            return Result.Success(responseDto);
        }
        else
        {
            var perfilCargo = new PerfilCargo(
                solicitudId: request.Dto.SolicitudId,
                cargo: solicitud.Cargo,
                descripcion: profesiogramaRawJson,
                version: 1,
                estadoId: estadoEnRevision.Id,
                createdBy: request.CreatedBy
            );

            perfilCargo.SetJsonOriginalIA(profesiogramaRawJson);
            perfilCargo.SetJsonActual(profesiogramaRawJson);

            // Poblar las secciones
            var parsed = ParseDescriptionJson(profesiogramaRawJson);
            foreach (var kvp in SeccionNombres)
            {
                var content = parsed.TryGetValue(kvp.Key, out var val) ? val : (kvp.Key is 6 or 7 or 8 or 9 or 10 or 11 ? "[]" : (kvp.Key == 12 ? "{}" : ""));
                var sec = new PerfilSeccion(perfilCargo.Id, kvp.Key, kvp.Value, content, kvp.Key, request.CreatedBy);
                perfilCargo.Secciones.Add(sec);
            }

            await _perfilCargoRepository.AddAsync(perfilCargo);
            perfilCargo.SetCodigo($"PRF-{solicitud.Codigo}");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var res = await _perfilCargoRepository.GetByIdAsync(perfilCargo.Id);
            var responseDto = _mapper.Map<PerfilResponseDto>(res ?? perfilCargo);
            return Result.Success(responseDto);
        }
    }

    private static readonly Dictionary<int, string> SeccionNombres = new()
    {
        { 1, "Objetivo del Cargo" },
        { 2, "Seniority" },
        { 3, "Condiciones Contractuales" },
        { 4, "Formación Académica" },
        { 5, "Experiencia Requerida" },
        { 6, "Idiomas" },
        { 7, "Funciones Principales" },
        { 8, "Responsabilidades Clave" },
        { 9, "Competencias Técnicas" },
        { 10, "Competencias Blandas" },
        { 11, "Certificaciones Requeridas" },
        { 12, "Herramientas, KPIs y Riesgos" }
    };

    private static Dictionary<int, string> ParseDescriptionJson(string json)
    {
        var result = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(json)) return result;

        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node == null) return result;

            result[1] = node["Objetivo"]?.ToString() ?? "";
            result[2] = node["Seniority"]?.ToString() ?? "";
            result[3] = node["Condiciones"]?.ToString() ?? "";
            result[4] = node["Formacion"]?.ToString() ?? "";
            result[5] = node["Experiencia"]?.ToString() ?? "";
            result[6] = node["Idiomas"]?.ToJsonString() ?? "[]";
            result[7] = node["Funciones"]?.ToJsonString() ?? "[]";
            result[8] = node["Responsabilidades"]?.ToJsonString() ?? "[]";
            result[9] = node["CompetenciasTecnicas"]?.ToJsonString() ?? "[]";
            result[10] = node["CompetenciasBlandas"]?.ToJsonString() ?? "[]";
            result[11] = node["Certificaciones"]?.ToJsonString() ?? "[]";

            var sec12 = new System.Text.Json.Nodes.JsonObject
            {
                ["Herramientas"] = (node["Herramientas"] ?? new System.Text.Json.Nodes.JsonArray()).DeepClone(),
                ["Kpis"] = (node["Kpis"] ?? new System.Text.Json.Nodes.JsonArray()).DeepClone(),
                ["Riesgos"] = (node["Riesgos"] ?? new System.Text.Json.Nodes.JsonArray()).DeepClone(),
                ["ObservacionesIA"] = node["ObservacionesIA"]?.ToString() ?? ""
            };
            result[12] = sec12.ToJsonString();
        }
        catch
        {
            // Fallback
        }

        return result;
    }
}
