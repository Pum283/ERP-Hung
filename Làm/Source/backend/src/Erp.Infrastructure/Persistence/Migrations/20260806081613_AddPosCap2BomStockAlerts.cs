using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPosCap2BomStockAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                schema: "pos",
                table: "store",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_store_TenantId_WarehouseId",
                schema: "pos",
                table: "store",
                columns: new[] { "TenantId", "WarehouseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_store_TenantId_WarehouseId",
                schema: "pos",
                table: "store");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                schema: "pos",
                table: "store");
        }
    }
}
