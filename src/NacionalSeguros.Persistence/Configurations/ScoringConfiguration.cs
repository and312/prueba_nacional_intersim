using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ScoringConfiguration : IEntityTypeConfiguration<Scoring>
{
    public void Configure(EntityTypeBuilder<Scoring> builder)
    {
        builder.ToTable("Scorings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("ScoringId")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.PostulanteId)
            .IsRequired();

        builder.Property(s => s.VacanteId)
            .IsRequired();

        builder.Property(s => s.ScoreSkills)
            .IsRequired();

        builder.Property(s => s.ScoreExperiencia)
            .IsRequired();

        builder.Property(s => s.ScoreFinal)
            .IsRequired()
            .HasColumnType("DECIMAL(5,2)");

        builder.Property(s => s.JustificacionText)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(s => s.ExecutionId)
            .IsRequired();

        builder.Property(s => s.CreatedDate)
            .IsRequired();

        // Relationships
        builder.HasOne(s => s.Postulante)
            .WithMany()
            .HasForeignKey(s => s.PostulanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Scorings_Postulantes");

        builder.HasOne(s => s.Vacante)
            .WithMany()
            .HasForeignKey(s => s.VacanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Scorings_Vacantes");

        builder.HasOne(s => s.AgentExecution)
            .WithMany()
            .HasForeignKey(s => s.ExecutionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Scorings_AgentExecutions");
    }
}
