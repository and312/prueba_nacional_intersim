using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Commands.CorregirPerfil;

public class CorregirPerfilCommandHandler : IRequestHandler<CorregirPerfilCommand, Result<PerfilResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilCargoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CorregirPerfilCommandHandler(
        IPerfilCargoRepository perfilCargoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _perfilCargoRepository = perfilCargoRepository ?? throw new ArgumentNullException(nameof(perfilCargoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PerfilResponseDto>> Handle(CorregirPerfilCommand request, CancellationToken cancellationToken)
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

        // Solo permitir corregir si el último estado es EnRevisionRRHH o PerfilCorregidoRRHH o sus equivalentes PERF
        if (latestPerfil.Estado?.Codigo != "EnRevisionRRHH" && 
            latestPerfil.Estado?.Codigo != "PerfilCorregidoRRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-REV-RRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-COR-RRHH" &&
            latestPerfil.Estado?.Codigo != "PERF-RES-GEN")
        {
            return Result.Failure<PerfilResponseDto>(new Error("PerfilCargo.InvalidState", $"No se puede corregir el perfil en su estado actual '{latestPerfil.Estado?.Nombre}'."));
        }

        var estadoCorregido = await _perfilCargoRepository.GetEstadoByCodigoAsync("PERF-COR-RRHH")
            ?? await _perfilCargoRepository.GetEstadoByCodigoAsync("PerfilCorregidoRRHH");
        if (estadoCorregido == null)
        {
            return Result.Failure<PerfilResponseDto>(new Error("Estado.NotFound", "El estado 'PERF-COR-RRHH' o 'PerfilCorregidoRRHH' no está parametrizado."));
        }

        // Registrar comentario para la trazabilidad automática en StateHistory
        _unitOfWork.TransitionComment = "Ajuste de perfil por observación / corrección de RRHH";

        // Actualizar el perfil en su lugar
        latestPerfil.ActualizarContenidoConsolidado(request.Descripcion, request.UserEmail);
        latestPerfil.SetJsonOriginalIA(latestPerfil.JsonOriginalIA ?? latestPerfil.Descripcion);
        latestPerfil.SetJsonActual(request.Descripcion);
        latestPerfil.CambiarEstado(estadoCorregido.Id, request.UserEmail);

        // Poblar o actualizar las secciones
        var parsed = ParseDescriptionJson(request.Descripcion);
        foreach (var kvp in SeccionNombres)
        {
            var content = parsed.TryGetValue(kvp.Key, out var val) ? val : (kvp.Key is 6 or 7 or 8 or 9 or 10 or 11 ? "[]" : (kvp.Key == 12 ? "{}" : ""));
            
            var existingSec = latestPerfil.Secciones.FirstOrDefault(s => s.NumeroSeccion == kvp.Key);
            if (existingSec != null)
            {
                existingSec.ActualizarContenido(content, request.UserEmail);
            }
            else
            {
                var newSec = new PerfilSeccion(latestPerfil.Id, kvp.Key, kvp.Value, content, kvp.Key, request.UserEmail);
                latestPerfil.Secciones.Add(newSec);
            }
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

        // Pre-cargar relaciones para mapear al DTO
        var res = await _perfilCargoRepository.GetByIdAsync(latestPerfil.Id);
        var responseDto = _mapper.Map<PerfilResponseDto>(res ?? latestPerfil);

        return Result.Success(responseDto);
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
