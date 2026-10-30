using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queryz.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddReportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportTables",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", unicode: false, maxLength: 255, nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTables_ReportId_Ordinal",
                schema: "app",
                table: "ReportTables",
                columns: new[] { "ReportId", "Ordinal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportTables",
                schema: "app");
        }
    }
}
