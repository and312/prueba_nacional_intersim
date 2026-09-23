using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("Cargos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("CargoId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.AreaCargoId)
            .IsRequired();

        builder.HasOne(c => c.AreaCargo)
            .WithMany(a => a.Cargos)
            .HasForeignKey(c => c.AreaCargoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Cargos_Codigo");

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Descripcion)
            .HasMaxLength(250);

        builder.Property(c => c.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(100);

        builder.Property(c => c.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.Ignore(c => c.Solicitudes);
    }
}
