using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class ParametroConfiguration : IEntityTypeConfiguration<Parametro>
{
    public void Configure(EntityTypeBuilder<Parametro> builder)
    {
        builder.ToTable("Parametros", tb => tb.HasTrigger("trg_Parametro_PreventCircular"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("ParametroId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CatalogoId)
            .IsRequired();

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Valor)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.ParametroIdPadre)
            .HasColumnName("ParametroIdPadre");

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.IsDeleted)
            .IsRequired();

        builder.Property(p => p.Orden)
            .HasColumnName("Orden")
            .IsRequired()
            .HasDefaultValue(0);

        // Unique index for (CatalogoId, Codigo)
        builder.HasIndex(p => new { p.CatalogoId, p.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Parametros_Catalogo_Codigo");

        // Relationships
        builder.HasOne(p => p.Catalogo)
            .WithMany(c => c.Parametros)
            .HasForeignKey(p => p.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Padre)
            .WithMany(p => p.Hijos)
            .HasForeignKey(p => p.ParametroIdPadre)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft Delete Query Filter
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
