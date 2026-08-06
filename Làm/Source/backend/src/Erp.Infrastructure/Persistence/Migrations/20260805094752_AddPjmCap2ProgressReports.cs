using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPjmCap2ProgressReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueDate",
                schema: "pjm",
                table: "wbs_item",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMilestone",
                schema: "pjm",
                table: "wbs_item",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentComplete",
                schema: "pjm",
                table: "wbs_item",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_wbs_item_TenantId_DueDate",
                schema: "pjm",
                table: "wbs_item",
                columns: new[] { "TenantId", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wbs_item_TenantId_DueDate",
                schema: "pjm",
                table: "wbs_item");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "pjm",
                table: "wbs_item");

            migrationBuilder.DropColumn(
                name: "IsMilestone",
                schema: "pjm",
                table: "wbs_item");

            migrationBuilder.DropColumn(
                name: "PercentComplete",
                schema: "pjm",
                table: "wbs_item");
        }
    }
}
