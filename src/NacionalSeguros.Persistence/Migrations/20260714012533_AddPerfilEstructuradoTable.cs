using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfilEstructuradoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerfilEstructurado",
                columns: table => new
                {
                    PerfilEstructuradoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    ObjetivoPrincipalCargo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PerfilIdealCandidato = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PerfilTipoAltoAjuste = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EstadoGeneracion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DatosGeneralesCargo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerfilRequerido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HerramientasSistemas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FiltrosClaveSeleccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConocimientosTecnicosRequeridos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuncionesPrincipalesCargo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompetenciasClave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndicadoresExitoCargo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatrizPonderacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FuentesUtilizadas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alertas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilEstructurado", x => x.PerfilEstructuradoId);
                    table.ForeignKey(
                        name: "FK_PerfilEstructurado_Solicitudes",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "SolicitudId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_PerfilEstructurado_Solicitud",
                table: "PerfilEstructurado",
                column: "SolicitudId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfilEstructurado");
        }
    }
}
