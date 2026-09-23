using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Solicitudes.Commands.RegistrarDocumento;

public class RegistrarDocumentoCommandHandler : IRequestHandler<RegistrarDocumentoCommand, Result>
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly ISolicitudDocumentoRepository _solicitudDocumentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarDocumentoCommandHandler(
        ISolicitudRepository solicitudRepository,
        ISolicitudDocumentoRepository solicitudDocumentoRepository,
        IUnitOfWork unitOfWork)
    {
        _solicitudRepository = solicitudRepository;
        _solicitudDocumentoRepository = solicitudDocumentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegistrarDocumentoCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(request.SolicitudId);
        if (solicitud == null)
        {
            return Result.Failure(new Error("Solicitud.NotFound", $"La solicitud con ID {request.SolicitudId} no existe."));
        }

        var dto = request.Dto;

        var existingDocs = await _solicitudDocumentoRepository.GetBySolicitudIdAsync(request.SolicitudId, dto.TipoDocumento);
        var existing = existingDocs.FirstOrDefault();

        if (existing != null)
        {
            // Reemplazar físicamente el archivo anterior si es necesario (el servicio de almacenamiento se encarga)
            existing.Actualizar(
                dto.FileName,
                dto.StorageProvider,
                dto.StoragePath,
                dto.PublicUrl,
                dto.GeneradoPor,
                dto.CorrelationId
            );
        }
        else
        {
            var documento = new SolicitudDocumento(
                request.SolicitudId,
                dto.TipoDocumento,
                dto.FileName,
                dto.StorageProvider,
                dto.StoragePath,
                dto.PublicUrl,
                dto.GeneradoPor,
                dto.CorrelationId
            );

            await _solicitudDocumentoRepository.AddAsync(documento);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
