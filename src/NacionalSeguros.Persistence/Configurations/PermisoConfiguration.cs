using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("Permisos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PermisoId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Permisos_Codigo");

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
