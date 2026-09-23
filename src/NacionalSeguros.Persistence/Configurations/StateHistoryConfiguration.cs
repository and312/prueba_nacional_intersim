using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class StateHistoryConfiguration : IEntityTypeConfiguration<StateHistory>
{
    public void Configure(EntityTypeBuilder<StateHistory> builder)
    {
        builder.ToTable("StateHistory");

        builder.HasKey(sh => sh.Id);

        builder.Property(sh => sh.Id)
            .HasColumnName("StateHistoryId")
            .ValueGeneratedOnAdd();

        builder.Property(sh => sh.Entidad)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sh => sh.EntidadId)
            .IsRequired();

        builder.Property(sh => sh.EstadoAnteriorId)
            .IsRequired(false);

        builder.Property(sh => sh.EstadoNuevoId)
            .IsRequired(false);

        builder.Property(sh => sh.UsuarioId)
            .IsRequired(false);

        builder.Property(sh => sh.Fecha)
            .IsRequired();

        builder.Property(sh => sh.Comentario)
            .HasMaxLength(500);

        builder.Property(sh => sh.CorrelationId)
            .IsRequired(false);

        builder.Property(sh => sh.Iteracion)
            .IsRequired(false);

        builder.Property(sh => sh.Rol)
            .HasMaxLength(100)
            .IsRequired(false);

        // Relationships
        builder.HasOne(sh => sh.EstadoAnterior)
            .WithMany()
            .HasForeignKey(sh => sh.EstadoAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sh => sh.EstadoNuevo)
            .WithMany()
            .HasForeignKey(sh => sh.EstadoNuevoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sh => sh.Usuario)
            .WithMany()
            .HasForeignKey(sh => sh.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
