using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PostulanteConfiguration : IEntityTypeConfiguration<Postulante>
{
    public void Configure(EntityTypeBuilder<Postulante> builder)
    {
        builder.ToTable("Postulantes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PostulanteId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Nombres)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Apellidos)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Correo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(p => p.Origen)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("LinkedIn");

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

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
