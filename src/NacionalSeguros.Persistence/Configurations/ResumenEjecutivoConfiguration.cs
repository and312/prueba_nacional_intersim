using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ResumenEjecutivoConfiguration : IEntityTypeConfiguration<ResumenEjecutivo>
{
    public void Configure(EntityTypeBuilder<ResumenEjecutivo> builder)
    {
        builder.ToTable("ResumenEjecutivos");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("ResumenId")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.PerfilCargoId)
            .IsRequired();

        builder.Property(r => r.Resumen)
            .IsRequired();

        builder.Property(r => r.ObjetivoCargo)
            .IsRequired();

        builder.Property(r => r.FuncionesPrincipales)
            .IsRequired();

        builder.Property(r => r.RequisitosMinimos)
            .IsRequired();

        builder.Property(r => r.FormacionExperiencia)
            .IsRequired();

        builder.Property(r => r.HardSkills)
            .IsRequired();

        builder.Property(r => r.SoftSkills)
            .IsRequired();

        builder.Property(r => r.Modalidad)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Ubicacion)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.BandaSalarial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.CriteriosEvaluacion)
            .IsRequired();

        builder.Property(r => r.CaracteristicasClave)
            .IsRequired();

        builder.Property(r => r.ValoracionPerfil)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(r => r.ModifiedDate)
            .IsRequired(false);

        // Unique constraint on PerfilCargoId to enforce 1-to-1 relationship
        builder.HasIndex(r => r.PerfilCargoId)
            .IsUnique()
            .HasDatabaseName("UQ_ResumenEjecutivos_PerfilCargo");

        // Relationship
        builder.HasOne(r => r.PerfilCargo)
            .WithMany()
            .HasForeignKey(r => r.PerfilCargoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ResumenEjecutivos_PerfilesCargo");
    }
}
