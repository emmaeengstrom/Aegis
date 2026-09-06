using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aegis.Api.Migrations
{
    /// <inheritdoc />
    public partial class PreserveAuditLogsOnProjectDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Projects_ProjectId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "AuditLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Projects_ProjectId",
                table: "AuditLogs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Projects_ProjectId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Projects_ProjectId",
                table: "AuditLogs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
