using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queryz.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovedTablesToNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_ReportGroupRoles_Queryz_ReportGroups_ReportGroupId",
                table: "Queryz_ReportGroupRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_Reports_Queryz_DataSources_DataSourceId",
                table: "Queryz_Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_Reports_Queryz_ReportGroups_GroupId",
                table: "Queryz_Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_ReportSortings_Queryz_Reports_ReportId",
                table: "Queryz_ReportSortings");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_ReportTableColumns_Queryz_Enumerations_EnumerationId",
                table: "Queryz_ReportTableColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_ReportTableColumns_Queryz_Reports_ReportId",
                table: "Queryz_ReportTableColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_ReportTables_Queryz_Reports_ReportId",
                table: "Queryz_ReportTables");

            migrationBuilder.DropForeignKey(
                name: "FK_Queryz_ReportUserBlacklist_Queryz_Reports_ReportId",
                table: "Queryz_ReportUserBlacklist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_ReportUserBlacklist",
                table: "Queryz_ReportUserBlacklist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_ReportTables",
                table: "Queryz_ReportTables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_ReportTableColumns",
                table: "Queryz_ReportTableColumns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_ReportSortings",
                table: "Queryz_ReportSortings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_Reports",
                table: "Queryz_Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_ReportGroups",
                table: "Queryz_ReportGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_ReportGroupRoles",
                table: "Queryz_ReportGroupRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_Enumerations",
                table: "Queryz_Enumerations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queryz_DataSources",
                table: "Queryz_DataSources");

            migrationBuilder.EnsureSchema(
                name: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_ReportUserBlacklist",
                newName: "ReportUserBlacklist",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_ReportTables",
                newName: "ReportTables",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_ReportTableColumns",
                newName: "ReportTableColumns",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_ReportSortings",
                newName: "ReportSortings",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_Reports",
                newName: "Reports",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_ReportGroups",
                newName: "ReportGroups",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_ReportGroupRoles",
                newName: "ReportGroupRoles",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_Enumerations",
                newName: "Enumerations",
                newSchema: "qryz");

            migrationBuilder.RenameTable(
                name: "Queryz_DataSources",
                newName: "DataSources",
                newSchema: "qryz");

            migrationBuilder.RenameIndex(
                name: "IX_Queryz_ReportTables_ReportId",
                schema: "qryz",
                table: "ReportTables",
                newName: "IX_ReportTables_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_Queryz_ReportTableColumns_ReportId",
                schema: "qryz",
                table: "ReportTableColumns",
                newName: "IX_ReportTableColumns_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_Queryz_ReportTableColumns_EnumerationId",
                schema: "qryz",
                table: "ReportTableColumns",
                newName: "IX_ReportTableColumns_EnumerationId");

            migrationBuilder.RenameIndex(
                name: "IX_Queryz_ReportSortings_ReportId",
                schema: "qryz",
                table: "ReportSortings",
                newName: "IX_ReportSortings_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_Queryz_Reports_GroupId",
                schema: "qryz",
                table: "Reports",
                newName: "IX_Reports_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Queryz_Reports_DataSourceId",
                schema: "qryz",
                table: "Reports",
                newName: "IX_Reports_DataSourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportUserBlacklist",
                schema: "qryz",
                table: "ReportUserBlacklist",
                columns: new[] { "ReportId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportTables",
                schema: "qryz",
                table: "ReportTables",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportTableColumns",
                schema: "qryz",
                table: "ReportTableColumns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportSortings",
                schema: "qryz",
                table: "ReportSortings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                schema: "qryz",
                table: "Reports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportGroups",
                schema: "qryz",
                table: "ReportGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportGroupRoles",
                schema: "qryz",
                table: "ReportGroupRoles",
                columns: new[] { "ReportGroupId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enumerations",
                schema: "qryz",
                table: "Enumerations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataSources",
                schema: "qryz",
                table: "DataSources",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportGroupRoles_ReportGroups_ReportGroupId",
                schema: "qryz",
                table: "ReportGroupRoles",
                column: "ReportGroupId",
                principalSchema: "qryz",
                principalTable: "ReportGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_DataSources_DataSourceId",
                schema: "qryz",
                table: "Reports",
                column: "DataSourceId",
                principalSchema: "qryz",
                principalTable: "DataSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportGroups_GroupId",
                schema: "qryz",
                table: "Reports",
                column: "GroupId",
                principalSchema: "qryz",
                principalTable: "ReportGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportSortings_Reports_ReportId",
                schema: "qryz",
                table: "ReportSortings",
                column: "ReportId",
                principalSchema: "qryz",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportTableColumns_Enumerations_EnumerationId",
                schema: "qryz",
                table: "ReportTableColumns",
                column: "EnumerationId",
                principalSchema: "qryz",
                principalTable: "Enumerations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportTableColumns_Reports_ReportId",
                schema: "qryz",
                table: "ReportTableColumns",
                column: "ReportId",
                principalSchema: "qryz",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportTables_Reports_ReportId",
                schema: "qryz",
                table: "ReportTables",
                column: "ReportId",
                principalSchema: "qryz",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportUserBlacklist_Reports_ReportId",
                schema: "qryz",
                table: "ReportUserBlacklist",
                column: "ReportId",
                principalSchema: "qryz",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportGroupRoles_ReportGroups_ReportGroupId",
                schema: "qryz",
                table: "ReportGroupRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_DataSources_DataSourceId",
                schema: "qryz",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportGroups_GroupId",
                schema: "qryz",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportSortings_Reports_ReportId",
                schema: "qryz",
                table: "ReportSortings");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportTableColumns_Enumerations_EnumerationId",
                schema: "qryz",
                table: "ReportTableColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportTableColumns_Reports_ReportId",
                schema: "qryz",
                table: "ReportTableColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportTables_Reports_ReportId",
                schema: "qryz",
                table: "ReportTables");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportUserBlacklist_Reports_ReportId",
                schema: "qryz",
                table: "ReportUserBlacklist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportUserBlacklist",
                schema: "qryz",
                table: "ReportUserBlacklist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportTables",
                schema: "qryz",
                table: "ReportTables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportTableColumns",
                schema: "qryz",
                table: "ReportTableColumns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportSortings",
                schema: "qryz",
                table: "ReportSortings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                schema: "qryz",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportGroups",
                schema: "qryz",
                table: "ReportGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportGroupRoles",
                schema: "qryz",
                table: "ReportGroupRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enumerations",
                schema: "qryz",
                table: "Enumerations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataSources",
                schema: "qryz",
                table: "DataSources");

            migrationBuilder.RenameTable(
                name: "ReportUserBlacklist",
                schema: "qryz",
                newName: "Queryz_ReportUserBlacklist");

            migrationBuilder.RenameTable(
                name: "ReportTables",
                schema: "qryz",
                newName: "Queryz_ReportTables");

            migrationBuilder.RenameTable(
                name: "ReportTableColumns",
                schema: "qryz",
                newName: "Queryz_ReportTableColumns");

            migrationBuilder.RenameTable(
                name: "ReportSortings",
                schema: "qryz",
                newName: "Queryz_ReportSortings");

            migrationBuilder.RenameTable(
                name: "Reports",
                schema: "qryz",
                newName: "Queryz_Reports");

            migrationBuilder.RenameTable(
                name: "ReportGroups",
                schema: "qryz",
                newName: "Queryz_ReportGroups");

            migrationBuilder.RenameTable(
                name: "ReportGroupRoles",
                schema: "qryz",
                newName: "Queryz_ReportGroupRoles");

            migrationBuilder.RenameTable(
                name: "Enumerations",
                schema: "qryz",
                newName: "Queryz_Enumerations");

            migrationBuilder.RenameTable(
                name: "DataSources",
                schema: "qryz",
                newName: "Queryz_DataSources");

            migrationBuilder.RenameIndex(
                name: "IX_ReportTables_ReportId",
                table: "Queryz_ReportTables",
                newName: "IX_Queryz_ReportTables_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportTableColumns_ReportId",
                table: "Queryz_ReportTableColumns",
                newName: "IX_Queryz_ReportTableColumns_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportTableColumns_EnumerationId",
                table: "Queryz_ReportTableColumns",
                newName: "IX_Queryz_ReportTableColumns_EnumerationId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportSortings_ReportId",
                table: "Queryz_ReportSortings",
                newName: "IX_Queryz_ReportSortings_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_GroupId",
                table: "Queryz_Reports",
                newName: "IX_Queryz_Reports_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_DataSourceId",
                table: "Queryz_Reports",
                newName: "IX_Queryz_Reports_DataSourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_ReportUserBlacklist",
                table: "Queryz_ReportUserBlacklist",
                columns: new[] { "ReportId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_ReportTables",
                table: "Queryz_ReportTables",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_ReportTableColumns",
                table: "Queryz_ReportTableColumns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_ReportSortings",
                table: "Queryz_ReportSortings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_Reports",
                table: "Queryz_Reports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_ReportGroups",
                table: "Queryz_ReportGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_ReportGroupRoles",
                table: "Queryz_ReportGroupRoles",
                columns: new[] { "ReportGroupId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_Enumerations",
                table: "Queryz_Enumerations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queryz_DataSources",
                table: "Queryz_DataSources",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_ReportGroupRoles_Queryz_ReportGroups_ReportGroupId",
                table: "Queryz_ReportGroupRoles",
                column: "ReportGroupId",
                principalTable: "Queryz_ReportGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_Reports_Queryz_DataSources_DataSourceId",
                table: "Queryz_Reports",
                column: "DataSourceId",
                principalTable: "Queryz_DataSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_Reports_Queryz_ReportGroups_GroupId",
                table: "Queryz_Reports",
                column: "GroupId",
                principalTable: "Queryz_ReportGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_ReportSortings_Queryz_Reports_ReportId",
                table: "Queryz_ReportSortings",
                column: "ReportId",
                principalTable: "Queryz_Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_ReportTableColumns_Queryz_Enumerations_EnumerationId",
                table: "Queryz_ReportTableColumns",
                column: "EnumerationId",
                principalTable: "Queryz_Enumerations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_ReportTableColumns_Queryz_Reports_ReportId",
                table: "Queryz_ReportTableColumns",
                column: "ReportId",
                principalTable: "Queryz_Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_ReportTables_Queryz_Reports_ReportId",
                table: "Queryz_ReportTables",
                column: "ReportId",
                principalTable: "Queryz_Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Queryz_ReportUserBlacklist_Queryz_Reports_ReportId",
                table: "Queryz_ReportUserBlacklist",
                column: "ReportId",
                principalTable: "Queryz_Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
