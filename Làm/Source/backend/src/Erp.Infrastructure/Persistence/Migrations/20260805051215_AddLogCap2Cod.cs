using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogCap2Cod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CodAmount",
                schema: "log",
                table: "delivery_order",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CodCollectedAt",
                schema: "log",
                table: "delivery_order",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodCollectedByUserId",
                schema: "log",
                table: "delivery_order",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CodDueAt",
                schema: "log",
                table: "delivery_order",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodHandoverId",
                schema: "log",
                table: "delivery_order",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodNote",
                schema: "log",
                table: "delivery_order",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodStatus",
                schema: "log",
                table: "delivery_order",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCod",
                schema: "log",
                table: "delivery_order",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "cod_handover",
                schema: "log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DriverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CollectedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemittedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VarianceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VarianceNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReconciledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_cod_handover", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cod_handover_line",
                schema: "log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HandoverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_cod_handover_line", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_order_TenantId_CodStatus",
                schema: "log",
                table: "delivery_order",
                columns: new[] { "TenantId", "CodStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_cod_handover_TenantId_Code",
                schema: "log",
                table: "cod_handover",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cod_handover_line_TenantId_DeliveryOrderId",
                schema: "log",
                table: "cod_handover_line",
                columns: new[] { "TenantId", "DeliveryOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cod_handover_line_TenantId_HandoverId",
                schema: "log",
                table: "cod_handover_line",
                columns: new[] { "TenantId", "HandoverId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cod_handover",
                schema: "log");

            migrationBuilder.DropTable(
                name: "cod_handover_line",
                schema: "log");

            migrationBuilder.DropIndex(
                name: "IX_delivery_order_TenantId_CodStatus",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodAmount",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodCollectedAt",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodCollectedByUserId",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodDueAt",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodHandoverId",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodNote",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "CodStatus",
                schema: "log",
                table: "delivery_order");

            migrationBuilder.DropColumn(
                name: "IsCod",
                schema: "log",
                table: "delivery_order");
        }
    }
}
