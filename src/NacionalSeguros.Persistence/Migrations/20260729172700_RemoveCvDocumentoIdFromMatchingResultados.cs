using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCvDocumentoIdFromMatchingResultados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchingResultados_SolicitudDocumentos",
                table: "MatchingResultados");

            migrationBuilder.DropIndex(
                name: "IX_MatchingResultados_CvDocumentoId",
                table: "MatchingResultados");

            migrationBuilder.DropColumn(
                name: "CvDocumentoId",
                table: "MatchingResultados");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CvDocumentoId",
                table: "MatchingResultados",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchingResultados_CvDocumentoId",
                table: "MatchingResultados",
                column: "CvDocumentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchingResultados_SolicitudDocumentos",
                table: "MatchingResultados",
                column: "CvDocumentoId",
                principalTable: "SolicitudDocumentos",
                principalColumn: "DocumentoId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
