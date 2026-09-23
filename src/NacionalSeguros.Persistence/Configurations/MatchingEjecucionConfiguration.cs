using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class MatchingEjecucionConfiguration : IEntityTypeConfiguration<MatchingEjecucion>
{
    public void Configure(EntityTypeBuilder<MatchingEjecucion> builder)
    {
        builder.ToTable("MatchingEjecuciones");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("MatchingEjecucionId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CodigoMatching)
            .HasColumnName("CodigoMatching")
            .HasMaxLength(50)
            .HasDefaultValueSql("LOWER(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''))")
            .IsRequired();

        builder.HasIndex(p => p.CodigoMatching)
            .IsUnique();

        builder.HasIndex(p => p.PerfilCargoId)
            .IsUnique();

        builder.Property(p => p.PerfilCargoId)
            .IsRequired();

        builder.Property(p => p.PerfilEstructuradoId)
            .IsRequired();

        builder.Property(p => p.VersionPerfil)
            .IsRequired();

        builder.Property(p => p.EstadoId)
            .IsRequired();

        builder.Property(p => p.TotalEvaluados)
            .IsRequired();

        builder.Property(p => p.TotalInternosEvaluados)
            .IsRequired();

        builder.Property(p => p.TotalHistoricosEvaluados)
            .IsRequired();

        builder.Property(p => p.TotalMatchAlto)
            .IsRequired();

        builder.Property(p => p.TotalMatchMedio)
            .IsRequired();

        builder.Property(p => p.TotalMatchBajo)
            .IsRequired();

        builder.Property(p => p.TotalDescartados)
            .IsRequired();

        builder.Property(p => p.TotalPotenciales)
            .IsRequired();

        builder.Property(p => p.CompatibilidadPromedio)
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.Property(p => p.EstrategiaRecomendada)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.NivelConfianza)
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.Property(p => p.Justificacion)
            .IsRequired(false);

        builder.Property(p => p.FuentesConsultadasJson)
            .IsRequired();

        builder.Property(p => p.ResumenDescartesJson)
            .IsRequired();

        builder.Property(p => p.ParametrosMatchingJson)
            .IsRequired();

        builder.Property(p => p.FechaInicio)
            .IsRequired();

        builder.Property(p => p.FechaFin)
            .IsRequired(false);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.MensajeError)
            .IsRequired(false);

        // Relaciones
        builder.HasOne(p => p.PerfilCargo)
            .WithMany()
            .HasForeignKey(p => p.PerfilCargoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MatchingEjecuciones_PerfilesCargo");

        builder.HasOne(p => p.PerfilEstructurado)
            .WithMany()
            .HasForeignKey(p => p.PerfilEstructuradoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MatchingEjecuciones_PerfilEstructurado");

        builder.HasOne(p => p.Estado)
            .WithMany()
            .HasForeignKey(p => p.EstadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MatchingEjecuciones_Estados");
    }
}
