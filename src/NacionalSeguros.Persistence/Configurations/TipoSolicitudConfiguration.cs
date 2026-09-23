using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class TipoSolicitudConfiguration : IEntityTypeConfiguration<TipoSolicitudEntity>
{
    public void Configure(EntityTypeBuilder<TipoSolicitudEntity> builder)
    {
        builder.ToTable("TiposSolicitud");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("TipoSolicitudId")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_TiposSolicitud_Codigo");

        builder.Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Descripcion)
            .HasMaxLength(250);

        builder.Property(t => t.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100);

        builder.Property(t => t.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
