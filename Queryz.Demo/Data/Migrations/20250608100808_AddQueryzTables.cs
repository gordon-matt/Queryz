using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queryz.Demo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryzTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "qryz");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "ApplicationRoleApplicationUser",
                columns: table => new
                {
                    RolesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoleApplicationUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationRoleApplicationUser_AspNetRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationRoleApplicationUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSources",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    DataProvider = table.Column<byte>(type: "tinyint", nullable: false),
                    ConnectionString = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CustomProperties = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enumerations",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Values = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBitFlags = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enumerations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportGroups",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TimeZoneId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportGroupRoles",
                schema: "qryz",
                columns: table => new
                {
                    ReportGroupId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportGroupRoles", x => new { x.ReportGroupId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ReportGroupRoles_ReportGroups_ReportGroupId",
                        column: x => x.ReportGroupId,
                        principalSchema: "qryz",
                        principalTable: "ReportGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DataSourceId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDistinct = table.Column<bool>(type: "bit", nullable: false),
                    RowLimit = table.Column<int>(type: "int", nullable: true),
                    Filters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnumerationHandling = table.Column<byte>(type: "tinyint", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalSchema: "qryz",
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_ReportGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "qryz",
                        principalTable: "ReportGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportSortings",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ColumnName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    SortDirection = table.Column<byte>(type: "tinyint", nullable: false),
                    Ordinal = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSortings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportSortings_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "qryz",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTableColumns",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    IsLiteral = table.Column<bool>(type: "bit", nullable: false),
                    IsForeignKey = table.Column<bool>(type: "bit", nullable: false),
                    DisplayColumn = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    EnumerationId = table.Column<int>(type: "int", nullable: true),
                    TransformFunction = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    Format = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTableColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTableColumns_Enumerations_EnumerationId",
                        column: x => x.EnumerationId,
                        principalSchema: "qryz",
                        principalTable: "Enumerations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportTableColumns_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "qryz",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTables",
                schema: "qryz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    ParentTable = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    PrimaryKeyColumn = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ForeignKeyColumn = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    JoinType = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTables_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "qryz",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportUserBlacklist",
                schema: "qryz",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportUserBlacklist", x => new { x.ReportId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ReportUserBlacklist_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "qryz",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoleApplicationUser_UsersId",
                table: "ApplicationRoleApplicationUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DataSourceId",
                schema: "qryz",
                table: "Reports",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GroupId",
                schema: "qryz",
                table: "Reports",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSortings_ReportId",
                schema: "qryz",
                table: "ReportSortings",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTableColumns_EnumerationId",
                schema: "qryz",
                table: "ReportTableColumns",
                column: "EnumerationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTableColumns_ReportId",
                schema: "qryz",
                table: "ReportTableColumns",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTables_ReportId",
                schema: "qryz",
                table: "ReportTables",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationRoleApplicationUser");

            migrationBuilder.DropTable(
                name: "ReportGroupRoles",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "ReportSortings",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "ReportTableColumns",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "ReportTables",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "ReportUserBlacklist",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "Enumerations",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "DataSources",
                schema: "qryz");

            migrationBuilder.DropTable(
                name: "ReportGroups",
                schema: "qryz");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
