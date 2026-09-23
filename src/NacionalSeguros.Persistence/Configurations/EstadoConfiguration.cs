using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class EstadoConfiguration : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("Estados");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("EstadoId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Estados_Codigo");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Entidad)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SLAId)
            .HasColumnName("SLAId");
    }
}
