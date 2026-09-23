using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("Sesiones");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("SesionId")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.UsuarioId)
            .IsRequired();

        builder.Property(s => s.RefreshToken)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(s => s.RefreshToken)
            .IsUnique()
            .HasDatabaseName("UQ_Sesiones_RefreshToken");

        builder.Property(s => s.FechaExpiracion)
            .IsRequired();

        builder.Property(s => s.Activa)
            .IsRequired();

        builder.Property(s => s.CreatedDate)
            .IsRequired();
    }
}
