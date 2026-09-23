using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakePerfilCargoIdUniqueInMatchingEjecuciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchingEjecuciones_PerfilCargoId",
                table: "MatchingEjecuciones");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingEjecuciones_PerfilCargoId",
                table: "MatchingEjecuciones",
                column: "PerfilCargoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchingEjecuciones_PerfilCargoId",
                table: "MatchingEjecuciones");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingEjecuciones_PerfilCargoId",
                table: "MatchingEjecuciones",
                column: "PerfilCargoId");
        }
    }
}
