using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ApiKeyAuditoriaConfiguration : IEntityTypeConfiguration<ApiKeyAuditoria>
{
    public void Configure(EntityTypeBuilder<ApiKeyAuditoria> builder)
    {
        builder.ToTable("ApiKeyAuditoria");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("ApiKeyAuditoriaId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.FechaHora)
            .IsRequired();

        builder.Property(a => a.IntegracionId)
            .IsRequired(false);

        builder.Property(a => a.IntegracionNombre)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(a => a.Workflow)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(a => a.ApiKeyId)
            .IsRequired(false);

        builder.Property(a => a.ApiKeyNombre)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(a => a.Endpoint)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.Metodo)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.IP)
            .HasMaxLength(45)
            .IsRequired(false);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(a => a.TiempoRespuestaMs)
            .IsRequired();

        builder.Property(a => a.Resultado)
            .IsRequired()
            .HasMaxLength(50);
    }
}
