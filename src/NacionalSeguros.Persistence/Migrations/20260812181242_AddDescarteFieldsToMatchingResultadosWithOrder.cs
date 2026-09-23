using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NacionalSeguros.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDescarteFieldsToMatchingResultadosWithOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add temporary column to preserve CreatedDate
            migrationBuilder.Sql("ALTER TABLE MatchingResultados ADD CreatedDate_Temp datetime2 NULL;");
            
            // 2. Copy data
            migrationBuilder.Sql("UPDATE MatchingResultados SET CreatedDate_Temp = CreatedDate;");
            
            // 3. Drop existing CreatedDate column
            migrationBuilder.Sql("ALTER TABLE MatchingResultados DROP COLUMN CreatedDate;");
            
            // 4. Add TipoDescarte and MotivoExclusion columns
            migrationBuilder.Sql("ALTER TABLE MatchingResultados ADD TipoDescarte nvarchar(50) NULL;");
            migrationBuilder.Sql("ALTER TABLE MatchingResultados ADD MotivoExclusion nvarchar(max) NULL;");
            
            // 5. Add CreatedDate back (not null)
            migrationBuilder.Sql("ALTER TABLE MatchingResultados ADD CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE();");
            
            // 6. Copy data back
            migrationBuilder.Sql("UPDATE MatchingResultados SET CreatedDate = CreatedDate_Temp;");
            
            // 7. Drop temporary column
            migrationBuilder.Sql("ALTER TABLE MatchingResultados DROP COLUMN CreatedDate_Temp;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoExclusion",
                table: "MatchingResultados");

            migrationBuilder.DropColumn(
                name: "TipoDescarte",
                table: "MatchingResultados");
        }
    }
}
