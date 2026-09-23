using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("RolId")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Roles_Nombre");

        builder.Property(r => r.Descripcion)
            .HasMaxLength(250);

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.IsDeleted)
            .IsRequired();

        // Relación Muchos a Muchos con Permisos (vía tabla RolPermisos)
        builder.HasMany(r => r.Permisos)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RolPermisos",
                p => p.HasOne<Permiso>().WithMany().HasForeignKey("PermisoId"),
                r => r.HasOne<Rol>().WithMany().HasForeignKey("RolId"),
                je =>
                {
                    je.HasKey("RolId", "PermisoId");
                    je.ToTable("RolPermisos");
                });

        // Soft Delete Query Filter
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
