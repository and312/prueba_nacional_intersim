using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchingEjecucionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchingEjecuciones",
                columns: table => new
                {
                    MatchingEjecucionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoMatching = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "LOWER(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''))"),
                    PerfilCargoId = table.Column<int>(type: "int", nullable: false),
                    PerfilEstructuradoId = table.Column<int>(type: "int", nullable: false),
                    VersionPerfil = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalEvaluados = table.Column<int>(type: "int", nullable: false),
                    TotalInternosEvaluados = table.Column<int>(type: "int", nullable: false),
                    TotalHistoricosEvaluados = table.Column<int>(type: "int", nullable: false),
                    TotalMatchAlto = table.Column<int>(type: "int", nullable: false),
                    TotalMatchMedio = table.Column<int>(type: "int", nullable: false),
                    TotalMatchBajo = table.Column<int>(type: "int", nullable: false),
                    TotalDescartados = table.Column<int>(type: "int", nullable: false),
                    TotalPotenciales = table.Column<int>(type: "int", nullable: false),
                    CompatibilidadPromedio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    EstrategiaRecomendada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NivelConfianza = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Justificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuentesConsultadasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResumenDescartesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParametrosMatchingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingEjecuciones", x => x.MatchingEjecucionId);
                    table.ForeignKey(
                        name: "FK_MatchingEjecuciones_PerfilEstructurado",
                        column: x => x.PerfilEstructuradoId,
                        principalTable: "PerfilEstructurado",
                        principalColumn: "PerfilEstructuradoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchingEjecuciones_PerfilesCargo",
                        column: x => x.PerfilCargoId,
                        principalTable: "PerfilesCargo",
                        principalColumn: "PerfilCargoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchingEjecuciones_CodigoMatching",
                table: "MatchingEjecuciones",
                column: "CodigoMatching",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchingEjecuciones_PerfilCargoId",
                table: "MatchingEjecuciones",
                column: "PerfilCargoId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingEjecuciones_PerfilEstructuradoId",
                table: "MatchingEjecuciones",
                column: "PerfilEstructuradoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchingEjecuciones");
        }
    }
}
