using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.ListSolicitudesKanban;

public class ListSolicitudesKanbanQueryHandler : IRequestHandler<ListSolicitudesKanbanQuery, Result<PagedSolicitudesResponseDto>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public ListSolicitudesKanbanQueryHandler(
        ISolicitudRepository solicitudRepository,
        IUsuarioRepository usuarioRepository,
        IMapper mapper)
    {
        _solicitudRepository = solicitudRepository ?? throw new ArgumentNullException(nameof(solicitudRepository));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<PagedSolicitudesResponseDto>> Handle(ListSolicitudesKanbanQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var usuario = await _usuarioRepository.GetByCorreoAsync(request.UserEmail);
        int? solicitanteIdFilter = null;
        bool isRrhh = false;

        if (request.SolicitanteIdExplicit.HasValue)
        {
            solicitanteIdFilter = request.SolicitanteIdExplicit.Value;
        }
        else if (usuario != null)
        {
            bool isAdmin = usuario.Roles.Any(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));
            if (request.SoloMisSolicitudes)
            {
                solicitanteIdFilter = usuario.Id;
            }
            else if (!isAdmin)
            {
                if (usuario.Roles.Any(r => r.Nombre.Equals("Solicitante", StringComparison.OrdinalIgnoreCase)))
                {
                    solicitanteIdFilter = usuario.Id;
                }

                if (usuario.Roles.Any(r => r.Nombre.Equals("RRHH", StringComparison.OrdinalIgnoreCase)) ||
                    (usuario.Area != null && usuario.Area.Nombre.Equals("Recursos Humanos", StringComparison.OrdinalIgnoreCase)))
                {
                    isRrhh = true;
                }
            }
        }
        Console.WriteLine($"[DEBUG-SIR] request.SoloMisSolicitudes={request.SoloMisSolicitudes}, request.UserEmail={request.UserEmail}, usuario={usuario?.Nombre} (ID: {usuario?.Id}), solicitanteIdFilter={solicitanteIdFilter}");

        var (items, totalCount) = await _solicitudRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            request.EstadoId,
            request.Search,
            solicitanteIdFilter,
            excludePending: request.SoloMisSolicitudes ? false : isRrhh);

        var mappedItems = _mapper.Map<List<SolicitudResponseDto>>(items);

        for (int i = 0; i < mappedItems.Count; i++)
        {
            var item = mappedItems[i];
            var origSolicitud = items.FirstOrDefault(s => s.Id == item.SolicitudId);
            if (origSolicitud != null && origSolicitud.Comentarios != null)
            {
                var lastObsComment = origSolicitud.Comentarios
                    .Where(c => c.EstadoAsociado == "SOL-OBS" || c.TipoComentario == "OBSERVACION" ||
                                (c.Texto.StartsWith("[") && (c.Texto.Contains("SOL-OBS") || c.Texto.Contains("Observad"))))
                    .OrderByDescending(c => c.Fecha)
                    .ThenByDescending(c => c.Id)
                    .FirstOrDefault();

                if (lastObsComment != null)
                {
                    string textoLimpio = lastObsComment.Texto;
                    if (lastObsComment.Texto.StartsWith("[") && lastObsComment.Texto.Contains("]"))
                    {
                        int lastCloseBracket = lastObsComment.Texto.LastIndexOf(']');
                        if (lastCloseBracket >= 0 && lastCloseBracket < lastObsComment.Texto.Length - 1)
                        {
                            textoLimpio = lastObsComment.Texto.Substring(lastCloseBracket + 1).Trim();
                        }
                    }

                    mappedItems[i] = item with
                    {
                        UltimaObservacionRRHH = new UltimaObservacionRrhhDto(
                            textoLimpio,
                            lastObsComment.Fecha,
                            lastObsComment.Usuario?.Nombre ?? "Usuario RRHH"
                        )
                    };
                }
            }
        }

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new PagedSolicitudesResponseDto
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            TotalPages = totalPages
        };

        return Result.Success(response);
    }
}
