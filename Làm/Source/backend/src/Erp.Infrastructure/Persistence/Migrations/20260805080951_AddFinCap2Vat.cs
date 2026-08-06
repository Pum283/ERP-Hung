using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinCap2Vat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "fin",
                table: "tax",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                schema: "fin",
                table: "tax",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "fin",
                table: "tax",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxType",
                schema: "fin",
                table: "tax",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "vat_document",
                schema: "fin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RatePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    InvoiceSeries = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PartnerCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartnerTaxCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_vat_document", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vat_document_TenantId_Code",
                schema: "fin",
                table: "vat_document",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vat_document_TenantId_Direction_Status",
                schema: "fin",
                table: "vat_document",
                columns: new[] { "TenantId", "Direction", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_vat_document_TenantId_PeriodId_Direction",
                schema: "fin",
                table: "vat_document",
                columns: new[] { "TenantId", "PeriodId", "Direction" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vat_document",
                schema: "fin");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "fin",
                table: "tax");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                schema: "fin",
                table: "tax");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "fin",
                table: "tax");

            migrationBuilder.DropColumn(
                name: "TaxType",
                schema: "fin",
                table: "tax");
        }
    }
}
