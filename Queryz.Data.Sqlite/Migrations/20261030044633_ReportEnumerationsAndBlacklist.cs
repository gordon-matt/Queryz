using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queryz.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ReportEnumerationsAndBlacklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "EnumerationHandling",
                schema: "app",
                table: "Reports",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                schema: "app",
                table: "ReportGroups",
                type: "TEXT",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EnumerationId",
                schema: "app",
                table: "ReportColumns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransformFunction",
                schema: "app",
                table: "ReportColumns",
                type: "TEXT",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Enumerations",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Values = table.Column<string>(type: "TEXT", nullable: false),
                    IsBitFlags = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enumerations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportGroupRoles",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportGroupRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportUserBlacklist",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportUserBlacklist", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enumerations_Name",
                schema: "app",
                table: "Enumerations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportGroupRoles_ReportGroupId_RoleId",
                schema: "app",
                table: "ReportGroupRoles",
                columns: new[] { "ReportGroupId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportUserBlacklist_ReportId_UserId",
                schema: "app",
                table: "ReportUserBlacklist",
                columns: new[] { "ReportId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enumerations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ReportGroupRoles",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ReportUserBlacklist",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "EnumerationHandling",
                schema: "app",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                schema: "app",
                table: "ReportGroups");

            migrationBuilder.DropColumn(
                name: "EnumerationId",
                schema: "app",
                table: "ReportColumns");

            migrationBuilder.DropColumn(
                name: "TransformFunction",
                schema: "app",
                table: "ReportColumns");
        }
    }
}
