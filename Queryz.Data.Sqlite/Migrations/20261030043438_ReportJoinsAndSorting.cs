using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queryz.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ReportJoinsAndSorting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForeignKeyColumn",
                schema: "app",
                table: "ReportTables",
                type: "TEXT",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "JoinType",
                schema: "app",
                table: "ReportTables",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "ParentTable",
                schema: "app",
                table: "ReportTables",
                type: "TEXT",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryKeyColumn",
                schema: "app",
                table: "ReportTables",
                type: "TEXT",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailEnabled",
                schema: "app",
                table: "Reports",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                schema: "app",
                table: "ReportColumns",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Format",
                schema: "app",
                table: "ReportColumns",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                schema: "app",
                table: "ReportColumns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiteral",
                schema: "app",
                table: "ReportColumns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ReportSortings",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    ColumnName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 255, nullable: false),
                    SortDirection = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSortings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSortings_ReportId_Ordinal",
                schema: "app",
                table: "ReportSortings",
                columns: new[] { "ReportId", "Ordinal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportSortings",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "ForeignKeyColumn",
                schema: "app",
                table: "ReportTables");

            migrationBuilder.DropColumn(
                name: "JoinType",
                schema: "app",
                table: "ReportTables");

            migrationBuilder.DropColumn(
                name: "ParentTable",
                schema: "app",
                table: "ReportTables");

            migrationBuilder.DropColumn(
                name: "PrimaryKeyColumn",
                schema: "app",
                table: "ReportTables");

            migrationBuilder.DropColumn(
                name: "EmailEnabled",
                schema: "app",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Alias",
                schema: "app",
                table: "ReportColumns");

            migrationBuilder.DropColumn(
                name: "Format",
                schema: "app",
                table: "ReportColumns");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                schema: "app",
                table: "ReportColumns");

            migrationBuilder.DropColumn(
                name: "IsLiteral",
                schema: "app",
                table: "ReportColumns");
        }
    }
}
