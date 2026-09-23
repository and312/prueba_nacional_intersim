using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Integracion : Entity<int>
{
    private readonly List<ApiKey> _apiKeys = new();

    protected Integracion()
    {
    }

    public Integracion(
        string nombre,
        string codigo,
        string? descripcion,
        string tipo,
        string responsable,
        string correoResponsable,
        string creadoPor)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Descripcion = descripcion;
        Tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
        Responsable = responsable ?? throw new ArgumentNullException(nameof(responsable));
        CorreoResponsable = correoResponsable ?? throw new ArgumentNullException(nameof(correoResponsable));
        Estado = "Activo";
        CreatedBy = creadoPor ?? "System";
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Responsable { get; private set; } = string.Empty;
    public string CorreoResponsable { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "Activo";
    public string? Observaciones { get; private set; }

    // Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();

    public void Actualizar(
        string nombre,
        string? descripcion,
        string tipo,
        string responsable,
        string correoResponsable,
        string? observaciones,
        string modificadoPor)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Descripcion = descripcion;
        Tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
        Responsable = responsable ?? throw new ArgumentNullException(nameof(responsable));
        CorreoResponsable = correoResponsable ?? throw new ArgumentNullException(nameof(correoResponsable));
        Observaciones = observaciones;
        
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Inactivar(string modificadoPor)
    {
        Estado = "Inactivo";
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Activar(string modificadoPor)
    {
        Estado = "Activo";
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Delete(string eliminadoPor)
    {
        IsDeleted = true;
        DeletedBy = eliminadoPor;
        DeletedDate = DateTime.UtcNow;
    }
}
