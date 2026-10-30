using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Queryz.Data.Npgsql.Migrations
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
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                schema: "app",
                table: "ReportGroups",
                type: "character varying(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EnumerationId",
                schema: "app",
                table: "ReportColumns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransformFunction",
                schema: "app",
                table: "ReportColumns",
                type: "character varying(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Enumerations",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Values = table.Column<string>(type: "text", nullable: false),
                    IsBitFlags = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportGroupId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
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
