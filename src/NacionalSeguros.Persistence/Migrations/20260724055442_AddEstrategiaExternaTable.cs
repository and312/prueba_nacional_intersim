using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstrategiaExternaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstrategiaExternas",
                columns: table => new
                {
                    EstrategiaExternaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchingEjecucionId = table.Column<int>(type: "int", nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Justificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriteriosDificiles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicoObjetivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanalesSugeridos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanAccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BriefingEditable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conclusion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstrategiaExternas", x => x.EstrategiaExternaId);
                    table.ForeignKey(
                        name: "FK_EstrategiaExternas_Estados",
                        column: x => x.EstadoId,
                        principalTable: "Estados",
                        principalColumn: "EstadoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstrategiaExternas_MatchingEjecuciones",
                        column: x => x.MatchingEjecucionId,
                        principalTable: "MatchingEjecuciones",
                        principalColumn: "MatchingEjecucionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstrategiaExternas_EstadoId",
                table: "EstrategiaExternas",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstrategiaExternas_MatchingEjecucionId",
                table: "EstrategiaExternas",
                column: "MatchingEjecucionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstrategiaExternas");
        }
    }
}
