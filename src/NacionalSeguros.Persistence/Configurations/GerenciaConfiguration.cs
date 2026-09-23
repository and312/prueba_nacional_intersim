using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class GerenciaConfiguration : IEntityTypeConfiguration<Gerencia>
{
    public void Configure(EntityTypeBuilder<Gerencia> builder)
    {
        builder.ToTable("Gerencias");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("GerenciaId")
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(g => g.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Gerencias_Nombre");

        builder.Property(g => g.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(g => g.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Soft Delete Query Filter
        builder.HasQueryFilter(g => !g.IsDeleted);
    }
}
