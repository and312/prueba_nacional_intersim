using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Permiso : Entity<int>
{
    private readonly List<Rol> _roles = new();

    protected Permiso()
    {
    }

    public Permiso(string codigo, string nombre)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        CreatedBy = "System";
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;

    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<Rol> Roles => _roles.AsReadOnly();
}
