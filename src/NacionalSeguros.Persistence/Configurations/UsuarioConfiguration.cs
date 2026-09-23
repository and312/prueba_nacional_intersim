using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Enums;

namespace NacionalSeguros.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Id)
            .HasColumnName("UsuarioId")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Correo)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Correo)
            .IsUnique()
            .HasDatabaseName("UQ_Usuarios_Correo");

        builder.Property(u => u.ClaveHash)
            .HasMaxLength(256);

        builder.Property(u => u.TipoAutenticacion)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion(
                v => v.ToString(),
                v => (TipoAutenticacion)System.Enum.Parse(typeof(TipoAutenticacion), v));

        builder.Property(u => u.ActiveDirectoryId)
            .HasMaxLength(100);

        builder.Property(u => u.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (UsuarioEstado)System.Enum.Parse(typeof(UsuarioEstado), v));

        builder.Property(u => u.MfaHabilitado)
            .IsRequired();

        builder.Property(u => u.MfaSecreto)
            .HasMaxLength(128);

        // Relación Muchos a Muchos con Roles (vía tabla UsuarioRoles)
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Usuarios)
            .UsingEntity<Dictionary<string, object>>(
                "UsuarioRoles",
                r => r.HasOne<Rol>().WithMany().HasForeignKey("RolId"),
                l => l.HasOne<Usuario>().WithMany().HasForeignKey("UsuarioId"),
                je =>
                {
                    je.HasKey("UsuarioId", "RolId");
                    je.ToTable("UsuarioRoles");
                });

        // Relación Uno a Muchos con Sesiones
        builder.HasMany(u => u.Sesiones)
            .WithOne(s => s.Usuario)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Módulo 01 - Nuevas propiedades y relaciones
        builder.Property(u => u.Nombres)
            .HasMaxLength(100);

        builder.Property(u => u.Apellidos)
            .HasMaxLength(100);

        builder.Property(u => u.Cargo)
            .HasMaxLength(100)
            .HasColumnName("Cargo");

        builder.Property(u => u.Gerencia)
            .HasMaxLength(100);

        builder.Property(u => u.Telefono)
            .HasMaxLength(20);

        builder.Property(u => u.Extension)
            .HasMaxLength(10);

        builder.Property(u => u.Observaciones)
            .HasMaxLength(500);

        builder.Property(u => u.FotografiaUrl)
            .HasMaxLength(250);

        builder.Property(u => u.ResetPasswordToken)
            .HasMaxLength(200);

        builder.Property(u => u.ResetPasswordTokenExpiration);

        builder.HasOne(u => u.Area)
            .WithMany(a => a.Usuarios)
            .HasForeignKey(u => u.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft Delete Query Filter
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
