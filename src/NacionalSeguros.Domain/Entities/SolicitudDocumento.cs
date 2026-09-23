using System;

namespace NacionalSeguros.Domain.Entities;

public class SolicitudDocumento
{
    // Requerido por EF Core
    protected SolicitudDocumento()
    {
    }

    public SolicitudDocumento(
        int solicitudId,
        string tipoDocumento,
        string fileName,
        string storageProvider,
        string storagePath,
        string? publicUrl,
        string? generadoPor,
        Guid? correlationId)
    {
        SolicitudId = solicitudId;
        TipoDocumento = tipoDocumento ?? throw new ArgumentNullException(nameof(tipoDocumento));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        StorageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        PublicUrl = publicUrl;
        GeneradoPor = generadoPor;
        CorrelationId = correlationId;
        CreatedDate = DateTime.UtcNow;
    }

    public int DocumentoId { get; private set; }
    public int SolicitudId { get; private set; }
    public string TipoDocumento { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string StorageProvider { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string? PublicUrl { get; private set; }
    public string? GeneradoPor { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public DateTime CreatedDate { get; private set; }

    // Propiedad de navegación
    public Solicitud Solicitud { get; private set; } = null!;

    public void Actualizar(
        string fileName,
        string storageProvider,
        string storagePath,
        string? publicUrl,
        string? generadoPor,
        Guid? correlationId)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        StorageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        PublicUrl = publicUrl;
        GeneradoPor = generadoPor;
        CorrelationId = correlationId;
    }
}
