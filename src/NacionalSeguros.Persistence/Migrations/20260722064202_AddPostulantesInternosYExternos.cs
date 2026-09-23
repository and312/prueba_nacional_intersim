using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPostulantesInternosYExternos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostulantesExternos",
                columns: table => new
                {
                    PostulanteExternoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoId = table.Column<int>(type: "int", nullable: false),
                    NombreCargoPostulado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaPostulacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PretensionSalarialBs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PretensionNegociable = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DisponibilidadIncorporacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NombresApellidos = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Edad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CiudadResidencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NumeroCelular = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CiIdentidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstadoCivil = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroHijos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Colegio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CarreraInstitucionUniversitaria = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstadoAcademico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostgradoInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaestriaInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienciasLaborales = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CvUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulantesExternos", x => x.PostulanteExternoId);
                    table.ForeignKey(
                        name: "FK_PostulantesExternos_PerfilesCargo",
                        column: x => x.PerfilCargoId,
                        principalTable: "PerfilesCargo",
                        principalColumn: "PerfilCargoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostulantesInternos",
                columns: table => new
                {
                    PostulanteInternoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoId = table.Column<int>(type: "int", nullable: false),
                    NombreCargoPostulado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaPostulacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PretensionSalarialBs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PretensionNegociable = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VinculoGrupoNacional = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DetalleVinculoGrupo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VinculoSectorFinanciero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DisponibilidadIncorporacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NombresApellidos = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Edad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CiudadResidencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NumeroCelular = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CiIdentidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstadoCivil = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroHijos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Colegio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CarreraInstitucionUniversitaria = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstadoAcademico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostgradoInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaestriaInstitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpresaActual = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AreaActualTrabajo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupervisorNombreCargo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CargoActual = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotivacionPostulacion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaIngresoCompania = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CvUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulantesInternos", x => x.PostulanteInternoId);
                    table.ForeignKey(
                        name: "FK_PostulantesInternos_PerfilesCargo",
                        column: x => x.PerfilCargoId,
                        principalTable: "PerfilesCargo",
                        principalColumn: "PerfilCargoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostulantesExternos_PerfilCargoId",
                table: "PostulantesExternos",
                column: "PerfilCargoId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulantesInternos_PerfilCargoId",
                table: "PostulantesInternos",
                column: "PerfilCargoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostulantesExternos");

            migrationBuilder.DropTable(
                name: "PostulantesInternos");
        }
    }
}
