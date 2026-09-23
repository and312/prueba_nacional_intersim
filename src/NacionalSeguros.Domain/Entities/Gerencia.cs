using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Gerencia : Entity<int>
{
    protected Gerencia()
    {
    }

    public Gerencia(string nombre, string? createdBy = null)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Activo = true;
        IsDeleted = false;
        CreatedBy = createdBy ?? "System";
        CreatedDate = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }

    public void ActualizarNombre(string nombre)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
