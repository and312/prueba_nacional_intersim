using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class CatalogoConfiguration : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        builder.ToTable("Catalogos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("CatalogoId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Catalogos_Codigo");

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired();

        builder.HasMany(c => c.Parametros)
            .WithOne(p => p.Catalogo)
            .HasForeignKey(p => p.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft Delete Query Filter
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
