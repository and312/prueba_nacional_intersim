using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class IntegracionConfiguration : IEntityTypeConfiguration<Integracion>
{
    public void Configure(EntityTypeBuilder<Integracion> builder)
    {
        builder.ToTable("Integraciones");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("IntegracionId")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Descripcion)
            .HasMaxLength(500);

        builder.Property(i => i.Tipo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Responsable)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.CorreoResponsable)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Activo");

        builder.Property(i => i.Observaciones)
            .HasMaxLength(1000);

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.CreatedDate)
            .IsRequired();

        builder.Property(i => i.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(i => i.ModifiedDate)
            .IsRequired(false);

        builder.Property(i => i.DeletedBy)
            .HasMaxLength(100);

        builder.Property(i => i.DeletedDate)
            .IsRequired(false);

        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(i => i.Codigo)
            .IsUnique();

        builder.HasMany(i => i.ApiKeys)
            .WithOne(a => a.Integracion)
            .HasForeignKey(a => a.IntegracionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
