using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ModalidadTrabajoConfiguration : IEntityTypeConfiguration<ModalidadTrabajo>
{
    public void Configure(EntityTypeBuilder<ModalidadTrabajo> builder)
    {
        builder.ToTable("ModalidadesTrabajo");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("ModalidadTrabajoId")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_ModalidadesTrabajo_Codigo");

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Descripcion)
            .HasMaxLength(250);

        builder.Property(m => m.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.CreatedDate)
            .IsRequired();

        builder.Property(m => m.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(m => m.DeletedBy)
            .HasMaxLength(100);

        builder.Property(m => m.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
