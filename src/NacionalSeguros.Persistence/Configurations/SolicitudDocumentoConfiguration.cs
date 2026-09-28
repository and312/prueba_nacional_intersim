using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class SolicitudDocumentoConfiguration : IEntityTypeConfiguration<SolicitudDocumento>
{
    public void Configure(EntityTypeBuilder<SolicitudDocumento> builder)
    {
        builder.ToTable("SolicitudDocumentos");

        builder.HasKey(sd => sd.DocumentoId);

        builder.Property(sd => sd.DocumentoId)
            .ValueGeneratedOnAdd();

        builder.Property(sd => sd.SolicitudId)
            .IsRequired();

        builder.Property(sd => sd.TipoDocumento)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(sd => sd.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(sd => sd.StorageProvider)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(sd => sd.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sd => sd.PublicUrl)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(sd => sd.GeneradoPor)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(sd => sd.CorrelationId)
            .IsRequired(false);

        builder.Property(sd => sd.CreatedDate)
            .IsRequired();

        builder.Property(sd => sd.ContenidoBinario)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        // Relaciones
        builder.HasOne(sd => sd.Solicitud)
            .WithMany()
            .HasForeignKey(sd => sd.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SolicitudDocumentos_Solicitudes");

        builder.HasIndex(sd => new { sd.SolicitudId, sd.TipoDocumento })
            .IsUnique()
            .HasDatabaseName("UQ_SolicitudDocumentos_Solicitud_Tipo");
    }
}
