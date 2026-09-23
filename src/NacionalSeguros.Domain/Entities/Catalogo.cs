using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Catalogo : Entity<int>
{
    private readonly List<Parametro> _parametros = new();

    // Requerido por EF Core
    protected Catalogo()
    {
    }

    public Catalogo(string nombre, string codigo, string createdBy)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;

    // Campos de Auditoría Básicos
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedades de navegación
    public IReadOnlyCollection<Parametro> Parametros => _parametros.AsReadOnly();

    // Métodos de Dominio
    public void Actualizar(string nombre)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
    }

    public void EliminarLogicamente()
    {
        IsDeleted = true;
        foreach (var parametro in _parametros)
        {
            parametro.EliminarLogicamente();
        }
    }

    public void AgregarParametro(Parametro parametro)
    {
        if (parametro == null) throw new ArgumentNullException(nameof(parametro));
        if (!_parametros.Contains(parametro))
        {
            _parametros.Add(parametro);
        }
    }
}
