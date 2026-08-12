using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSysStep154NotifScanExportIp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                schema: "erp_sys",
                table: "ImportExportJobs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultContent",
                schema: "erp_sys",
                table: "ImportExportJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultContentType",
                schema: "erp_sys",
                table: "ImportExportJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultFileName",
                schema: "erp_sys",
                table: "ImportExportJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScanStatus",
                schema: "erp_sys",
                table: "file_object",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScannedAt",
                schema: "erp_sys",
                table: "file_object",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThreatName",
                schema: "erp_sys",
                table: "file_object",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sys_file_scan_log",
                schema: "erp_sys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScanStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ThreatName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ScannedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScannedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_file_scan_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sys_ip_rule",
                schema: "erp_sys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IpAddressOrCidr = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_ip_rule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sys_user_notification_preference",
                schema: "erp_sys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelInApp = table.Column<bool>(type: "bit", nullable: false),
                    ChannelEmail = table.Column<bool>(type: "bit", nullable: false),
                    ChannelSms = table.Column<bool>(type: "bit", nullable: false),
                    ChannelPush = table.Column<bool>(type: "bit", nullable: false),
                    MuteAll = table.Column<bool>(type: "bit", nullable: false),
                    QuietHoursStart = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    QuietHoursEnd = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user_notification_preference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sys_file_scan_log_TenantId_FileObjectId_ScannedAt",
                schema: "erp_sys",
                table: "sys_file_scan_log",
                columns: new[] { "TenantId", "FileObjectId", "ScannedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_ip_rule_TenantId_IpAddressOrCidr_RuleType",
                schema: "erp_sys",
                table: "sys_ip_rule",
                columns: new[] { "TenantId", "IpAddressOrCidr", "RuleType" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_notification_preference_TenantId_UserId",
                schema: "erp_sys",
                table: "sys_user_notification_preference",
                columns: new[] { "TenantId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sys_file_scan_log",
                schema: "erp_sys");

            migrationBuilder.DropTable(
                name: "sys_ip_rule",
                schema: "erp_sys");

            migrationBuilder.DropTable(
                name: "sys_user_notification_preference",
                schema: "erp_sys");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                schema: "erp_sys",
                table: "ImportExportJobs");

            migrationBuilder.DropColumn(
                name: "ResultContent",
                schema: "erp_sys",
                table: "ImportExportJobs");

            migrationBuilder.DropColumn(
                name: "ResultContentType",
                schema: "erp_sys",
                table: "ImportExportJobs");

            migrationBuilder.DropColumn(
                name: "ResultFileName",
                schema: "erp_sys",
                table: "ImportExportJobs");

            migrationBuilder.DropColumn(
                name: "ScanStatus",
                schema: "erp_sys",
                table: "file_object");

            migrationBuilder.DropColumn(
                name: "ScannedAt",
                schema: "erp_sys",
                table: "file_object");

            migrationBuilder.DropColumn(
                name: "ThreatName",
                schema: "erp_sys",
                table: "file_object");
        }
    }
}
