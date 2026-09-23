using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PerfilEstructuradoConfiguration : IEntityTypeConfiguration<PerfilEstructurado>
{
    public void Configure(EntityTypeBuilder<PerfilEstructurado> builder)
    {
        builder.ToTable("PerfilEstructurado");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PerfilEstructuradoId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.SolicitudId)
            .IsRequired();

        builder.Property(p => p.ObjetivoPrincipalCargo)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.PerfilIdealCandidato)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.PerfilTipoAltoAjuste)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.EstadoGeneracion)
            .IsRequired()
            .HasMaxLength(100);

        // NVARCHAR(MAX) columns for JSON structures
        builder.Property(p => p.DatosGeneralesCargo)
            .IsRequired();

        builder.Property(p => p.PerfilRequerido)
            .IsRequired();

        builder.Property(p => p.HerramientasSistemas)
            .IsRequired();

        builder.Property(p => p.FiltrosClaveSeleccion)
            .IsRequired();

        builder.Property(p => p.ConocimientosTecnicosRequeridos)
            .IsRequired();

        builder.Property(p => p.FuncionesPrincipalesCargo)
            .IsRequired();

        builder.Property(p => p.CompetenciasClave)
            .IsRequired();

        builder.Property(p => p.IndicadoresExitoCargo)
            .IsRequired();

        builder.Property(p => p.MatrizPonderacion)
            .IsRequired();

        builder.Property(p => p.FuentesUtilizadas)
            .IsRequired();

        builder.Property(p => p.Alertas)
            .IsRequired();

        // Audit fields
        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.ModifiedDate)
            .IsRequired(false);

        // Unique constraint on SolicitudId
        builder.HasIndex(p => p.SolicitudId)
            .IsUnique()
            .HasDatabaseName("UQ_PerfilEstructurado_Solicitud");

        // Relationship
        builder.HasOne(p => p.Solicitud)
            .WithMany()
            .HasForeignKey(p => p.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PerfilEstructurado_Solicitudes");
    }
}
