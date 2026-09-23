using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PerfilSeccionConfiguration : IEntityTypeConfiguration<PerfilSeccion>
{
    public void Configure(EntityTypeBuilder<PerfilSeccion> builder)
    {
        builder.ToTable("PerfilSecciones");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id)
            .HasColumnName("PerfilSeccionId")
            .ValueGeneratedOnAdd();

        builder.Property(ps => ps.PerfilCargoId)
            .IsRequired();

        builder.Property(ps => ps.NumeroSeccion)
            .IsRequired();

        builder.Property(ps => ps.NombreSeccion)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ps => ps.Contenido)
            .IsRequired();

        builder.Property(ps => ps.Orden)
            .IsRequired();

        builder.Property(ps => ps.UltimaActualizacion)
            .IsRequired();

        builder.Property(ps => ps.UsuarioActualizacion)
            .IsRequired()
            .HasMaxLength(100);

        // Unique index
        builder.HasIndex(ps => new { ps.PerfilCargoId, ps.NumeroSeccion })
            .IsUnique()
            .HasDatabaseName("UQ_PerfilSecciones_PerfilCargo_Numero");

        // Relationship with PerfilCargo
        builder.HasOne(ps => ps.PerfilCargo)
            .WithMany(pc => pc.Secciones)
            .HasForeignKey(ps => ps.PerfilCargoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PerfilSecciones_PerfilesCargo");
    }
}
