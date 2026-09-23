using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class AreaCargoConfiguration : IEntityTypeConfiguration<AreaCargo>
{
    public void Configure(EntityTypeBuilder<AreaCargo> builder)
    {
        builder.ToTable("AreasCargo");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AreaCargoId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_AreasCargo_Codigo");

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Descripcion)
            .HasMaxLength(250);

        builder.Property(a => a.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.CreatedDate)
            .IsRequired();

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(100);

        builder.Property(a => a.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
