using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Queryz.Data.Migrations
{
    public partial class AddReportBuilderTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Queryz_DataSources",
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
                    table.PrimaryKey("PK_Queryz_DataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_Enumerations",
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
                    table.PrimaryKey("PK_Queryz_Enumerations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_ReportGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TimeZoneId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queryz_ReportGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_ReportGroupRoles",
                columns: table => new
                {
                    ReportGroupId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queryz_ReportGroupRoles", x => new { x.ReportGroupId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Queryz_ReportGroupRoles_Queryz_ReportGroups_ReportGroupId",
                        column: x => x.ReportGroupId,
                        principalTable: "Queryz_ReportGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_Reports",
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
                    table.PrimaryKey("PK_Queryz_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queryz_Reports_Queryz_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "Queryz_DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Queryz_Reports_Queryz_ReportGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Queryz_ReportGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_ReportSortings",
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
                    table.PrimaryKey("PK_Queryz_ReportSortings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queryz_ReportSortings_Queryz_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Queryz_Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_ReportTableColumns",
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
                    table.PrimaryKey("PK_Queryz_ReportTableColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queryz_ReportTableColumns_Queryz_Enumerations_EnumerationId",
                        column: x => x.EnumerationId,
                        principalTable: "Queryz_Enumerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Queryz_ReportTableColumns_Queryz_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Queryz_Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_ReportTables",
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
                    table.PrimaryKey("PK_Queryz_ReportTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queryz_ReportTables_Queryz_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Queryz_Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queryz_ReportUserBlacklist",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queryz_ReportUserBlacklist", x => new { x.ReportId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Queryz_ReportUserBlacklist_Queryz_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Queryz_Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoleApplicationUser_UsersId",
                table: "ApplicationRoleApplicationUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Queryz_Reports_DataSourceId",
                table: "Queryz_Reports",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Queryz_Reports_GroupId",
                table: "Queryz_Reports",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Queryz_ReportSortings_ReportId",
                table: "Queryz_ReportSortings",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Queryz_ReportTableColumns_EnumerationId",
                table: "Queryz_ReportTableColumns",
                column: "EnumerationId");

            migrationBuilder.CreateIndex(
                name: "IX_Queryz_ReportTableColumns_ReportId",
                table: "Queryz_ReportTableColumns",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Queryz_ReportTables_ReportId",
                table: "Queryz_ReportTables",
                column: "ReportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationRoleApplicationUser");

            migrationBuilder.DropTable(
                name: "Queryz_ReportGroupRoles");

            migrationBuilder.DropTable(
                name: "Queryz_ReportSortings");

            migrationBuilder.DropTable(
                name: "Queryz_ReportTableColumns");

            migrationBuilder.DropTable(
                name: "Queryz_ReportTables");

            migrationBuilder.DropTable(
                name: "Queryz_ReportUserBlacklist");

            migrationBuilder.DropTable(
                name: "Queryz_Enumerations");

            migrationBuilder.DropTable(
                name: "Queryz_Reports");

            migrationBuilder.DropTable(
                name: "Queryz_DataSources");

            migrationBuilder.DropTable(
                name: "Queryz_ReportGroups");

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
