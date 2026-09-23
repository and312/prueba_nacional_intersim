using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AuditId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.FechaHoraUTC)
            .IsRequired();

        builder.Property(a => a.UsuarioId)
            .IsRequired(false);

        builder.Property(a => a.UsuarioNombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Rol)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Modulo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Entidad)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntidadId)
            .IsRequired();

        builder.Property(a => a.Accion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EstadoAnterior);
        builder.Property(a => a.EstadoNuevo);

        builder.Property(a => a.Canal)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.CorrelationId);
    }
}
