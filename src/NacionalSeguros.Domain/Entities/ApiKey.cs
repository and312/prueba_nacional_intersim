using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class ApiKey : Entity<long>
{
    private readonly List<ApiKeyPermiso> _apiKeyPermisos = new();

    protected ApiKey()
    {
    }

    public ApiKey(
        string nombre,
        string apiKeyHash,
        string workflow,
        string? descripcion,
        string estado,
        DateTime? fechaExpiracion,
        string creadoPor,
        string permisos,
        int? integracionId = null,
        string? observaciones = null)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        ApiKeyHash = apiKeyHash ?? throw new ArgumentNullException(nameof(apiKeyHash));
        Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        Descripcion = descripcion ?? string.Empty;
        Estado = estado ?? "Activo";
        FechaCreacion = DateTime.UtcNow;
        FechaExpiracion = fechaExpiracion;
        CreadoPor = creadoPor ?? "System";
        Permisos = permisos ?? string.Empty;
        IntegracionId = integracionId;
        Observaciones = observaciones;
        IsDeleted = false;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string ApiKeyHash { get; private set; } = string.Empty;
    public string Workflow { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "Activo";
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaExpiracion { get; private set; }
    public DateTime? UltimoUso { get; private set; }
    public string? UltimaIP { get; private set; }
    public string CreadoPor { get; private set; } = string.Empty;
    public string Permisos { get; private set; } = string.Empty; // Mantenemos por compatibilidad

    public int? IntegracionId { get; private set; }
    public string? Observaciones { get; private set; }

    // Auditoría
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation
    public Integracion? Integracion { get; private set; }
    public IReadOnlyCollection<ApiKeyPermiso> ApiKeyPermisos => _apiKeyPermisos.AsReadOnly();

    public void RegistrarUso(string ip)
    {
        UltimoUso = DateTime.UtcNow;
        UltimaIP = ip;
    }

    public void CambiarEstado(string nuevoEstado, string modificadoPor)
    {
        Estado = nuevoEstado;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Rotar(string nuevoHash, DateTime? nuevaExpiracion, string modificadoPor)
    {
        ApiKeyHash = nuevoHash ?? throw new ArgumentNullException(nameof(nuevoHash));
        FechaExpiracion = nuevaExpiracion;
        FechaCreacion = DateTime.UtcNow;
        Estado = "Activo";
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Revocar(string modificadoPor)
    {
        Estado = "Revocado";
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Delete(string eliminadoPor)
    {
        IsDeleted = true;
        DeletedBy = eliminadoPor;
        DeletedDate = DateTime.UtcNow;
    }

    public void ActualizarPermisos(IEnumerable<ApiKeyPermiso> permisos)
    {
        _apiKeyPermisos.Clear();
        _apiKeyPermisos.AddRange(permisos);
    }
}
