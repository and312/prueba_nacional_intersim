using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PostulacionConfiguration : IEntityTypeConfiguration<Postulacion>
{
    public void Configure(EntityTypeBuilder<Postulacion> builder)
    {
        builder.ToTable("Postulaciones");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PostulacionId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.PostulanteId)
            .IsRequired();

        builder.Property(p => p.VacanteId)
            .IsRequired();

        builder.Property(p => p.FechaPostulacion)
            .IsRequired();

        builder.Property(p => p.EstadoPipelineId)
            .IsRequired();

        builder.Property(p => p.PretensionSalarial)
            .IsRequired()
            .HasColumnType("DECIMAL(18,2)");

        // Audit properties
        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ModifiedDate);

        builder.Property(p => p.IsDeleted)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Postulante)
            .WithMany()
            .HasForeignKey(p => p.PostulanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Postulaciones_Postulantes");

        builder.HasOne(p => p.Vacante)
            .WithMany()
            .HasForeignKey(p => p.VacanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Postulaciones_Vacantes");

        builder.HasOne(p => p.EstadoPipeline)
            .WithMany()
            .HasForeignKey(p => p.EstadoPipelineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Postulaciones_Estados");

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
