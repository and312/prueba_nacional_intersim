using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.ActualizarPerfilSeccion;

public class ActualizarPerfilSeccionCommandHandler : IRequestHandler<ActualizarPerfilSeccionCommand, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

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

    public ActualizarPerfilSeccionCommandHandler(
        IPerfilCargoRepository perfilCargoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(ActualizarPerfilSeccionCommand request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilCargoRepository.GetByIdAsync(request.PerfilCargoId);
        if (perfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"El perfil de cargo con ID {request.PerfilCargoId} no existe."));
        }

        var latestPerfil = await _perfilCargoRepository.GetLatestBySolicitudIdAsync(perfil.SolicitudId);
        if (latestPerfil == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.NotFound", $"No se encontró la versión más reciente del perfil para la solicitud {perfil.SolicitudId}."));
        }

        if (latestPerfil.Id != perfil.Id)
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.ObsoleteVersion", "Estás intentando modificar una versión obsoleta del perfil."));
        }

        // RRHH puede editar únicamente en PERF-REV-RRHH, PERF-COR-RRHH, PERF-RES-GEN, PERF-OBS-AREA y sus equivalentes de Set 2
        if (latestPerfil.Estado?.Codigo != "PERF-REV-RRHH" && 
            latestPerfil.Estado?.Codigo != "PERF-COR-RRHH" &&
            latestPerfil.Estado?.Codigo != "EnRevisionRRHH" &&
            latestPerfil.Estado?.Codigo != "PerfilCorregidoRRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-RES-GEN" &&
            latestPerfil.Estado?.Codigo != "PERF-OBS-AREA" &&
            latestPerfil.Estado?.Codigo != "ObservadoAreaSol" &&
            latestPerfil.Estado?.Codigo != "ObservadoSolicitante")
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.InvalidState", $"No se puede modificar el perfil en su estado actual '{latestPerfil.Estado?.Nombre}'."));
        }

        if (!SeccionNombres.ContainsKey(request.NumeroSeccion))
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilSeccion.InvalidSection", $"El número de sección {request.NumeroSeccion} no es válido."));
        }

        // Cargar secciones actuales o crearlas desde el JSON original si no existen
        var currentSections = latestPerfil.Secciones.ToList();
        if (!currentSections.Any())
        {
            var parsed = ParseDescriptionJson(latestPerfil.Descripcion);
            foreach (var kvp in SeccionNombres)
            {
                var content = parsed.TryGetValue(kvp.Key, out var val) ? val : (kvp.Key is 6 or 7 or 8 or 9 or 10 or 11 ? "[]" : (kvp.Key == 12 ? "{}" : ""));
                var sec = new PerfilSeccion(latestPerfil.Id, kvp.Key, kvp.Value, content, kvp.Key, latestPerfil.CreatedBy);
                latestPerfil.Secciones.Add(sec);
                currentSections.Add(sec);
            }
        }

        string? valorAnterior = null;
        string seccionNombre = SeccionNombres[request.NumeroSeccion];

        // Modificar la sección correspondiente
        var targetSection = latestPerfil.Secciones.FirstOrDefault(s => s.NumeroSeccion == request.NumeroSeccion);
        if (targetSection != null)
        {
            valorAnterior = targetSection.Contenido;
            targetSection.ActualizarContenido(request.Contenido, request.Usuario);
        }
        else
        {
            var nuevaSec = new PerfilSeccion(
                perfilCargoId: latestPerfil.Id,
                numeroSeccion: request.NumeroSeccion,
                nombreSeccion: seccionNombre,
                contenido: request.Contenido,
                orden: request.NumeroSeccion,
                usuarioActualizacion: request.Usuario
            );
            latestPerfil.Secciones.Add(nuevaSec);
            targetSection = nuevaSec;
        }

        // Consolidar todas las secciones en un único JSON
        var consolidatedJson = RebuildConsolidatedJson(latestPerfil.Secciones, latestPerfil.Cargo);
        latestPerfil.ActualizarContenidoConsolidado(consolidatedJson, request.Usuario);

        // Sincronizar con la tabla física PerfilEstructurado
        var estructuradoEntity = await _perfilCargoRepository.GetEstructuradoBySolicitudIdAsync(perfil.SolicitudId, cancellationToken);
        if (estructuradoEntity != null)
        {
            string objetivoPrincipalCargo = estructuradoEntity.ObjetivoPrincipalCargo;
            string perfilIdealCandidato = estructuradoEntity.PerfilIdealCandidato;
            string perfilTipoAltoAjuste = estructuradoEntity.PerfilTipoAltoAjuste;
            string datosGeneralesCargo = estructuradoEntity.DatosGeneralesCargo;
            string perfilRequerido = estructuradoEntity.PerfilRequerido;
            string herramientasSistemas = estructuradoEntity.HerramientasSistemas;
            string filtrosClaveSeleccion = estructuradoEntity.FiltrosClaveSeleccion;
            string conocimientosTecnicosRequeridos = estructuradoEntity.ConocimientosTecnicosRequeridos;
            string funcionesPrincipalesCargo = estructuradoEntity.FuncionesPrincipalesCargo;
            string competenciasClave = estructuradoEntity.CompetenciasClave;
            string indicadoresExitoCargo = estructuradoEntity.IndicadoresExitoCargo;
            string matrizPonderacion = estructuradoEntity.MatrizPonderacion;

            if (request.NumeroSeccion == 1)
            {
                objetivoPrincipalCargo = request.Contenido;
            }
            else if (request.NumeroSeccion == 3)
            {
                string reportaA = "";
                string banda = "";
                var lines = request.Contenido.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("Reporta a:", StringComparison.OrdinalIgnoreCase))
                        reportaA = line.Substring("Reporta a:".Length).Trim();
                    else if (line.StartsWith("Banda Salarial/Presupuesto:", StringComparison.OrdinalIgnoreCase))
                        banda = line.Substring("Banda Salarial/Presupuesto:".Length).Trim();
                }

                string? salario = null;
                if (!string.IsNullOrEmpty(banda))
                {
                    salario = banda.Trim();
                }
                latestPerfil.ActualizarSalario(salario);

                var dgNode = System.Text.Json.Nodes.JsonNode.Parse(estructuradoEntity.DatosGeneralesCargo) ?? new System.Text.Json.Nodes.JsonObject();
                dgNode["reportaA"] = reportaA;
                if (!string.IsNullOrEmpty(salario))
                {
                    dgNode["salario"] = salario;
                    dgNode["bandaSalarial"] = salario;
                }
                datosGeneralesCargo = dgNode.ToJsonString();
            }
            else if (request.NumeroSeccion == 7)
            {
                funcionesPrincipalesCargo = request.Contenido;
            }
            else if (request.NumeroSeccion == 10)
            {
                try
                {
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(request.Contenido);
                    perfilIdealCandidato = list?.FirstOrDefault() ?? "";
                }
                catch
                {
                    perfilIdealCandidato = request.Contenido;
                }
                competenciasClave = request.Contenido;
            }
            else if (request.NumeroSeccion == 12)
            {
                try
                {
                    var s12Node = System.Text.Json.Nodes.JsonNode.Parse(request.Contenido);
                    herramientasSistemas = s12Node?["Herramientas"]?.ToJsonString() ?? "[]";
                    indicadoresExitoCargo = s12Node?["Kpis"]?.ToJsonString() ?? "[]";
                }
                catch
                {
                    // No-op
                }
            }
            else if (request.NumeroSeccion == 8)
            {
                matrizPonderacion = request.Contenido;
            }
            else if (request.NumeroSeccion == 11)
            {
                try
                {
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(request.Contenido);
                    perfilTipoAltoAjuste = list?.FirstOrDefault() ?? "";
                }
                catch
                {
                    perfilTipoAltoAjuste = request.Contenido;
                }
            }

            estructuradoEntity.Actualizar(
                objetivoPrincipalCargo,
                perfilIdealCandidato,
                perfilTipoAltoAjuste,
                estructuradoEntity.EstadoGeneracion,
                datosGeneralesCargo,
                perfilRequerido,
                herramientasSistemas,
                filtrosClaveSeleccion,
                conocimientosTecnicosRequeridos,
                funcionesPrincipalesCargo,
                competenciasClave,
                indicadoresExitoCargo,
                matrizPonderacion,
                estructuradoEntity.FuentesUtilizadas,
                estructuradoEntity.Alertas,
                request.Usuario
            );

            _perfilCargoRepository.UpdateEstructurado(estructuradoEntity);
        }

        // Asignar el código si no existe
        string nuevoCodigo = latestPerfil.Codigo;
        if (string.IsNullOrEmpty(nuevoCodigo))
        {
            var solicitud = latestPerfil.Solicitud;
            if (solicitud != null && !string.IsNullOrEmpty(solicitud.Codigo))
            {
                nuevoCodigo = $"PRF-{solicitud.Codigo}";
            }
            else
            {
                nuevoCodigo = "PRF-SOL-UNKNOWN";
            }
            latestPerfil.SetCodigo(nuevoCodigo);
        }

        _perfilCargoRepository.Update(latestPerfil);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Registrar auditoría con Version = 1
        var auditoria = new PerfilAuditoria(
            perfilCargoId: latestPerfil.Id,
            perfilSeccionId: targetSection?.Id,
            version: 1,
            seccionModificada: seccionNombre,
            valorAnterior: valorAnterior,
            valorNuevo: request.Contenido,
            usuario: request.Usuario,
            motivoCambio: request.Motivo ?? $"Modificación de la sección {seccionNombre}",
            estadoPerfil: latestPerfil.Estado?.Nombre ?? "En Revisión RRHH"
        );

        await _perfilCargoRepository.AddAuditoriaAsync(auditoria);
        
        // Registrar comentario en unit of work
        _unitOfWork.TransitionComment = $"Edición sección: {seccionNombre}. v1";
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Devolver la respuesta
        var res = await _perfilCargoRepository.GetByIdAsync(latestPerfil.Id);
        var responseDto = _mapper.Map<PerfilResponseDto>(res ?? latestPerfil);

        return Result.Success(responseDto);
    }

    private static Dictionary<int, string> ParseDescriptionJson(string json)
    {
        var result = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(json)) return result;

        try
        {
            var node = JsonNode.Parse(json);
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

            var sec12 = new JsonObject
            {
                ["Herramientas"] = (node["Herramientas"] ?? new JsonArray()).DeepClone(),
                ["Kpis"] = (node["Kpis"] ?? new JsonArray()).DeepClone(),
                ["Riesgos"] = (node["Riesgos"] ?? new JsonArray()).DeepClone(),
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

    private static string RebuildConsolidatedJson(ICollection<PerfilSeccion> secciones, string cargoNombre)
    {
        var root = new JsonObject();
        root["Cargo"] = cargoNombre;

        foreach (var sec in secciones)
        {
            try
            {
                switch (sec.NumeroSeccion)
                {
                    case 1: root["Objetivo"] = sec.Contenido; break;
                    case 2: root["Seniority"] = sec.Contenido; break;
                    case 3: root["Condiciones"] = sec.Contenido; break;
                    case 4: root["Formacion"] = sec.Contenido; break;
                    case 5: root["Experiencia"] = sec.Contenido; break;
                    case 6: root["Idiomas"] = JsonNode.Parse(sec.Contenido); break;
                    case 7: root["Funciones"] = JsonNode.Parse(sec.Contenido); break;
                    case 8: root["Responsabilidades"] = JsonNode.Parse(sec.Contenido); break;
                    case 9: root["CompetenciasTecnicas"] = JsonNode.Parse(sec.Contenido); break;
                    case 10: root["CompetenciasBlandas"] = JsonNode.Parse(sec.Contenido); break;
                    case 11: root["Certificaciones"] = JsonNode.Parse(sec.Contenido); break;
                    case 12:
                        var s12 = JsonNode.Parse(sec.Contenido);
                        if (s12 != null)
                        {
                            root["Herramientas"] = s12["Herramientas"]?.DeepClone();
                            root["Kpis"] = s12["Kpis"]?.DeepClone();
                            root["Riesgos"] = s12["Riesgos"]?.DeepClone();
                            root["ObservacionesIA"] = s12["ObservacionesIA"]?.ToString();
                        }
                        break;
                }
            }
            catch
            {
                // Fallback en caso de error
                var key = sec.NumeroSeccion switch
                {
                    1 => "Objetivo", 2 => "Seniority", 3 => "Condiciones", 4 => "Formacion", 5 => "Experiencia",
                    6 => "Idiomas", 7 => "Funciones", 8 => "Responsabilidades", 9 => "CompetenciasTecnicas",
                    10 => "CompetenciasBlandas", 11 => "Certificaciones", _ => "Herramientas"
                };
                if (sec.NumeroSeccion is 6 or 7 or 8 or 9 or 10 or 11)
                {
                    root[key] = new JsonArray();
                }
                else
                {
                    root[key] = sec.Contenido;
                }
            }
        }

        return root.ToJsonString();
    }
}
