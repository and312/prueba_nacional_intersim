using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Rol : Entity<int>
{
    private readonly List<Permiso> _permisos = new();
    private readonly List<Usuario> _usuarios = new();

    protected Rol()
    {
    }

    public Rol(string nombre, string? descripcion)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Descripcion = descripcion;
        CreatedBy = "System";
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<Permiso> Permisos => _permisos.AsReadOnly();
    public IReadOnlyCollection<Usuario> Usuarios => _usuarios.AsReadOnly();

    public void Actualizar(string nombre, string? descripcion)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Descripcion = descripcion;
    }

    public void LimpiarPermisos()
    {
        _permisos.Clear();
    }

    public void AsignarPermiso(Permiso permiso)
    {
        if (permiso == null) throw new ArgumentNullException(nameof(permiso));
        if (!_permisos.Contains(permiso))
        {
            _permisos.Add(permiso);
        }
    }

    public void EliminarPermiso(Permiso permiso)
    {
        if (permiso == null) throw new ArgumentNullException(nameof(permiso));
        _permisos.Remove(permiso);
    }
}
