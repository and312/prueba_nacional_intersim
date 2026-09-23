using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class SolicitudComentarioConfiguration : IEntityTypeConfiguration<SolicitudComentario>
{
    public void Configure(EntityTypeBuilder<SolicitudComentario> builder)
    {
        builder.ToTable("SolicitudComentarios");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Id)
            .HasColumnName("ComentarioId")
            .ValueGeneratedOnAdd();

        builder.Property(sc => sc.SolicitudId)
            .IsRequired();

        builder.Property(sc => sc.UsuarioId)
            .IsRequired();

        builder.Property(sc => sc.Texto)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(sc => sc.Fecha)
            .IsRequired();

        builder.Property(sc => sc.EstadoAsociado)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(sc => sc.Iteracion)
            .IsRequired(false);

        builder.Property(sc => sc.TipoComentario)
            .HasMaxLength(50)
            .IsRequired(false);

        // Relationships
        builder.HasOne(sc => sc.Solicitud)
            .WithMany(s => s.Comentarios)
            .HasForeignKey(sc => sc.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SolicitudComentarios_Solicitudes");

        builder.HasOne(sc => sc.Usuario)
            .WithMany()
            .HasForeignKey(sc => sc.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_SolicitudComentarios_Usuarios");
    }
}
