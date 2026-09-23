using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Sla : Entity<int>
{
    protected Sla()
    {
    }

    public Sla(string nombre, int diasMaximos, string modulo, string createdBy)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        DiasMaximos = diasMaximos;
        Modulo = modulo ?? throw new ArgumentNullException(nameof(modulo));
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Nombre { get; private set; } = string.Empty;
    public int DiasMaximos { get; private set; }
    public string Modulo { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }
}
