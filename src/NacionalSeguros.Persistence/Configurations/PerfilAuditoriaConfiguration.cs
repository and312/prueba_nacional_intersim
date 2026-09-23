using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PerfilAuditoriaConfiguration : IEntityTypeConfiguration<PerfilAuditoria>
{
    public void Configure(EntityTypeBuilder<PerfilAuditoria> builder)
    {
        builder.ToTable("PerfilAuditoria");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Id)
            .HasColumnName("PerfilAuditoriaId")
            .ValueGeneratedOnAdd();

        builder.Property(pa => pa.PerfilCargoId)
            .IsRequired();

        builder.Property(pa => pa.PerfilSeccionId)
            .IsRequired(false);

        builder.Property(pa => pa.Version)
            .IsRequired();

        builder.Property(pa => pa.SeccionModificada)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(pa => pa.ValorAnterior)
            .IsRequired(false);

        builder.Property(pa => pa.ValorNuevo)
            .IsRequired(false);

        builder.Property(pa => pa.Usuario)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pa => pa.FechaHora)
            .IsRequired();

        builder.Property(pa => pa.MotivoCambio)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(pa => pa.EstadoPerfil)
            .HasMaxLength(50)
            .IsRequired(false);

        // Relationships
        builder.HasOne(pa => pa.PerfilCargo)
            .WithMany()
            .HasForeignKey(pa => pa.PerfilCargoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PerfilAuditoria_PerfilesCargo");

        builder.HasOne(pa => pa.PerfilSeccion)
            .WithMany()
            .HasForeignKey(pa => pa.PerfilSeccionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilAuditoria_PerfilSecciones");
    }
}
