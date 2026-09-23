using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ApiKeyPermisoConfiguration : IEntityTypeConfiguration<ApiKeyPermiso>
{
    public void Configure(EntityTypeBuilder<ApiKeyPermiso> builder)
    {
        builder.ToTable("ApiKeyPermisos");

        builder.HasKey(ap => new { ap.ApiKeyId, ap.PermisoId });

        builder.HasOne(ap => ap.ApiKey)
            .WithMany(a => a.ApiKeyPermisos)
            .HasForeignKey(ap => ap.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.Permiso)
            .WithMany()
            .HasForeignKey(ap => ap.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
