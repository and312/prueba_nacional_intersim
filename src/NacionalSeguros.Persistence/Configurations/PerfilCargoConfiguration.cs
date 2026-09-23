using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PerfilCargoConfiguration : IEntityTypeConfiguration<PerfilCargo>
{
    public void Configure(EntityTypeBuilder<PerfilCargo> builder)
    {
        builder.ToTable("PerfilesCargo");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PerfilCargoId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.SolicitudId)
            .IsRequired();

        builder.Property(p => p.Codigo)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.HasIndex(p => p.Codigo)
            .HasFilter("[Codigo] IS NOT NULL")
            .HasDatabaseName("IX_PerfilesCargo_Codigo");

        builder.Property(p => p.Cargo)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Cargo");

        builder.Property(p => p.Descripcion)
            .IsRequired();

        builder.Property(p => p.Version)
            .IsRequired();

        builder.HasIndex(p => new { p.SolicitudId, p.Version })
            .IsUnique()
            .HasDatabaseName("UQ_PerfilesCargo_Solicitud_Version");

        builder.Property(p => p.EstadoId)
            .IsRequired();

        builder.Property(p => p.PdfUrl)
            .HasMaxLength(500);

        builder.Property(p => p.JsonOriginalIA)
            .IsRequired(false);

        builder.Property(p => p.JsonActual)
            .IsRequired(false);

        builder.Property(p => p.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Salario)
            .HasMaxLength(100)
            .IsRequired(false);

        // Audit properties
        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(p => p.Solicitud)
            .WithMany()
            .HasForeignKey(p => p.SolicitudId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilesCargo_Solicitudes");

        builder.HasOne(p => p.Estado)
            .WithMany()
            .HasForeignKey(p => p.EstadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilesCargo_Estados");

        builder.HasMany(p => p.Secciones)
            .WithOne(ps => ps.PerfilCargo)
            .HasForeignKey(ps => ps.PerfilCargoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PerfilSecciones_PerfilesCargo");

        // Query filter for Soft Delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
