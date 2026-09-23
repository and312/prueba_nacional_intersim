using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Regional : Entity<int>
{
    private readonly List<Solicitud> _solicitudes = new();

    protected Regional()
    {
    }

    public Regional(string codigo, string nombre, string? descripcion = null)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Descripcion = descripcion;
        Estado = "Activo";
        CreatedBy = "SYSTEM";
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string Estado { get; private set; } = "Activo";

    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<Solicitud> Solicitudes => _solicitudes.AsReadOnly();

    public void Actualizar(string nombre, string? descripcion)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Descripcion = descripcion;
    }

    public void SetEstado(string estado)
    {
        Estado = estado ?? throw new ArgumentNullException(nameof(estado));
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedDate = DateTime.UtcNow;
    }

    public void Reactivar()
    {
        IsDeleted = false;
        Estado = "Activo";
        DeletedBy = null;
        DeletedDate = null;
    }

    public void SetAuditoriaModificacion(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetAuditoriaCreacion(string createdBy)
    {
        CreatedBy = createdBy;
        CreatedDate = DateTime.UtcNow;
    }
}
