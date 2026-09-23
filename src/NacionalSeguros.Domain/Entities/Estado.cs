using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Estado : Entity<int>
{
    // Requerido por EF Core
    protected Estado()
    {
    }

    public Estado(string codigo, string nombre, string entidad, int? slaId = null)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Entidad = entidad ?? throw new ArgumentNullException(nameof(entidad));
        SLAId = slaId;
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Entidad { get; private set; } = string.Empty;
    public int? SLAId { get; private set; }
}
