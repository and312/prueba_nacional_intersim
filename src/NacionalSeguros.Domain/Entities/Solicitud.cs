using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Events;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Solicitud : Entity<int>
{
    private readonly List<SolicitudComentario> _comentarios = new();

    // Requerido por EF Core
    protected Solicitud()
    {
    }

    public Solicitud(
        string cargo,
        int solicitanteId,
        int? regionalId,
        int? tipoSolicitudId,
        int? modalidadTrabajoId,
        string seniority,
        string prioridad,
        string funciones,
        int estadoId,
        string? objetivoCargo = null,
        string? formacionAcademica = null,
        string? experienciaMinima = null,
        string? experienciaIndispensable = null,
        string? conocimientosTecnicos = null,
        string? herramientasSistemas = null,
        string? competenciasClave = null,
        string? disponibilidadRequerida = null,
        string? criteriosExcluyentes = null,
        string? criteriosDeseables = null,
        string? motivo = null,
        int? cantidadVacantes = null,
        string? observaciones = null,
        string canalOrigen = "BackOffice",
        string? workflowOrigen = null,
        long? apiKeyId = null,
        Guid? correlationId = null,
        int? cargoId = null)
    {
        Cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
        CargoId = cargoId;
        SolicitanteId = solicitanteId;
        RegionalId = regionalId;
        TipoSolicitudId = tipoSolicitudId;
        ModalidadTrabajoId = modalidadTrabajoId;
        Seniority = seniority ?? throw new ArgumentNullException(nameof(seniority));
        Prioridad = prioridad ?? throw new ArgumentNullException(nameof(prioridad));
        Funciones = funciones ?? throw new ArgumentNullException(nameof(funciones));
        EstadoId = estadoId;
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
        CanalOrigen = canalOrigen ?? "BackOffice";
        WorkflowOrigen = workflowOrigen;
        ApiKeyId = apiKeyId;
        CorrelationId = correlationId;
        Motivo = motivo;
        CantidadVacantes = cantidadVacantes;
        Observaciones = observaciones;

        ObjetivoCargo = objetivoCargo;
        FormacionAcademica = formacionAcademica;
        ExperienciaMinima = experienciaMinima;
        ExperienciaIndispensable = experienciaIndispensable;
        ConocimientosTecnicos = conocimientosTecnicos;
        HerramientasSistemas = herramientasSistemas;
        CompetenciasClave = competenciasClave;
        DisponibilidadRequerida = disponibilidadRequerida;
        CriteriosExcluyentes = criteriosExcluyentes;
        CriteriosDeseables = criteriosDeseables;

        Iteracion = 1;
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Cargo { get; private set; } = string.Empty;
    public int? CargoId { get; private set; }
    public int SolicitanteId { get; private set; }
    public int? DecisorId { get; private set; }
    public int? RegionalId { get; private set; }
    public int? TipoSolicitudId { get; private set; }
    public int? ModalidadTrabajoId { get; private set; }
    public string Seniority { get; private set; } = string.Empty;
    public string Prioridad { get; private set; } = string.Empty;
    public string Funciones { get; private set; } = string.Empty;
    public int EstadoId { get; private set; }

    public string? Motivo { get; private set; }
    public int? CantidadVacantes { get; private set; }
    public string? Observaciones { get; private set; }

    // Perfil Requerido
    public string? ObjetivoCargo { get; private set; }
    public string? FormacionAcademica { get; private set; }
    public string? ExperienciaMinima { get; private set; }
    public string? ExperienciaIndispensable { get; private set; }
    public string? ConocimientosTecnicos { get; private set; }
    public string? HerramientasSistemas { get; private set; }
    public string? CompetenciasClave { get; private set; }
    public string? DisponibilidadRequerida { get; private set; }
    public string? CriteriosExcluyentes { get; private set; }
    public string? CriteriosDeseables { get; private set; }

    // Campos de Auditoría e Integración
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    public string CanalOrigen { get; private set; } = "BackOffice";
    public string? WorkflowOrigen { get; private set; }
    public long? ApiKeyId { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public int Iteracion { get; private set; }

    // Propiedades de navegación
    public Cargo? CargoEntity { get; private set; }
    public Usuario Solicitante { get; private set; } = null!;
    public Usuario? Decisor { get; private set; }
    public Estado Estado { get; private set; } = null!;
    public Regional? Regional { get; private set; }
    public TipoSolicitudEntity? TipoSolicitud { get; private set; }
    public ModalidadTrabajo? ModalidadTrabajo { get; private set; }
    public IReadOnlyCollection<SolicitudComentario> Comentarios => _comentarios.AsReadOnly();

    public void SetCargoId(int? cargoId)
    {
        CargoId = cargoId;
    }

    public void SetEstado(Estado estado)
    {
        Estado = estado ?? throw new ArgumentNullException(nameof(estado));
        EstadoId = estado.Id;
    }

    // Métodos de Dominio
    public void Actualizar(
        string cargo,
        int? regionalId,
        int? tipoSolicitudId,
        int? modalidadTrabajoId,
        string seniority,
        string prioridad,
        string funciones,
        string modificadoPor,
        string? objetivoCargo = null,
        string? formacionAcademica = null,
        string? experienciaMinima = null,
        string? experienciaIndispensable = null,
        string? conocimientosTecnicos = null,
        string? herramientasSistemas = null,
        string? competenciasClave = null,
        string? disponibilidadRequerida = null,
        string? criteriosExcluyentes = null,
        string? criteriosDeseables = null,
        string? motivo = null,
        int? cantidadVacantes = null,
        string? observaciones = null,
        string canalOrigen = "BackOffice",
        string? estadoAnteriorCodigoOverride = null)
    {
        // Solo permitir modificaciones en Borrador, Registrada, Pendiente u Observada
        if (Estado != null && Estado.Codigo != "SOL-BOR" && Estado.Codigo != "SOL-REG" && Estado.Codigo != "SOL-PEN" && Estado.Codigo != "SOL-OBS")
        {
            throw new InvalidOperationException("Solo se pueden modificar solicitudes en estado Borrador, Registrada, Pendiente u Observada.");
        }

        Cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
        RegionalId = regionalId;
        TipoSolicitudId = tipoSolicitudId;
        ModalidadTrabajoId = modalidadTrabajoId;
        Seniority = seniority ?? throw new ArgumentNullException(nameof(seniority));
        Prioridad = prioridad ?? throw new ArgumentNullException(nameof(prioridad));
        Funciones = funciones ?? throw new ArgumentNullException(nameof(funciones));
        Motivo = motivo;
        CantidadVacantes = cantidadVacantes;
        Observaciones = observaciones;

        ObjetivoCargo = objetivoCargo;
        FormacionAcademica = formacionAcademica;
        ExperienciaMinima = experienciaMinima;
        ExperienciaIndispensable = experienciaIndispensable;
        ConocimientosTecnicos = conocimientosTecnicos;
        HerramientasSistemas = herramientasSistemas;
        CompetenciasClave = competenciasClave;
        DisponibilidadRequerida = disponibilidadRequerida;
        CriteriosExcluyentes = criteriosExcluyentes;
        CriteriosDeseables = criteriosDeseables;

        // Mantener el estado si no se cambia
        string estadoAnterior = estadoAnteriorCodigoOverride ?? Estado?.Codigo ?? "SOL-REG";

        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new SolicitudActualizadaEvent(Id, estadoAnterior, modificadoPor, canalOrigen));
    }

    public void Transitar(Estado nuevoEstado, int? decisorId, string modificadoPor)
    {
        if (nuevoEstado == null) throw new ArgumentNullException(nameof(nuevoEstado));
        if (!string.Equals(nuevoEstado.Entidad, "Solicitud", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El estado especificado no pertenece a la entidad Solicitud.");
        }

        string origen = Estado?.Codigo ?? "SOL-REG";
        string destino = nuevoEstado.Codigo;

        bool esValido = false;

        if (origen == "SOL-REC" && destino == "SOL-BOR") esValido = true;
        else if (origen == "SOL-BOR" && (destino == "SOL-ENV")) esValido = true;
        else if (origen == "SOL-ENV" && (destino == "SOL-OBS" || destino == "SOL-APR" || destino == "SOL-RECH")) esValido = true;
        else if (origen == "SOL-OBS" && (destino == "SOL-COR" || destino == "SOL-ENV" || destino == "SOL-BOR")) esValido = true;
        else if (origen == "SOL-COR" && (destino == "SOL-BOR" || destino == "SOL-ENV")) esValido = true;
        else if (origen == "SOL-REG" && (destino == "SOL-PEN" || destino == "SOL-ENV")) esValido = true;
        else if (origen == "SOL-PEN" && (destino == "SOL-ENV")) esValido = true;
        else if (origen == destino) esValido = true;

        if (!esValido)
        {
            throw new InvalidOperationException($"Transición de estado inválida de {origen} a {destino}.");
        }

        if (origen == "SOL-OBS")
        {
            Iteracion++;
        }

        int estadoAnteriorId = EstadoId;
        EstadoId = nuevoEstado.Id;
        Estado = nuevoEstado;

        if (destino == "SOL-APR" || destino == "SOL-RECH")
        {
            if (decisorId == null)
            {
                throw new InvalidOperationException("Se requiere un decisor para aprobar o rechazar la solicitud.");
            }
            DecisorId = decisorId;
        }

        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;

        RaiseDomainEvent(new SolicitudEstadoTransitadoEvent(Id, estadoAnteriorId, EstadoId, modificadoPor));
    }

    public void Cancelar(string motivo, string modificadoPor)
    {
    }

    public void AgregarComentario(int usuarioId, string texto, string? estadoAsociado = null, int? iteracion = null, string? tipoComentario = null)
    {
        if (string.IsNullOrWhiteSpace(texto)) throw new ArgumentException("El texto del comentario no puede estar vacío.", nameof(texto));
        _comentarios.Add(new SolicitudComentario(Id, usuarioId, texto, estadoAsociado, iteracion, tipoComentario));
    }

    public void EliminarLogicamente(string eliminadoPor)
    {
        IsDeleted = true;
        DeletedBy = eliminadoPor;
        DeletedDate = DateTime.UtcNow;
    }
}
