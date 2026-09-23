using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Area : Entity<int>
{
    private readonly List<Usuario> _usuarios = new();

    protected Area()
    {
    }

    public Area(string codigo, string nombre, int gerenciaId, string? responsable = null)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        GerenciaId = gerenciaId;
        Responsable = responsable;
        Estado = "Activo";
        CreatedBy = "System";
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int GerenciaId { get; private set; }
    public Gerencia Gerencia { get; private set; } = null!;
    public string? Responsable { get; private set; }
    public string Estado { get; private set; } = "Activo";

    // Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<Usuario> Usuarios => _usuarios.AsReadOnly();

    public void Actualizar(string nombre, int gerenciaId, string? responsable, string modificadoPor)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        GerenciaId = gerenciaId;
        Responsable = responsable;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Activar(string modificadoPor)
    {
        Estado = "Activo";
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Desactivar(string modificadoPor)
    {
        Estado = "Inactivo";
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void EliminarLogicamente()
    {
        IsDeleted = true;
    }
}
