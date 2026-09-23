using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class MatchingConfiguration : IEntityTypeConfiguration<Matching>
{
    public void Configure(EntityTypeBuilder<Matching> builder)
    {
        builder.ToTable("Matchings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("MatchingId")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.PostulanteId)
            .IsRequired();

        builder.Property(m => m.VacanteId)
            .IsRequired();

        builder.Property(m => m.ScoreCoincidencia)
            .IsRequired()
            .HasColumnType("DECIMAL(5,2)");

        builder.Property(m => m.CoincidenciasText)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(m => m.BrechasText)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(m => m.ExecutionId)
            .IsRequired();

        builder.Property(m => m.CreatedDate)
            .IsRequired();

        // Relationships
        builder.HasOne(m => m.Postulante)
            .WithMany()
            .HasForeignKey(m => m.PostulanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Matchings_Postulantes");

        builder.HasOne(m => m.Vacante)
            .WithMany()
            .HasForeignKey(m => m.VacanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Matchings_Vacantes");

        builder.HasOne(m => m.AgentExecution)
            .WithMany()
            .HasForeignKey(m => m.ExecutionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Matchings_AgentExecutions");
    }
}
