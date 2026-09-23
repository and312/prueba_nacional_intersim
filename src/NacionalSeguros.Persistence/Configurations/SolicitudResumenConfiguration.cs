using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class SolicitudResumenConfiguration : IEntityTypeConfiguration<SolicitudResumen>
{
    public void Configure(EntityTypeBuilder<SolicitudResumen> builder)
    {
        builder.ToTable("SolicitudResumenes");

        builder.HasKey(sr => sr.ResumenId);

        builder.Property(sr => sr.ResumenId)
            .ValueGeneratedOnAdd();

        builder.Property(sr => sr.SolicitudId)
            .IsRequired();

        builder.Property(sr => sr.ProfileSummary)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(sr => sr.CompletitudPorcentaje)
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.Property(sr => sr.CamposDetectados)
            .IsRequired(false);

        builder.Property(sr => sr.CamposEsperados)
            .IsRequired(false);

        builder.Property(sr => sr.CaptureState)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(sr => sr.CaptureConfidence)
            .HasPrecision(4, 2)
            .IsRequired(false);

        builder.Property(sr => sr.Recommendation)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(sr => sr.CamposFaltantesJson)
            .IsRequired(false);

        builder.Property(sr => sr.InconsistenciasJson)
            .IsRequired(false);

        builder.Property(sr => sr.AgentName)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(sr => sr.AgentVersion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sr => sr.CorrelationId)
            .IsRequired(false);

        builder.Property(sr => sr.CreatedDate)
            .IsRequired();

        builder.Property(sr => sr.ModifiedDate)
            .IsRequired(false);

        // Relaciones
        builder.HasOne(sr => sr.Solicitud)
            .WithOne()
            .HasForeignKey<SolicitudResumen>(sr => sr.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SolicitudResumenes_Solicitudes");
    }
}
