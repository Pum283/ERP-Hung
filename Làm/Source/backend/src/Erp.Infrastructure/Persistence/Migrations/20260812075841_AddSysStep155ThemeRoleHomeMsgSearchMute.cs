using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSysStep155ThemeRoleHomeMsgSearchMute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                schema: "erp_sys",
                table: "tenant",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconStorageKey",
                schema: "erp_sys",
                table: "tenant",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                schema: "erp_sys",
                table: "tenant",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                schema: "erp_sys",
                table: "tenant",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MuteUntil",
                schema: "erp_sys",
                table: "conversation_member",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sys_role_home_config",
                schema: "erp_sys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LandingPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
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
                    table.PrimaryKey("PK_sys_role_home_config", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sys_role_home_config_TenantId_RoleId",
                schema: "erp_sys",
                table: "sys_role_home_config",
                columns: new[] { "TenantId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sys_role_home_config",
                schema: "erp_sys");

            migrationBuilder.DropColumn(
                name: "AccentColor",
                schema: "erp_sys",
                table: "tenant");

            migrationBuilder.DropColumn(
                name: "FaviconStorageKey",
                schema: "erp_sys",
                table: "tenant");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                schema: "erp_sys",
                table: "tenant");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                schema: "erp_sys",
                table: "tenant");

            migrationBuilder.DropColumn(
                name: "MuteUntil",
                schema: "erp_sys",
                table: "conversation_member");
        }
    }
}
