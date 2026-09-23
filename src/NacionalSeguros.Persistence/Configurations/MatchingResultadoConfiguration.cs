using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class MatchingResultadoConfiguration : IEntityTypeConfiguration<MatchingResultado>
{
    public void Configure(EntityTypeBuilder<MatchingResultado> builder)
    {
        builder.ToTable("MatchingResultados");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("MatchingResultadoId")
            .HasColumnOrder(1)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.MatchingEjecucionId)
            .HasColumnOrder(2)
            .IsRequired();

        builder.Property(p => p.PostulanteId)
            .HasColumnOrder(3)
            .IsRequired();

        builder.Property(p => p.Origen)
            .HasColumnOrder(4)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PorcentajeMatching)
            .HasColumnOrder(5)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(p => p.Clasificacion)
            .HasColumnOrder(6)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PosicionRanking)
            .HasColumnOrder(7)
            .IsRequired();

        builder.Property(p => p.PuntajeObtenido)
            .HasColumnOrder(8)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(p => p.PuntajeMaximo)
            .HasColumnOrder(9)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(p => p.FortalezasJson)
            .HasColumnOrder(10)
            .IsRequired();

        builder.Property(p => p.BrechasJson)
            .HasColumnOrder(11)
            .IsRequired();

        builder.Property(p => p.DesgloseCriteriosJson)
            .HasColumnOrder(12)
            .IsRequired();

        builder.Property(p => p.TipoDescarte)
            .HasColumnOrder(13)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.MotivoExclusion)
            .HasColumnOrder(14)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(p => p.CreatedDate)
            .HasColumnOrder(15)
            .IsRequired();

        // Relaciones
        builder.HasOne(p => p.MatchingEjecucion)
            .WithMany()
            .HasForeignKey(p => p.MatchingEjecucionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MatchingResultados_MatchingEjecuciones");
    }
}
