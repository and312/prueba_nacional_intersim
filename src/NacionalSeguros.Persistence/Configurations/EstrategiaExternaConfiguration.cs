using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class EstrategiaExternaConfiguration : IEntityTypeConfiguration<EstrategiaExterna>
{
    public void Configure(EntityTypeBuilder<EstrategiaExterna> builder)
    {
        builder.ToTable("EstrategiaExternas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("EstrategiaExternaId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.MatchingEjecucionId)
            .IsRequired();

        builder.Property(p => p.Prioridad)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.Justificacion)
            .IsRequired(false);

        builder.Property(p => p.CriteriosDificiles)
            .IsRequired(false);

        builder.Property(p => p.PublicoObjetivo)
            .IsRequired(false);

        builder.Property(p => p.CanalesSugeridos)
            .IsRequired(false);

        builder.Property(p => p.PlanAccion)
            .IsRequired(false);

        builder.Property(p => p.BriefingEditable)
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
            .HasForeignKey<EstrategiaExterna>(p => p.MatchingEjecucionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EstrategiaExternas_MatchingEjecuciones");

        builder.HasOne(p => p.Estado)
            .WithMany()
            .HasForeignKey(p => p.EstadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EstrategiaExternas_Estados");
    }
}
