using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Parametro : Entity<int>
{
    private readonly List<Parametro> _hijos = new();

    // Requerido por EF Core
    protected Parametro()
    {
    }

    public Parametro(int catalogoId, string codigo, string valor, int? parametroIdPadre, string createdBy)
    {
        CatalogoId = catalogoId;
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        ParametroIdPadre = parametroIdPadre;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
        Orden = 0;
    }

    public int CatalogoId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;
    public int? ParametroIdPadre { get; private set; }
    public int Orden { get; private set; }

    // Campos de Auditoría Básicos
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedades de navegación
    public Catalogo Catalogo { get; private set; } = null!;
    public Parametro? Padre { get; private set; }
    public IReadOnlyCollection<Parametro> Hijos => _hijos.AsReadOnly();

    public void SetCatalogo(Catalogo catalogo)
    {
        Catalogo = catalogo ?? throw new ArgumentNullException(nameof(catalogo));
        CatalogoId = catalogo.Id;
    }

    // Métodos de Dominio
    public void Actualizar(string valor, int? parametroIdPadre)
    {
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        ParametroIdPadre = parametroIdPadre;
    }

    public void EliminarLogicamente()
    {
        IsDeleted = true;
        foreach (var hijo in _hijos)
        {
            hijo.EliminarLogicamente();
        }
    }
}
