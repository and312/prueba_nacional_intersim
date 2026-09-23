using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Postulante : Entity<int>
{
    // Requerido por EF Core
    protected Postulante()
    {
    }

    public Postulante(
        string nombres,
        string apellidos,
        string correo,
        string documentoIdentidad,
        string origen,
        string createdBy)
    {
        Nombres = nombres ?? throw new ArgumentNullException(nameof(nombres));
        Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
        Correo = correo ?? throw new ArgumentNullException(nameof(correo));
        DocumentoIdentidad = documentoIdentidad ?? throw new ArgumentNullException(nameof(documentoIdentidad));
        Origen = origen ?? "LinkedIn";
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public string Nombres { get; private set; } = string.Empty;
    public string Apellidos { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public string DocumentoIdentidad { get; private set; } = string.Empty;
    public string Origen { get; private set; } = "LinkedIn";

    // Campos de Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Métodos de Dominio
    public void ActualizarDatos(string nombres, string apellidos, string documentoIdentidad, string modificadoPor)
    {
        Nombres = nombres ?? throw new ArgumentNullException(nameof(nombres));
        Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
        DocumentoIdentidad = documentoIdentidad ?? throw new ArgumentNullException(nameof(documentoIdentidad));
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
