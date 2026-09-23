using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class VacanteConfiguration : IEntityTypeConfiguration<Vacante>
{
    public void Configure(EntityTypeBuilder<Vacante> builder)
    {
        builder.ToTable("Vacantes");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("VacanteId")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.PerfilCargoId)
            .IsRequired();

        builder.Property(v => v.SolicitudId)
            .IsRequired();

        builder.Property(v => v.EstadoId)
            .IsRequired();

        builder.Property(v => v.FechaApertura)
            .IsRequired();

        builder.Property(v => v.FechaCierre);

        builder.Property(v => v.BandaSalarialMin)
            .IsRequired()
            .HasColumnType("DECIMAL(18,2)");

        builder.Property(v => v.BandaSalarialMax)
            .IsRequired()
            .HasColumnType("DECIMAL(18,2)");

        // Audit properties
        builder.Property(v => v.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.CreatedDate)
            .IsRequired();

        builder.Property(v => v.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(v => v.ModifiedDate);

        builder.Property(v => v.IsDeleted)
            .IsRequired();

        // Relationships
        builder.HasOne(v => v.PerfilCargo)
            .WithMany()
            .HasForeignKey(v => v.PerfilCargoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Vacantes_PerfilesCargo");

        builder.HasOne(v => v.Solicitud)
            .WithMany()
            .HasForeignKey(v => v.SolicitudId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Vacantes_Solicitudes");

        builder.HasOne(v => v.Estado)
            .WithMany()
            .HasForeignKey(v => v.EstadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Vacantes_Estados");

        // Query filter for Soft Delete
        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
