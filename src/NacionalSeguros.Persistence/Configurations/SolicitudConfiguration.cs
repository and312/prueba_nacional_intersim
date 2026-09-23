using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable("Solicitudes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("SolicitudId")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Cargo)
            .IsRequired()
            .HasMaxLength(400)
            .HasColumnName("Cargo");

        builder.Property(s => s.CargoId)
            .HasColumnName("CargoId")
            .IsRequired(false);

        builder.Property(s => s.SolicitanteId)
            .IsRequired();

        builder.Property(s => s.DecisorId)
            .IsRequired(false);

        builder.Property(s => s.RegionalId)
            .IsRequired(false);

        builder.Property(s => s.TipoSolicitudId)
            .IsRequired(false);

        builder.Property(s => s.ModalidadTrabajoId)
            .IsRequired(false);

        builder.Property(s => s.Seniority)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Prioridad)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Funciones)
            .IsRequired();

        builder.Property(s => s.EstadoId)
            .IsRequired();

        builder.Property(s => s.Codigo)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(s => s.Motivo)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.CantidadVacantes)
            .IsRequired(false);

        builder.Property(s => s.Observaciones)
            .IsRequired(false);

        // Perfil Requerido
        builder.Property(s => s.ObjetivoCargo)
            .IsRequired(false);

        builder.Property(s => s.FormacionAcademica)
            .IsRequired(false);

        builder.Property(s => s.ExperienciaMinima)
            .IsRequired(false);

        builder.Property(s => s.ExperienciaIndispensable)
            .IsRequired(false);

        builder.Property(s => s.ConocimientosTecnicos)
            .IsRequired(false);

        builder.Property(s => s.HerramientasSistemas)
            .IsRequired(false);

        builder.Property(s => s.CompetenciasClave)
            .IsRequired(false);

        builder.Property(s => s.DisponibilidadRequerida)
            .IsRequired(false);

        builder.Property(s => s.CriteriosExcluyentes)
            .IsRequired(false);

        builder.Property(s => s.CriteriosDeseables)
            .IsRequired(false);

        // Audit properties
        builder.Property(s => s.CreatedDate)
            .IsRequired();

        builder.Property(s => s.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(s => s.DeletedBy)
            .HasMaxLength(100);

        // Integration properties
        builder.Property(s => s.CanalOrigen)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue("BackOffice");

        builder.Property(s => s.WorkflowOrigen)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(s => s.ApiKeyId)
            .IsRequired(false);

        builder.Property(s => s.CorrelationId)
            .IsRequired(false);

        builder.Property(s => s.Iteracion)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasOne<ApiKey>()
            .WithMany()
            .HasForeignKey(s => s.ApiKeyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_ApiKeys");

        // Relationships
        builder.HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_Usuarios_Solicitante");

        builder.HasOne(s => s.Decisor)
            .WithMany()
            .HasForeignKey(s => s.DecisorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_Usuarios_Decisor");

        builder.HasOne(s => s.Estado)
            .WithMany()
            .HasForeignKey(s => s.EstadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_Estados");

        builder.HasOne(s => s.Regional)
            .WithMany(r => r.Solicitudes)
            .HasForeignKey(s => s.RegionalId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_Regionales");

        builder.HasOne(s => s.TipoSolicitud)
            .WithMany(t => t.Solicitudes)
            .HasForeignKey(s => s.TipoSolicitudId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_TiposSolicitud");

        builder.HasOne(s => s.ModalidadTrabajo)
            .WithMany(m => m.Solicitudes)
            .HasForeignKey(s => s.ModalidadTrabajoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_ModalidadesTrabajo");

        builder.HasOne(s => s.CargoEntity)
            .WithMany()
            .HasForeignKey(s => s.CargoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Solicitudes_Cargos_CargoId");

        // Query filter for Soft Delete
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
