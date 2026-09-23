using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetSolicitudStateHistory;

public class GetSolicitudStateHistoryQueryHandler : IRequestHandler<GetSolicitudStateHistoryQuery, Result<List<StateHistoryResponseDto>>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IMapper _mapper;

    public GetSolicitudStateHistoryQueryHandler(ISolicitudRepository solicitudRepository, IMapper mapper)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<List<StateHistoryResponseDto>>> Handle(GetSolicitudStateHistoryQuery request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure<List<StateHistoryResponseDto>>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.SolicitudId} no existe."));
        }

        var history = await _solicitudRepository.GetStateHistoryAsync(request.SolicitudId);
        var comments = solicitud.Comentarios;

        var mergedList = new List<StateHistoryResponseDto>();

        foreach (var sh in history)
        {
            string comentario = sh.Comentario ?? string.Empty;
            string? tipoComentario = null;
            string? estadoRelacionado = null;
            int? iteracion = null;

            if (comentario.StartsWith("[") && comentario.Contains("]"))
            {
                int estadoIdx = comentario.IndexOf("[ESTADO:", StringComparison.OrdinalIgnoreCase);
                if (estadoIdx >= 0)
                {
                    int start = estadoIdx + 8;
                    int end = comentario.IndexOf("]", start);
                    if (end > start)
                    {
                        estadoRelacionado = comentario.Substring(start, end - start).Trim();
                    }
                }

                int iteracionIdx = comentario.IndexOf("[ITERACION:", StringComparison.OrdinalIgnoreCase);
                if (iteracionIdx >= 0)
                {
                    int start = iteracionIdx + 11;
                    int end = comentario.IndexOf("]", start);
                    if (end > start)
                    {
                        if (int.TryParse(comentario.Substring(start, end - start), out int iterVal))
                        {
                            iteracion = iterVal;
                        }
                    }
                }

                int tipoIdx = comentario.IndexOf("[TIPO:", StringComparison.OrdinalIgnoreCase);
                if (tipoIdx >= 0)
                {
                    int start = tipoIdx + 6;
                    int end = comentario.IndexOf("]", start);
                    if (end > start)
                    {
                        tipoComentario = comentario.Substring(start, end - start).Trim();
                    }
                }

                int lastCloseBracket = comentario.LastIndexOf(']');
                if (lastCloseBracket >= 0 && lastCloseBracket < comentario.Length - 1)
                {
                    comentario = comentario.Substring(lastCloseBracket + 1).Trim();
                }
            }

            if (string.IsNullOrEmpty(tipoComentario))
            {
                if (sh.EstadoNuevo != null && sh.EstadoNuevo.Codigo == "SOL-OBS") tipoComentario = "OBSERVACION";
                else if (sh.EstadoNuevo != null && sh.EstadoNuevo.Codigo == "SOL-RECH") tipoComentario = "RECHAZO";
                else if (sh.EstadoNuevo != null && sh.EstadoNuevo.Codigo == "SOL-APR") tipoComentario = "APROBACION";
            }

            if (sh.EstadoAnterior?.Codigo == "SOL-OBS" || sh.EstadoNuevo?.Codigo == "SOL-COR" || (sh.EstadoAnterior?.Codigo == "SOL-COR" && sh.EstadoNuevo?.Codigo == "SOL-BOR"))
            {
                var comentarioRespuesta = comments.FirstOrDefault(c => (c.Iteracion == sh.Iteracion || c.Iteracion == (sh.Iteracion ?? 1)) && c.TipoComentario == "RESPUESTA_SOLICITANTE");
                if (comentarioRespuesta != null && !string.IsNullOrWhiteSpace(comentarioRespuesta.Texto))
                {
                    comentario = comentarioRespuesta.Texto;
                }
            }

            mergedList.Add(new StateHistoryResponseDto(
                sh.Id,
                sh.EntidadId,
                sh.EstadoAnterior?.Nombre ?? string.Empty,
                sh.EstadoNuevo?.Nombre ?? string.Empty,
                sh.Usuario?.Nombre ?? "Sistema",
                sh.Fecha,
                comentario,
                sh.Rol ?? (sh.Usuario != null && sh.Usuario.Roles.Any() ? string.Join(", ", sh.Usuario.Roles.Select(r => r.Nombre)) : "Sistema"),
                tipoComentario,
                estadoRelacionado ?? sh.EstadoNuevo?.Codigo,
                iteracion ?? sh.Iteracion,
                sh.CorrelationId
            ));
        }

        foreach (var c in comments)
        {
            bool isDuplicate = System.Linq.Enumerable.Any(history, sh => 
                Math.Abs((sh.Fecha - c.Fecha).TotalSeconds) < 10 || 
                sh.Comentario == c.Texto
            );

            if (!isDuplicate)
            {
                string comentarioText = c.Texto;
                string? tipoComentario = c.TipoComentario;
                string? estadoRelacionado = c.EstadoAsociado;
                int? iteracion = c.Iteracion;

                if (c.Texto.StartsWith("[") && c.Texto.Contains("]"))
                {
                    int estadoIdx = c.Texto.IndexOf("[ESTADO:", StringComparison.OrdinalIgnoreCase);
                    if (estadoIdx >= 0)
                    {
                        int start = estadoIdx + 8;
                        int end = c.Texto.IndexOf("]", start);
                        if (end > start)
                        {
                            estadoRelacionado = c.Texto.Substring(start, end - start).Trim();
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
                                iteracion = iterVal;
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
                            tipoComentario = c.Texto.Substring(start, end - start).Trim();
                        }
                    }

                    int lastCloseBracket = c.Texto.LastIndexOf(']');
                    if (lastCloseBracket >= 0 && lastCloseBracket < c.Texto.Length - 1)
                    {
                        comentarioText = c.Texto.Substring(lastCloseBracket + 1).Trim();
                    }
                }

                string estadoNuevoNombre = "";
                if (estadoRelacionado == "SOL-OBS") estadoNuevoNombre = "Solicitud Observada";
                else if (estadoRelacionado == "SOL-RECH") estadoNuevoNombre = "Solicitud Rechazada";
                else if (estadoRelacionado == "SOL-APR") estadoNuevoNombre = "Aprobada";

                mergedList.Add(new StateHistoryResponseDto(
                    c.Id,
                    c.SolicitudId,
                    "",
                    estadoNuevoNombre,
                    c.Usuario?.Nombre ?? "Usuario",
                    c.Fecha,
                    comentarioText,
                    c.Usuario != null && c.Usuario.Roles.Any() ? string.Join(", ", c.Usuario.Roles.Select(r => r.Nombre)) : "Usuario",
                    tipoComentario ?? "Comentario",
                    estadoRelacionado,
                    iteracion,
                    null
                ));
            }
        }

        var ordered = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(mergedList, x => x.FechaCambio));
        return Result.Success(ordered);
    }
}
