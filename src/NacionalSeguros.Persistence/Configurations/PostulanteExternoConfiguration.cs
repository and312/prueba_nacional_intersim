using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Persistence.Configurations;

public class PostulanteExternoConfiguration : IEntityTypeConfiguration<PostulanteExterno>
{
    public void Configure(EntityTypeBuilder<PostulanteExterno> builder)
    {
        builder.ToTable("PostulantesExternos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PostulanteExternoId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.PerfilCargoId)
            .IsRequired();

        // Código autogenerado único
        builder.Property(p => p.CodigoPostulanteExterno)
            .HasColumnName("CodigoPostulanteExterno")
            .HasMaxLength(50)
            .HasDefaultValueSql("LOWER(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''))")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasIndex(p => p.CodigoPostulanteExterno)
            .IsUnique();

        builder.Property(p => p.NombreCargoPostulado)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(p => p.FechaPostulacion)
            .IsRequired();

        builder.Property(p => p.PretensionSalarialBs)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PretensionNegociable)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.DisponibilidadIncorporacion)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(p => p.MotivacionPostulacion)
            .HasColumnName("MotivacionPostulacion")
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(p => p.NombresApellidos)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Edad)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.CiudadResidencia)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Direccion)
            .HasMaxLength(300)
            .IsRequired(false);

        builder.Property(p => p.NumeroCelular)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.CorreoElectronico)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.CiIdentidad)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.EstadoCivil)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.NumeroHijos)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.Colegio)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(p => p.CarreraInstitucionUniversitaria)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(p => p.EstadoAcademico)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.PostgradoInstitucion)
            .IsRequired(false);

        builder.Property(p => p.MaestriaInstitucion)
            .IsRequired(false);

        builder.Property(p => p.ExperienciasLaborales)
            .IsRequired(false);

        // Nuevas Columnas Profesionales Externas
        builder.Property(p => p.Seniority)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.Certificaciones)
            .IsRequired(false);

        builder.Property(p => p.CursosComplementarios)
            .IsRequired(false);

        builder.Property(p => p.Idiomas)
            .IsRequired(false);

        builder.Property(p => p.ExperienciaTotalAnios)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.ExperienciaLiderazgoAnios)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.SectoresExperiencia)
            .IsRequired(false);

        builder.Property(p => p.HardSkills)
            .IsRequired(false);

        builder.Property(p => p.SoftSkills)
            .IsRequired(false);

        builder.Property(p => p.HerramientasSistemas)
            .IsRequired(false);

        builder.Property(p => p.ConocimientosTecnicos)
            .IsRequired(false);

        builder.Property(p => p.FuncionesRelevantes)
            .IsRequired(false);

        builder.Property(p => p.LogrosRelevantes)
            .IsRequired(false);

        // Audit properties
        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.ModifiedDate)
            .IsRequired(false);

        // Relationship
        builder.HasOne(p => p.PerfilCargo)
            .WithMany()
            .HasForeignKey(p => p.PerfilCargoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PostulantesExternos_PerfilesCargo");

        // Query filter for Soft Delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
