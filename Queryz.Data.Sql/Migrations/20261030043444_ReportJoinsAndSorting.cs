using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queryz.Data.Sql.Migrations
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
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "JoinType",
                schema: "app",
                table: "ReportTables",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "ParentTable",
                schema: "app",
                table: "ReportTables",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryKeyColumn",
                schema: "app",
                table: "ReportTables",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailEnabled",
                schema: "app",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                schema: "app",
                table: "ReportColumns",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Format",
                schema: "app",
                table: "ReportColumns",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                schema: "app",
                table: "ReportColumns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiteral",
                schema: "app",
                table: "ReportColumns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ReportSortings",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ColumnName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    SortDirection = table.Column<byte>(type: "tinyint", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false)
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
