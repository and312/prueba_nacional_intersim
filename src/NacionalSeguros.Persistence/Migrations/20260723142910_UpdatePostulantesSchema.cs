using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostulantesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "PostulantesExternos");

            migrationBuilder.AddColumn<string>(
                name: "Certificaciones",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostulanteInterno",
                table: "PostulantesInternos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "LOWER(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''))");

            migrationBuilder.AddColumn<string>(
                name: "ConocimientosTecnicosJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CursosComplementarios",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisponibleCambioRegional",
                table: "PostulantesInternos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "no");

            migrationBuilder.AddColumn<string>(
                name: "ExperienciaRelevanteAnios",
                table: "PostulantesInternos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienciaTotalAnios",
                table: "PostulantesInternos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FuncionesActualesJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HardSkillsJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HerramientasSistemasJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Idiomas",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogrosRelevantesJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectoresExperienciaJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeniorityActual",
                table: "PostulantesInternos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoftSkillsJson",
                table: "PostulantesInternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Certificaciones",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostulanteExterno",
                table: "PostulantesExternos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "LOWER(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''))");

            migrationBuilder.AddColumn<string>(
                name: "ConocimientosTecnicos",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CursosComplementarios",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienciaLiderazgoAnios",
                table: "PostulantesExternos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienciaTotalAnios",
                table: "PostulantesExternos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FuncionesRelevantes",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HardSkills",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HerramientasSistemas",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Idiomas",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogrosRelevantes",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivacionPostulacion",
                table: "PostulantesExternos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectoresExperiencia",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Seniority",
                table: "PostulantesExternos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoftSkills",
                table: "PostulantesExternos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostulantesInternos_CodigoPostulanteInterno",
                table: "PostulantesInternos",
                column: "CodigoPostulanteInterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostulantesExternos_CodigoPostulanteExterno",
                table: "PostulantesExternos",
                column: "CodigoPostulanteExterno",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostulantesInternos_CodigoPostulanteInterno",
                table: "PostulantesInternos");

            migrationBuilder.DropIndex(
                name: "IX_PostulantesExternos_CodigoPostulanteExterno",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "Certificaciones",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "CodigoPostulanteInterno",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "ConocimientosTecnicosJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "CursosComplementarios",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "DisponibleCambioRegional",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "ExperienciaRelevanteAnios",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "ExperienciaTotalAnios",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "FuncionesActualesJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "HardSkillsJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "HerramientasSistemasJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "Idiomas",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "LogrosRelevantesJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "SectoresExperienciaJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "SeniorityActual",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "SoftSkillsJson",
                table: "PostulantesInternos");

            migrationBuilder.DropColumn(
                name: "Certificaciones",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "CodigoPostulanteExterno",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "ConocimientosTecnicos",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "CursosComplementarios",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "ExperienciaLiderazgoAnios",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "ExperienciaTotalAnios",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "FuncionesRelevantes",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "HardSkills",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "HerramientasSistemas",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "Idiomas",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "LogrosRelevantes",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "MotivacionPostulacion",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "SectoresExperiencia",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "Seniority",
                table: "PostulantesExternos");

            migrationBuilder.DropColumn(
                name: "SoftSkills",
                table: "PostulantesExternos");

            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "PostulantesInternos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "PostulantesExternos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
