using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Perfiles.Queries;

public record ListarPerfilesQuery(int? SolicitanteId) : IRequest<Result<List<PerfilListResponseDto>>>;

public record ObtenerPerfilDetalleQuery(int PerfilId, int? SolicitanteId) : IRequest<Result<PerfilDetailResponseDto>>;

public class PerfilQueriesHandler 
    : IRequestHandler<ListarPerfilesQuery, Result<List<PerfilListResponseDto>>>,
      IRequestHandler<ObtenerPerfilDetalleQuery, Result<PerfilDetailResponseDto>>
{
    private readonly IPerfilCargoRepository _perfilRepository;
    private readonly ISolicitudDocumentoRepository _documentoRepository;

    public PerfilQueriesHandler(
        IPerfilCargoRepository perfilRepository, 
        ISolicitudDocumentoRepository documentoRepository)
    {
        _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));
        _documentoRepository = documentoRepository ?? throw new ArgumentNullException(nameof(documentoRepository));
    }

    public async Task<Result<List<PerfilListResponseDto>>> Handle(ListarPerfilesQuery request, CancellationToken cancellationToken)
    {
        var perfiles = request.SolicitanteId.HasValue
            ? await _perfilRepository.ListBySolicitanteIdAsync(request.SolicitanteId.Value)
            : await _perfilRepository.ListAsync();

        var list = perfiles.Select(p => {
            string estadoCodigo = p.Estado?.Codigo ?? "DESCONOCIDO";
            string accion = estadoCodigo switch
            {
                "PERF-PEN-GEN" => "Generar Resumen (Automatización)",
                "PERF-REV-RRHH" => "Revisar Borrador (RRHH)",
                "PERF-RES-GEN" => "Cargar PDFs (Automatización)",
                "PERF-REV-AREA" => "Revisar Perfil (Área Solicitante)",
                "PERF-OBS-AREA" => "Corregir Perfil (RRHH)",
                "PERF-COR-RRHH" => "Enviar a Área (RRHH)",
                "PERF-APR-AREA" => "Aprobación Final (RRHH)",
                "PERF-APR-FIN" => "Ninguna - Aprobado",
                _ => "Revisión General"
            };

            string codigoSolicitud = p.Solicitud?.Codigo ?? string.Empty;
            string codigoPerfil = string.IsNullOrEmpty(codigoSolicitud) ? $"PRF-{p.Id}" : $"PRF-{codigoSolicitud}";
            string canal = p.Solicitud?.CanalOrigen ?? "Web";
            string prioridad = p.Solicitud?.Prioridad ?? "Media";
            DateTime fechaSolicitud = (p.Solicitud?.CreatedDate != null && p.Solicitud.CreatedDate.Year > 2000)
                ? p.Solicitud.CreatedDate
                : (p.CreatedDate.Year > 2000 ? p.CreatedDate : (p.ModifiedDate ?? DateTime.UtcNow));
            DateTime ultimaActualizacion = (p.ModifiedDate != null && p.ModifiedDate.Value.Year > 2000)
                ? p.ModifiedDate.Value
                : (p.CreatedDate.Year > 2000 ? p.CreatedDate : fechaSolicitud);

            return new PerfilListResponseDto(
                p.Id,
                codigoPerfil,
                codigoSolicitud,
                p.Solicitud?.Cargo ?? "Sin Cargo",
                p.Solicitud?.Solicitante?.Area?.Nombre ?? "Sin Área",
                p.Estado?.Nombre ?? "Sin Estado",
                estadoCodigo,
                ultimaActualizacion,
                accion,
                p.Solicitud?.Solicitante?.Nombre ?? "Sin Asignar",
                p.Solicitud?.SolicitanteId ?? 0,
                canal,
                prioridad,
                fechaSolicitud
            );
        }).ToList();

        return Result.Success(list);
    }

    public async Task<Result<PerfilDetailResponseDto>> Handle(ObtenerPerfilDetalleQuery request, CancellationToken cancellationToken)
    {
        var perfil = await _perfilRepository.GetByIdWithDetailsAsync(request.PerfilId);

        if (perfil == null)
        {
            return Result.Failure<PerfilDetailResponseDto>(new Error("Perfil.NotFound", $"No se encontró el perfil con ID {request.PerfilId}."));
        }

        // Si el perfil solicitado no está activo, redirigir automáticamente a la última versión activa
        if (!perfil.Activo)
        {
            var activePerfil = await _perfilRepository.GetLatestBySolicitudIdAsync(perfil.SolicitudId);
            if (activePerfil != null)
            {
                perfil = activePerfil;
            }
        }

        // Validar propiedad del perfil si es Solicitante
        if (request.SolicitanteId.HasValue && perfil.Solicitud.SolicitanteId != request.SolicitanteId.Value)
        {
            return Result.Failure<PerfilDetailResponseDto>(new Error("Perfil.Forbidden", "No tiene permisos para visualizar este perfil."));
        }

        // Obtener observaciones
        var observacionesList = await _perfilRepository.GetObservacionesByPerfilIdAsync(perfil.Id);
        var observaciones = observacionesList.Select(o => new PerfilObservacionResponseDto(
            o.Id,
            o.PerfilCargoId,
            o.TipoObservacionId,
            o.TipoObservacion?.Nombre ?? "Desconocido",
            o.Comentario,
            o.UsuarioSolicitanteId,
            o.UsuarioSolicitante?.Nombre ?? "Desconocido",
            o.NumeroIteracion,
            o.EstadoObservacion,
            o.CreatedDate,
            o.AtendidaPorUsuarioId,
            o.AtendidaPorUsuario?.Nombre,
            o.FechaAtencion
        )).ToList();

        // Obtener Resumen Ejecutivo desde tabla física dbo.ResumenEjecutivos
        var resumenEntity = await _perfilRepository.GetResumenByPerfilCargoIdAsync(perfil.Id);
        ResumenEjecutivoDto? resumenDto = null;

        if (resumenEntity != null)
        {
            var funciones = DeserializeList(resumenEntity.FuncionesPrincipales);
            var requisitos = DeserializeList(resumenEntity.RequisitosMinimos);
            var hardSkills = DeserializeList(resumenEntity.HardSkills);
            var softSkills = DeserializeList(resumenEntity.SoftSkills);

            // Extract bandaSalarial from section 3 if exists
            string finalBandaSalarial = resumenEntity.BandaSalarial;
            var sec3 = perfil.Secciones.FirstOrDefault(sec => sec.NumeroSeccion == 3);
            if (sec3 != null && !string.IsNullOrEmpty(sec3.Contenido))
            {
                var lines = sec3.Contenido.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("Banda Salarial/Presupuesto:", StringComparison.OrdinalIgnoreCase))
                    {
                        var val = line.Substring("Banda Salarial/Presupuesto:".Length).Trim();
                        if (!string.IsNullOrEmpty(val))
                        {
                            finalBandaSalarial = val;
                        }
                    }
                }
            }

            resumenDto = new ResumenEjecutivoDto(
                resumenEntity.Resumen,
                resumenEntity.ObjetivoCargo,
                funciones,
                requisitos,
                resumenEntity.FormacionExperiencia,
                hardSkills,
                softSkills,
                resumenEntity.Modalidad,
                resumenEntity.Ubicacion,
                finalBandaSalarial,
                resumenEntity.CriteriosEvaluacion,
                resumenEntity.CaracteristicasClave,
                resumenEntity.ValoracionPerfil
            );
        }

        // Obtener Perfil Estructurado (desde tabla física dbo.PerfilEstructurado si existe, de lo contrario fallback dinámico)
        object perfilEstructurado;
        var estructuradoEntity = await _perfilRepository.GetEstructuradoBySolicitudIdAsync(perfil.SolicitudId, cancellationToken);

        if (estructuradoEntity != null)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            perfilEstructurado = new PerfilEstructuradoPersistidoDto(
                estructuradoEntity.Id,
                estructuradoEntity.SolicitudId,
                estructuradoEntity.ObjetivoPrincipalCargo,
                estructuradoEntity.PerfilIdealCandidato,
                estructuradoEntity.PerfilTipoAltoAjuste,
                estructuradoEntity.EstadoGeneracion,
                JsonSerializer.Deserialize<object>(estructuradoEntity.DatosGeneralesCargo, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.PerfilRequerido, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.HerramientasSistemas, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.FiltrosClaveSeleccion, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.ConocimientosTecnicosRequeridos, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.FuncionesPrincipalesCargo, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.CompetenciasClave, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.IndicadoresExitoCargo, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.MatrizPonderacion, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.FuentesUtilizadas, options) ?? new object(),
                JsonSerializer.Deserialize<object>(estructuradoEntity.Alertas, options) ?? new object()
            );
        }
        else
        {
            var s = perfil.Solicitud;

            // Fallback parsing from JsonActual or Descripcion or Secciones to extract edited sections
            string reportaA = s.Solicitante?.Cargo ?? "Sin Asignar";
            string funciones = s.Funciones ?? string.Empty;
            string herramientas = s.HerramientasSistemas ?? string.Empty;

            var sec3 = perfil.Secciones.FirstOrDefault(sec => sec.NumeroSeccion == 3);
            if (sec3 != null && !string.IsNullOrEmpty(sec3.Contenido))
            {
                var lines = sec3.Contenido.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("Reporta a:", StringComparison.OrdinalIgnoreCase))
                    {
                        reportaA = line.Substring("Reporta a:".Length).Trim();
                    }
                }
            }

            var sec7 = perfil.Secciones.FirstOrDefault(sec => sec.NumeroSeccion == 7);
            if (sec7 != null && !string.IsNullOrEmpty(sec7.Contenido))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(sec7.Contenido);
                    if (list != null) funciones = string.Join("\n", list);
                }
                catch
                {
                    funciones = sec7.Contenido;
                }
            }

            var sec12 = perfil.Secciones.FirstOrDefault(sec => sec.NumeroSeccion == 12);
            if (sec12 != null && !string.IsNullOrEmpty(sec12.Contenido))
            {
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(sec12.Contenido);
                    if (dict != null)
                    {
                        if (dict.TryGetValue("Herramientas", out var hVal) && hVal != null)
                        {
                            var hList = JsonSerializer.Deserialize<List<string>>(hVal.ToString()!);
                            if (hList != null) herramientas = string.Join("\n", hList);
                        }
                    }
                }
                catch
                {
                    // Fallback
                }
            }

            var datosGenerales = new DatosGeneralesDto(
                s.CreatedDate,
                s.Solicitante?.Area?.Nombre ?? "Sin Área",
                s.Solicitante?.Nombre ?? "Sin Asignar",
                reportaA,
                s.Cargo,
                s.Regional?.Nombre ?? "Sin Sede",
                s.CantidadVacantes ?? 1,
                s.TipoSolicitud?.Nombre ?? "Sin Tipo",
                s.Motivo ?? "Sin Motivo"
            );

            var perfilRequerido = new PerfilRequeridoDto(
                s.ObjetivoCargo ?? string.Empty,
                s.FormacionAcademica ?? string.Empty,
                s.ExperienciaMinima ?? string.Empty,
                s.ExperienciaIndispensable ?? string.Empty,
                s.ConocimientosTecnicos ?? string.Empty,
                herramientas,
                s.CompetenciasClave ?? string.Empty,
                s.CriteriosExcluyentes ?? string.Empty,
                s.CriteriosDeseables ?? string.Empty,
                funciones
            );

            var condicionesVacante = new CondicionesVacanteDto(
                s.ModalidadTrabajo?.Nombre ?? "Sin Modalidad",
                s.DisponibilidadRequerida ?? string.Empty,
                s.Seniority ?? string.Empty,
                s.Prioridad ?? string.Empty
            );

            perfilEstructurado = new PerfilEstructuradoDto(
                datosGenerales,
                perfilRequerido,
                condicionesVacante
            );
        }

        string accion = perfil.Estado.Codigo switch
        {
            "PERF-PEN-GEN" => "Generar Resumen (Automatización)",
            "PERF-REV-RRHH" => "Revisar Borrador (RRHH)",
            "PERF-RES-GEN" => "Cargar PDFs (Automatización)",
            "PERF-REV-AREA" => "Revisar Perfil (Área Solicitante)",
            "PERF-OBS-AREA" => "Corregir Perfil (RRHH)",
            "PERF-COR-RRHH" => "Enviar a Área (RRHH)",
            "PERF-APR-AREA" => "Aprobación Final (RRHH)",
            "PERF-APR-FIN" => "Ninguna - Aprobado",
            _ => "Revisión General"
        };

        bool aprobadoPorArea = await _perfilRepository.WasApprovedByAreaAsync(perfil.Id);

        var docsEntities = await _documentoRepository.GetBySolicitudIdAsync(perfil.SolicitudId);
        var documentos = docsEntities.Select(d => new PerfilDocumentoResponseDto(
            d.DocumentoId,
            d.SolicitudId,
            d.TipoDocumento,
            d.FileName,
            d.StorageProvider,
            d.StoragePath,
            d.PublicUrl,
            d.GeneradoPor,
            d.CreatedDate
        )).ToList();

        var response = new PerfilDetailResponseDto(
            perfil.Id,
            $"PRF-{perfil.Solicitud.Codigo}",
            perfil.Solicitud.Codigo ?? string.Empty,
            perfil.SolicitudId,
            perfil.Solicitud.Cargo,
            perfil.Solicitud.Solicitante?.Area?.Nombre ?? "Sin Área",
            perfil.Estado.Nombre,
            perfil.Estado.Codigo,
            perfil.ModifiedDate ?? perfil.CreatedDate,
            accion,
            perfil.Solicitud.Solicitante?.Nombre ?? "Sin Asignar",
            perfil.Solicitud.SolicitanteId,
            resumenDto,
            perfilEstructurado,
            observaciones,
            perfil.Version,
            aprobadoPorArea,
            perfil.Salario,
            documentos
        );

        return Result.Success(response);
    }

    private List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
