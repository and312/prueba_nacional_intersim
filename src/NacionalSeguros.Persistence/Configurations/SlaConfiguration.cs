using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class SlaConfiguration : IEntityTypeConfiguration<Sla>
{
    public void Configure(EntityTypeBuilder<Sla> builder)
    {
        builder.ToTable("SLAs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("SLAId")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DiasMaximos)
            .IsRequired();

        builder.Property(s => s.Modulo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.CreatedDate)
            .IsRequired();

        builder.Property(s => s.IsDeleted)
            .IsRequired();

        // Soft Delete Query Filter
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
