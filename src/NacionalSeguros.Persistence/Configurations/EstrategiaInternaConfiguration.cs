using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class EstrategiaInternaConfiguration : IEntityTypeConfiguration<EstrategiaInterna>
{
    public void Configure(EntityTypeBuilder<EstrategiaInterna> builder)
    {
        builder.ToTable("EstrategiaInternas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("EstrategiaInternaId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.MatchingEjecucionId)
            .IsRequired();

        builder.Property(p => p.Prioridad)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.Justificacion)
            .IsRequired(false);

        builder.Property(p => p.PlanAccion)
            .IsRequired(false);

        builder.Property(p => p.PlanEvaluacion)
            .IsRequired(false);

        builder.Property(p => p.MensajeContingencia)
            .IsRequired(false);

        builder.Property(p => p.Conclusion)
            .IsRequired(false);

        builder.Property(p => p.EstadoId)
            .IsRequired(false);

        builder.Property(p => p.FechaCreacion)
            .IsRequired();

        builder.Property(p => p.FechaModificacion)
            .IsRequired(false);

        // Relaciones e Índices
        builder.HasIndex(p => p.MatchingEjecucionId)
            .IsUnique();

        builder.HasOne(p => p.MatchingEjecucion)
            .WithOne()
            .HasForeignKey<EstrategiaInterna>(p => p.MatchingEjecucionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EstrategiaInternas_MatchingEjecuciones");

        builder.HasOne(p => p.Estado)
            .WithMany()
            .HasForeignKey(p => p.EstadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EstrategiaInternas_Estados");
    }
}
