using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudById;

public class GetSolicitudByIdQueryHandler : IRequestHandler<GetSolicitudByIdQuery, Result<SolicitudResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly ISolicitudResumenRepository _solicitudResumenRepository;
    private readonly ISolicitudDocumentoRepository _solicitudDocumentoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IParametroRepository _parametroRepository;
    private readonly IMapper _mapper;

    public GetSolicitudByIdQueryHandler(
        ISolicitudRepository solicitudRepository,
        ISolicitudResumenRepository solicitudResumenRepository,
        ISolicitudDocumentoRepository solicitudDocumentoRepository,
        IUsuarioRepository usuarioRepository,
        IParametroRepository parametroRepository,
        IMapper mapper)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _solicitudResumenRepository = solicitudResumenRepository ?? throw new ArgumentNullException(nameof(solicitudResumenRepository));
        _solicitudDocumentoRepository = solicitudDocumentoRepository ?? throw new ArgumentNullException(nameof(solicitudDocumentoRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _parametroRepository = parametroRepository ?? throw new ArgumentNullException(nameof(parametroRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<SolicitudResponseDto>> Handle(GetSolicitudByIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByCorreoAsync(request.UserEmail);
        if (usuario == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Usuario.NotFound", $"El usuario '{request.UserEmail}' no existe."));
        }

        var solicitud = await _solicitudRepository.GetByIdAsync(request.Id);
        if (solicitud == null)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.Id} no existe."));
        }

        bool esSolicitante = usuario.Roles.Any(r => r.Nombre.Equals("Solicitante", StringComparison.OrdinalIgnoreCase));
        if (esSolicitante && solicitud.SolicitanteId != usuario.Id)
        {
            return Result.Failure<SolicitudResponseDto>(new Error("Solicitud.Unauthorized", "No está autorizado para ver esta solicitud."));
        }

        var resumen = await _solicitudResumenRepository.GetBySolicitudIdAsync(request.Id);
        var documentos = await _solicitudDocumentoRepository.GetBySolicitudIdAsync(request.Id);
        var pdfDocument = documentos.FirstOrDefault(d => !string.IsNullOrEmpty(d.PublicUrl));

        var responseDto = _mapper.Map<SolicitudResponseDto>(solicitud);
        if (pdfDocument != null)
        {
            responseDto = responseDto with { PdfDocumentUrl = pdfDocument.PublicUrl };
        }
        if (resumen != null)
        {
            responseDto = responseDto with
            {
                CompletitudPorcentaje = resumen.CompletitudPorcentaje,
                CamposDetectados = resumen.CamposDetectados,
                CamposEsperados = resumen.CamposEsperados,
                ProfileSummary = resumen.ProfileSummary,
                CaptureState = resumen.CaptureState
            };
        }
        else
        {
            var expectedParams = await _parametroRepository.GetByCatalogoCodigoAsync("CAT-REQ-FIELDS");
            var expectedParamsList = expectedParams.ToList();
            int totalCampos = expectedParamsList.Count > 0 ? expectedParamsList.Count : 13;
            int camposLlenos = 0;

            if (expectedParamsList.Count > 0)
            {
                foreach (var param in expectedParamsList)
                {
                    bool isFilled = param.Codigo.ToLower() switch
                    {
                        "cargo" => !string.IsNullOrWhiteSpace(solicitud.Cargo),
                        "solicitanteid" => solicitud.SolicitanteId > 0,
                        "regionalid" => solicitud.RegionalId.HasValue,
                        "cantidadvacantes" => solicitud.CantidadVacantes.HasValue && solicitud.CantidadVacantes.Value >= 1,
                        "tiposolicitudid" => solicitud.TipoSolicitudId.HasValue,
                        "motivo" => !string.IsNullOrWhiteSpace(solicitud.Motivo),
                        "modalidadtrabajoid" => solicitud.ModalidadTrabajoId.HasValue,
                        "seniority" => !string.IsNullOrWhiteSpace(solicitud.Seniority),
                        "prioridad" => !string.IsNullOrWhiteSpace(solicitud.Prioridad),
                        "objetivocargo" => !string.IsNullOrWhiteSpace(solicitud.ObjetivoCargo),
                        "experienciaminima" => !string.IsNullOrWhiteSpace(solicitud.ExperienciaMinima),
                        "conocimientostecnicos" => !string.IsNullOrWhiteSpace(solicitud.ConocimientosTecnicos),
                        "funciones" => !string.IsNullOrWhiteSpace(solicitud.Funciones),
                        _ => CheckPropertyHasValue(solicitud, param.Codigo)
                    };

                    if (isFilled) camposLlenos++;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(solicitud.Cargo)) camposLlenos++;
                if (solicitud.SolicitanteId > 0) camposLlenos++;
                if (solicitud.RegionalId.HasValue) camposLlenos++;
                if (solicitud.CantidadVacantes.HasValue && solicitud.CantidadVacantes.Value >= 1) camposLlenos++;
                if (solicitud.TipoSolicitudId.HasValue) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.Motivo)) camposLlenos++;
                if (solicitud.ModalidadTrabajoId.HasValue) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.Seniority)) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.Prioridad)) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.ObjetivoCargo)) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.ExperienciaMinima)) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.ConocimientosTecnicos)) camposLlenos++;
                if (!string.IsNullOrWhiteSpace(solicitud.Funciones)) camposLlenos++;
            }

            decimal pct = totalCampos > 0 ? ((decimal)camposLlenos / totalCampos) * 100 : 0;

            responseDto = responseDto with
            {
                CompletitudPorcentaje = Math.Round(pct, 0),
                CamposDetectados = camposLlenos,
                CamposEsperados = totalCampos,
                ProfileSummary = pct >= 100 
                    ? "La solicitud fue registrada y cuenta con toda la información requerida de forma consistente."
                    : $"La solicitud tiene información incompleta. Se detectaron {camposLlenos} de {totalCampos} campos requeridos.",
                CaptureState = pct >= 100 ? "COMPLETADO" : "INCOMPLETO"
            };
        }

        var comentariosDtoList = solicitud.Comentarios
            .OrderBy(c => c.Fecha)
            .Select(c => {
                string textoLimpio = c.Texto;
                string? tipoComentario = c.TipoComentario;
                string? estadoRelacionado = c.EstadoAsociado;
                int? iteracion = c.Iteracion;

                // Fallback tag parsing for older comments stored in text
                if (c.Texto.StartsWith("[") && c.Texto.Contains("]"))
                {
                    int estadoIdx = c.Texto.IndexOf("[ESTADO:", StringComparison.OrdinalIgnoreCase);
                    if (estadoIdx >= 0)
                    {
                        int start = estadoIdx + 8;
                        int end = c.Texto.IndexOf("]", start);
                        if (end > start)
                        {
                            estadoRelacionado ??= c.Texto.Substring(start, end - start).Trim();
                        }
                    }

                    int iteracionIdx = c.Texto.IndexOf("[ITERACION:", StringComparison.OrdinalIgnoreCase);
                    if (iteracionIdx >= 0)
                    {
                        int start = iteracionIdx + 11;
                        int end = c.Texto.IndexOf("]", start);
                        if (end > start)
                        {
                            if (int.TryParse(c.Texto.Substring(start, end - start), out int iterVal))
                            {
                                iteracion ??= iterVal;
                            }
                        }
                    }

                    int tipoIdx = c.Texto.IndexOf("[TIPO:", StringComparison.OrdinalIgnoreCase);
                    if (tipoIdx >= 0)
                    {
                        int start = tipoIdx + 6;
                        int end = c.Texto.IndexOf("]", start);
                        if (end > start)
                        {
                            tipoComentario ??= c.Texto.Substring(start, end - start).Trim();
                        }
                    }

                    int fallbackTipoIdx = c.Texto.IndexOf("[Tipo:", StringComparison.OrdinalIgnoreCase);
                    if (fallbackTipoIdx >= 0)
                    {
                        int start = fallbackTipoIdx + 6;
                        int end = c.Texto.IndexOf("]", start);
                        if (end > start)
                        {
                            tipoComentario ??= c.Texto.Substring(start, end - start).Trim();
                        }
                    }

                    int lastCloseBracket = c.Texto.LastIndexOf(']');
                    if (lastCloseBracket >= 0 && lastCloseBracket < c.Texto.Length - 1)
                    {
                        textoLimpio = c.Texto.Substring(lastCloseBracket + 1).Trim();
                    }
                }

                if (string.IsNullOrEmpty(tipoComentario))
                {
                    if (c.Texto.Contains("SOL-OBS") || c.Texto.Contains("Observad")) tipoComentario = "OBSERVACION";
                    else if (c.Texto.Contains("SOL-RECH") || c.Texto.Contains("Rechaz")) tipoComentario = "RECHAZO";
                    else if (c.Texto.Contains("SOL-APR") || c.Texto.Contains("Aprob")) tipoComentario = "APROBACION";
                    else tipoComentario = "COMENTARIO";
                }

                if (string.IsNullOrEmpty(estadoRelacionado))
                {
                    if (tipoComentario == "OBSERVACION") estadoRelacionado = "SOL-OBS";
                    else if (tipoComentario == "RECHAZO") estadoRelacionado = "SOL-RECH";
                    else if (tipoComentario == "APROBACION") estadoRelacionado = "SOL-APR";
                }

                return new SolicitudComentarioResponseDto(
                    c.Id,
                    c.SolicitudId,
                    c.Usuario?.Nombre ?? "Usuario",
                    c.Usuario?.Correo ?? string.Empty,
                    textoLimpio,
                    c.Fecha,
                    c.Fecha.ToString("HH:mm"),
                    tipoComentario,
                    estadoRelacionado,
                    iteracion
                );
            })
            .ToList();

        var observacionesList = comentariosDtoList
            .Where(c => c.EstadoRelacionado == "SOL-OBS" || c.TipoComentario == "OBSERVACION")
            .Select(c => {
                string? campoObservado = null;
                var origComment = solicitud.Comentarios.FirstOrDefault(oc => oc.Id == c.ComentarioId);
                if (origComment != null && origComment.Texto.Contains("[Campo:"))
                {
                    int campoIdx = origComment.Texto.IndexOf("[Campo:", StringComparison.OrdinalIgnoreCase);
                    if (campoIdx >= 0)
                    {
                        int start = campoIdx + 7;
                        int end = origComment.Texto.IndexOf("]", start);
                        if (end > start)
                        {
                            campoObservado = origComment.Texto.Substring(start, end - start).Trim();
                        }
                    }
                }

                return new ObservacionResponseDto(
                    c.ComentarioId,
                    c.Fecha,
                    c.Hora,
                    solicitud.SolicitanteId,
                    c.UsuarioNombre,
                    "RRHH",
                    solicitud.Comentarios.FirstOrDefault(oc => oc.Id == c.ComentarioId)?.Texto ?? c.Texto,
                    c.Texto,
                    campoObservado,
                    "Solicitud Observada",
                    c.Fecha,
                    null
                );
            })
            .ToList();

        string? justificacionRechazo = comentariosDtoList
            .LastOrDefault(c => c.EstadoRelacionado == "SOL-RECH" || c.TipoComentario == "RECHAZO")
            ?.Texto;

        var lastObsDto = comentariosDtoList
            .Where(c => c.EstadoRelacionado == "SOL-OBS" || c.TipoComentario == "OBSERVACION")
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.ComentarioId)
            .FirstOrDefault();

        UltimaObservacionRrhhDto? ultimaObs = null;
        if (lastObsDto != null)
        {
            ultimaObs = new UltimaObservacionRrhhDto(
                lastObsDto.Texto,
                lastObsDto.Fecha,
                lastObsDto.UsuarioNombre
            );
        }

        var camposParams = await _parametroRepository.GetByCatalogoCodigoAsync("CAT-REQ-FIELDS");
        var camposRequeridosKeys = camposParams
            .OrderBy(p => p.Orden)
            .Select(p => p.Codigo)
            .ToList()
            .AsReadOnly();

        responseDto = responseDto with { 
            ObservacionesRrhh = observacionesList,
            JustificacionRechazo = justificacionRechazo,
            Comentarios = comentariosDtoList,
            UltimaObservacionRRHH = ultimaObs,
            CamposRequeridosKeys = camposRequeridosKeys
        };

        return Result.Success(responseDto);
    }

    private static bool CheckPropertyHasValue(Solicitud solicitud, string propertyName)
    {
        try
        {
            var prop = typeof(Solicitud).GetProperties()
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            
            if (prop == null) return false;
            
            var val = prop.GetValue(solicitud);
            if (val == null) return false;
            
            if (val is string str) return !string.IsNullOrWhiteSpace(str);
            if (val is int num) return num > 0;
            if (val is decimal dec) return dec > 0;
            if (val is DateTime dt) return dt != default;
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
