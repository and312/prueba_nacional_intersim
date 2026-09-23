using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AreaId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Areas_Codigo");

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.GerenciaId)
            .IsRequired();

        builder.HasOne(a => a.Gerencia)
            .WithMany()
            .HasForeignKey(a => a.GerenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Responsable)
            .HasMaxLength(100);

        builder.Property(a => a.Estado)
            .IsRequired()
            .HasMaxLength(20);

        // Soft Delete Query Filter
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
