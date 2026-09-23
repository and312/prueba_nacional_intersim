using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstrategiaInternaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstrategiaInternas",
                columns: table => new
                {
                    EstrategiaInternaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchingEjecucionId = table.Column<int>(type: "int", nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Justificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanAccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanEvaluacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MensajeContingencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conclusion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstrategiaInternas", x => x.EstrategiaInternaId);
                    table.ForeignKey(
                        name: "FK_EstrategiaInternas_Estados",
                        column: x => x.EstadoId,
                        principalTable: "Estados",
                        principalColumn: "EstadoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstrategiaInternas_MatchingEjecuciones",
                        column: x => x.MatchingEjecucionId,
                        principalTable: "MatchingEjecuciones",
                        principalColumn: "MatchingEjecucionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstrategiaInternas_EstadoId",
                table: "EstrategiaInternas",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstrategiaInternas_MatchingEjecucionId",
                table: "EstrategiaInternas",
                column: "MatchingEjecucionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstrategiaInternas");
        }
    }
}
