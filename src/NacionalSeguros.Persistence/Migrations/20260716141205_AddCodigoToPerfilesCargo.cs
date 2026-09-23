using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoToPerfilesCargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "PerfilesCargo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Poblar registros existentes con el código generado a partir de Solicitudes.Codigo
            migrationBuilder.Sql(@"
                UPDATE pc
                SET pc.Codigo = 'PRF-' + s.Codigo
                FROM PerfilesCargo pc
                INNER JOIN Solicitudes s ON pc.SolicitudId = s.SolicitudId
                WHERE pc.Codigo IS NULL
            ");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesCargo_Codigo",
                table: "PerfilesCargo",
                column: "Codigo",
                filter: "[Codigo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PerfilesCargo_Codigo",
                table: "PerfilesCargo");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "PerfilesCargo");
        }
    }
}
