using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMatchingEjecucionEstadoToEstadoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Seed Estados
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Estados ON;
                IF NOT EXISTS (SELECT 1 FROM Estados WHERE EstadoId = 37)
                    INSERT INTO Estados (EstadoId, Codigo, Nombre, Entidad) VALUES (37, 'MATCH-PEN', 'Pendiente de Generación', 'Solicitud');
                IF NOT EXISTS (SELECT 1 FROM Estados WHERE EstadoId = 38)
                    INSERT INTO Estados (EstadoId, Codigo, Nombre, Entidad) VALUES (38, 'MATCH-REV', 'En revisión RRHH', 'Solicitud');
                IF NOT EXISTS (SELECT 1 FROM Estados WHERE EstadoId = 39)
                    INSERT INTO Estados (EstadoId, Codigo, Nombre, Entidad) VALUES (39, 'MATCH-ARP', 'Estrategia Aprobada', 'Solicitud');
                SET IDENTITY_INSERT Estados OFF;
            ");

            // 2. Add column as nullable first
            migrationBuilder.AddColumn<int>(
                name: "EstadoId",
                table: "MatchingEjecuciones",
                type: "int",
                nullable: true);

            // 3. Migrate data from old string column
            migrationBuilder.Sql(@"
                UPDATE MatchingEjecuciones SET EstadoId = 39 WHERE Estado = 'Completado';
                UPDATE MatchingEjecuciones SET EstadoId = 37 WHERE EstadoId IS NULL;
            ");

            // 4. Alter column to be non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "EstadoId",
                table: "MatchingEjecuciones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 5. Drop old column
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "MatchingEjecuciones");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingEjecuciones_EstadoId",
                table: "MatchingEjecuciones",
                column: "EstadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchingEjecuciones_Estados",
                table: "MatchingEjecuciones",
                column: "EstadoId",
                principalTable: "Estados",
                principalColumn: "EstadoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchingEjecuciones_Estados",
                table: "MatchingEjecuciones");

            migrationBuilder.DropIndex(
                name: "IX_MatchingEjecuciones_EstadoId",
                table: "MatchingEjecuciones");

            migrationBuilder.DropColumn(
                name: "EstadoId",
                table: "MatchingEjecuciones");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "MatchingEjecuciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
