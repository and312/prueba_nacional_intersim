using System;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class PerfilCargo : Entity<int>
{
    // Requerido por EF Core
    protected PerfilCargo()
    {
        Secciones = new List<PerfilSeccion>();
    }

    public PerfilCargo(
        int solicitudId,
        string cargo,
        string descripcion,
        int version,
        int estadoId,
        string createdBy)
    {
        SolicitudId = solicitudId;
        Cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
        Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
        Version = version;
        EstadoId = estadoId;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
        Activo = true;
        Secciones = new List<PerfilSeccion>();
    }

    public int SolicitudId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Cargo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public int EstadoId { get; private set; }
    
    // Columnas de Reestructuración de Perfil
    public string? PdfUrl { get; private set; }
    public string? JsonOriginalIA { get; private set; }
    public string? JsonActual { get; private set; }
    public bool Activo { get; private set; }
    
    // Columna de Salario
    public string? Salario { get; private set; }

    // Campos de Auditoría
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedades de navegación
    public Solicitud Solicitud { get; private set; } = null!;
    public Estado Estado { get; private set; } = null!;
    public ICollection<PerfilSeccion> Secciones { get; private set; }

    // Métodos de Dominio
    public void Actualizar(string cargo, string descripcion, string modificadoPor)
    {
        // Solo permitir modificaciones si está en Borrador (1) u Observado (4)
        if (EstadoId != 1 && EstadoId != 4)
        {
            throw new InvalidOperationException("Solo se pueden modificar perfiles en estado Borrador u Observado.");
        }

        Cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
        Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetPdfUrl(string pdfUrl)
    {
        PdfUrl = pdfUrl ?? throw new ArgumentNullException(nameof(pdfUrl));
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetCodigo(string codigo)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
    }

    public void ActualizarContenidoConsolidado(string descripcion, string modificadoPor)
    {
        Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
        JsonActual = descripcion;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetJsonOriginalIA(string jsonOriginalIA)
    {
        JsonOriginalIA = jsonOriginalIA;
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetJsonActual(string jsonActual)
    {
        JsonActual = jsonActual;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
        ModifiedDate = DateTime.UtcNow;
    }

    public void ActualizarSalario(string? salario)
    {
        Salario = salario;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Activar()
    {
        Activo = true;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Aprobar(string modificadoPor)
    {
        EstadoId = 3; // SOL-APR (Aprobada)
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Observar(string modificadoPor)
    {
        EstadoId = 4; // SOL-OBS (Observada)
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void GenerarPerfil(int estadoId, string descripcion)
    {
        Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
        EstadoId = estadoId;
        ModifiedBy = "AgentePerfil";
        ModifiedDate = DateTime.UtcNow;
    }

    public void AprobarRRHH(int estadoId, string modificadoPor)
    {
        EstadoId = estadoId;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void EnviarArea(int estadoId, string modificadoPor)
    {
        EstadoId = estadoId;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }


    public void CambiarEstado(int estadoId, string modificadoPor)
    {
        EstadoId = estadoId;
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }

    public void IncrementarVersion()
    {
        Version += 1;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
