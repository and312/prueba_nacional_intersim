using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Requests;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Queries.GetDocumentos;

public class GetDocumentosQueryHandler : IRequestHandler<GetDocumentosQuery, Result<List<SolicitudDocumentoDto>>>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly ISolicitudDocumentoRepository _solicitudDocumentoRepository;

    public GetDocumentosQueryHandler(
        ISolicitudRepository solicitudRepository,
        ISolicitudDocumentoRepository solicitudDocumentoRepository)
    {
        _solicitudRepository = solicitudRepository;
        _solicitudDocumentoRepository = solicitudDocumentoRepository;
    }

    public async Task<Result<List<SolicitudDocumentoDto>>> Handle(GetDocumentosQuery request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure<List<SolicitudDocumentoDto>>(new Error("Solicitud.NotFound", $"La solicitud con ID {request.SolicitudId} no existe."));
        }

        var documents = await _solicitudDocumentoRepository.GetBySolicitudIdAsync(request.SolicitudId, request.TipoDocumento);

        var dtos = documents.Select(sd => new SolicitudDocumentoDto(
            sd.TipoDocumento,
            sd.FileName,
            sd.StorageProvider,
            sd.StoragePath,
            sd.PublicUrl,
            sd.GeneradoPor,
            sd.CorrelationId
        )).ToList();

        return Result.Success(dtos);
    }
}
