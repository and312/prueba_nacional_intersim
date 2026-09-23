using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKeys");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("ApiKeyId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ApiKeyHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Workflow)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Descripcion)
            .HasMaxLength(250);

        builder.Property(a => a.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Activo");

        builder.Property(a => a.FechaCreacion)
            .IsRequired();

        builder.Property(a => a.FechaExpiracion)
            .IsRequired(false);

        builder.Property(a => a.UltimoUso)
            .IsRequired(false);

        builder.Property(a => a.UltimaIP)
            .HasMaxLength(45)
            .IsRequired(false);

        builder.Property(a => a.CreadoPor)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Permisos)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.IntegracionId)
            .IsRequired(false);

        builder.Property(a => a.Observaciones)
            .HasMaxLength(1000);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ModifiedDate)
            .IsRequired(false);

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(100);

        builder.Property(a => a.DeletedDate)
            .IsRequired(false);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(a => a.ApiKeyHash)
            .IsUnique();

        builder.HasOne(a => a.Integracion)
            .WithMany(i => i.ApiKeys)
            .HasForeignKey(a => a.IntegracionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
