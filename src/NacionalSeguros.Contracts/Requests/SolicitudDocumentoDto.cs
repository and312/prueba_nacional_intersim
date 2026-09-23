using System;

namespace NacionalSeguros.Contracts.Requests;

public record SolicitudDocumentoDto(
    string TipoDocumento,
    string FileName,
    string StorageProvider,
    string StoragePath,
    string? PublicUrl,
    string? GeneradoPor,
    Guid? CorrelationId
);
