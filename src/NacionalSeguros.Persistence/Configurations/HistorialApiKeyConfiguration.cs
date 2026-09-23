using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class HistorialApiKeyConfiguration : IEntityTypeConfiguration<HistorialApiKey>
{
    public void Configure(EntityTypeBuilder<HistorialApiKey> builder)
    {
        builder.ToTable("HistorialApiKeys");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("HistorialApiKeyId")
            .ValueGeneratedOnAdd();

        builder.Property(h => h.ApiKeyId)
            .IsRequired();

        builder.Property(h => h.Accion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.Fecha)
            .IsRequired();

        builder.Property(h => h.RealizadoPor)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.Detalle)
            .HasMaxLength(500);

        builder.HasOne(h => h.ApiKey)
            .WithMany()
            .HasForeignKey(h => h.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
