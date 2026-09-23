using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchingResultadoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchingResultados",
                columns: table => new
                {
                    MatchingResultadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchingEjecucionId = table.Column<int>(type: "int", nullable: false),
                    PostulanteId = table.Column<int>(type: "int", nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PorcentajeMatching = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Clasificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PosicionRanking = table.Column<int>(type: "int", nullable: false),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntajeMaximo = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FortalezasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrechasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DesgloseCriteriosJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CvDocumentoId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingResultados", x => x.MatchingResultadoId);
                    table.ForeignKey(
                        name: "FK_MatchingResultados_MatchingEjecuciones",
                        column: x => x.MatchingEjecucionId,
                        principalTable: "MatchingEjecuciones",
                        principalColumn: "MatchingEjecucionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchingResultados_SolicitudDocumentos",
                        column: x => x.CvDocumentoId,
                        principalTable: "SolicitudDocumentos",
                        principalColumn: "DocumentoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchingResultados_CvDocumentoId",
                table: "MatchingResultados",
                column: "CvDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingResultados_MatchingEjecucionId",
                table: "MatchingResultados",
                column: "MatchingEjecucionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchingResultados");
        }
    }
}
