using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmCap2MarketingPromo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campaign",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SpentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeadCount = table.Column<int>(type: "int", nullable: false),
                    RevenueGenerated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_campaign", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "campaign_expense",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InvoiceRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_campaign_expense", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_history",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExternalConversationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_chat_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promotion",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DiscountType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MinOrderValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MaxUsageTotal = table.Column<int>(type: "int", nullable: true),
                    MaxUsagePerCustomer = table.Column<int>(type: "int", nullable: true),
                    CurrentUsageCount = table.Column<int>(type: "int", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_promotion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promotion_condition",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConditionType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ConditionValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_promotion_condition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "voucher",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    MaxUsage = table.Column<int>(type: "int", nullable: false),
                    AssignedCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_voucher", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_usage",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_voucher_usage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "web_lead",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LandingPage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UtmSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UtmMedium = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UtmCampaign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FormName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SyncStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_web_lead", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_campaign_TenantId_Code",
                schema: "crm",
                table: "campaign",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaign_expense_TenantId_CampaignId_ExpenseDate",
                schema: "crm",
                table: "campaign_expense",
                columns: new[] { "TenantId", "CampaignId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_history_TenantId_CustomerId_SentAt",
                schema: "crm",
                table: "chat_history",
                columns: new[] { "TenantId", "CustomerId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_promotion_TenantId_Code",
                schema: "crm",
                table: "promotion",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promotion_condition_TenantId_PromotionId",
                schema: "crm",
                table: "promotion_condition",
                columns: new[] { "TenantId", "PromotionId" });

            migrationBuilder.CreateIndex(
                name: "IX_voucher_TenantId_PromotionId",
                schema: "crm",
                table: "voucher",
                columns: new[] { "TenantId", "PromotionId" });

            migrationBuilder.CreateIndex(
                name: "IX_voucher_TenantId_VoucherCode",
                schema: "crm",
                table: "voucher",
                columns: new[] { "TenantId", "VoucherCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voucher_usage_TenantId_VoucherId_UsedAt",
                schema: "crm",
                table: "voucher_usage",
                columns: new[] { "TenantId", "VoucherId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_web_lead_TenantId_SyncStatus",
                schema: "crm",
                table: "web_lead",
                columns: new[] { "TenantId", "SyncStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "campaign_expense",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "chat_history",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "promotion",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "promotion_condition",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "voucher",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "voucher_usage",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "web_lead",
                schema: "crm");
        }
    }
}
