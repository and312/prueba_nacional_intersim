using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class TipoObservacionConfiguration : IEntityTypeConfiguration<TipoObservacion>
{
    public void Configure(EntityTypeBuilder<TipoObservacion> builder)
    {
        builder.ToTable("TiposObservacion");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("TipoObservacionId")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_TiposObservacion_Codigo");

        builder.Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Descripcion)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(t => t.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Activo");

        // Audit properties
        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.ModifiedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(t => t.ModifiedDate)
            .IsRequired(false);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(t => t.DeletedDate)
            .IsRequired(false);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
