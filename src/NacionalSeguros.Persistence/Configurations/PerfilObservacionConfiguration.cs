using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PerfilObservacionConfiguration : IEntityTypeConfiguration<PerfilObservacion>
{
    public void Configure(EntityTypeBuilder<PerfilObservacion> builder)
    {
        builder.ToTable("PerfilObservaciones");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("PerfilObservacionId")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.PerfilCargoId)
            .IsRequired();

        builder.Property(o => o.TipoObservacionId)
            .IsRequired();

        builder.Property(o => o.Comentario)
            .IsRequired();

        builder.Property(o => o.UsuarioSolicitanteId)
            .IsRequired();

        builder.Property(o => o.NumeroIteracion)
            .IsRequired();

        builder.Property(o => o.EstadoObservacion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.CreatedDate)
            .IsRequired();

        builder.Property(o => o.AtendidaPorUsuarioId)
            .IsRequired(false);

        builder.Property(o => o.FechaAtencion)
            .IsRequired(false);

        // Relationships
        builder.HasOne(o => o.PerfilCargo)
            .WithMany()
            .HasForeignKey(o => o.PerfilCargoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilObservaciones_PerfilesCargo");

        builder.HasOne(o => o.TipoObservacion)
            .WithMany()
            .HasForeignKey(o => o.TipoObservacionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilObservaciones_TiposObservacion");

        builder.HasOne(o => o.UsuarioSolicitante)
            .WithMany()
            .HasForeignKey(o => o.UsuarioSolicitanteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilObservaciones_Usuarios_Solicitante");

        builder.HasOne(o => o.AtendidaPorUsuario)
            .WithMany()
            .HasForeignKey(o => o.AtendidaPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilObservaciones_Usuarios_AtendidaPor");
    }
}
