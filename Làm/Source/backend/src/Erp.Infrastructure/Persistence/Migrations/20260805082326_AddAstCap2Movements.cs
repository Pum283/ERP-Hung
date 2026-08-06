using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAstCap2Movements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedEmployeeId",
                schema: "ast",
                table: "asset",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedEmployeeName",
                schema: "ast",
                table: "asset",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DisposalAmount",
                schema: "ast",
                table: "asset",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DisposedAt",
                schema: "ast",
                table: "asset",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "movement_doc",
                schema: "ast",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DocType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromEmployeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToEmployeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisposalKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisposalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BookValueSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_movement_doc", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_movement_doc_TenantId_AssetId",
                schema: "ast",
                table: "movement_doc",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_movement_doc_TenantId_Code",
                schema: "ast",
                table: "movement_doc",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movement_doc_TenantId_DocType_Status",
                schema: "ast",
                table: "movement_doc",
                columns: new[] { "TenantId", "DocType", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movement_doc",
                schema: "ast");

            migrationBuilder.DropColumn(
                name: "AssignedEmployeeId",
                schema: "ast",
                table: "asset");

            migrationBuilder.DropColumn(
                name: "AssignedEmployeeName",
                schema: "ast",
                table: "asset");

            migrationBuilder.DropColumn(
                name: "DisposalAmount",
                schema: "ast",
                table: "asset");

            migrationBuilder.DropColumn(
                name: "DisposedAt",
                schema: "ast",
                table: "asset");
        }
    }
}
