using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class RegionalConfiguration : IEntityTypeConfiguration<Regional>
{
    public void Configure(EntityTypeBuilder<Regional> builder)
    {
        builder.ToTable("Regionales");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("RegionalId")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Regionales_Codigo");

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(250);

        builder.Property(r => r.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(r => r.DeletedBy)
            .HasMaxLength(100);

        builder.Property(r => r.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
