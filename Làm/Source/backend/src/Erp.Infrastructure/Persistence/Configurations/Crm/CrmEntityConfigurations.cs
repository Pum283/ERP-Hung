using Erp.Domain.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmCustomerConfig : IEntityTypeConfiguration<CrmCustomer>
{
    public void Configure(EntityTypeBuilder<CrmCustomer> b)
    {
        b.ToTable("customer", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Phone });
        b.HasIndex(x => new { x.TenantId, x.TaxCode });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.CustomerType).HasMaxLength(30).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CompanyName).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.TaxCode).HasMaxLength(40);
        b.Property(x => x.Segment).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(2000);
    }
}

public sealed class CrmContactConfig : IEntityTypeConfiguration<CrmContact>
{
    public void Configure(EntityTypeBuilder<CrmContact> b)
    {
        b.ToTable("contact", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CustomerId });
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Title).HasMaxLength(100);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);
    }
}

public sealed class CrmCustomerHandoverConfig : IEntityTypeConfiguration<CrmCustomerHandover>
{
    public void Configure(EntityTypeBuilder<CrmCustomerHandover> b)
    {
        b.ToTable("customer_handover", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CustomerId, x.HandedAt });
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class CrmLeadSourceConfig : IEntityTypeConfiguration<CrmLeadSource>
{
    public void Configure(EntityTypeBuilder<CrmLeadSource> b)
    {
        b.ToTable("lead_source", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ChannelType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class CrmLeadConfig : IEntityTypeConfiguration<CrmLead>
{
    public void Configure(EntityTypeBuilder<CrmLead> b)
    {
        b.ToTable("lead", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Phone });
        b.HasIndex(x => new { x.TenantId, x.PipelineStatus });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.CompanyName).HasMaxLength(200);
        b.Property(x => x.PipelineStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.LostReason).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(2000);
        b.Property(x => x.IntakeChannel).HasMaxLength(20).IsRequired();
    }
}

public sealed class CrmLeadTaskConfig : IEntityTypeConfiguration<CrmLeadTask>
{
    public void Configure(EntityTypeBuilder<CrmLeadTask> b)
    {
        b.ToTable("lead_task", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.LeadId, x.DueAt });
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class CrmLeadActivityConfig : IEntityTypeConfiguration<CrmLeadActivity>
{
    public void Configure(EntityTypeBuilder<CrmLeadActivity> b)
    {
        b.ToTable("lead_activity", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.LeadId, x.ActivityAt });
        b.Property(x => x.ActivityType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Content).HasMaxLength(2000).IsRequired();
    }
}

public sealed class CrmOpportunityConfig : IEntityTypeConfiguration<CrmOpportunity>
{
    public void Configure(EntityTypeBuilder<CrmOpportunity> b)
    {
        b.ToTable("opportunity", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Stage).HasMaxLength(30).IsRequired();
        b.Property(x => x.EstimatedValue).HasPrecision(18, 2);
        b.Property(x => x.ProbabilityPercent).HasPrecision(9, 4);
        b.Property(x => x.LostReason).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(2000);
    }
}

public sealed class CrmOpportunityLineConfig : IEntityTypeConfiguration<CrmOpportunityLine>
{
    public void Configure(EntityTypeBuilder<CrmOpportunityLine> b)
    {
        b.ToTable("opportunity_line", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.OpportunityId, x.LineNo });
        b.Property(x => x.ItemCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}

public sealed class CrmQuoteConfig : IEntityTypeConfiguration<CrmQuote>
{
    public void Configure(EntityTypeBuilder<CrmQuote> b)
    {
        b.ToTable("quote", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.DiscountApprovalStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.SentChannel).HasMaxLength(30).IsRequired();
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.DiscountPercent).HasPrecision(9, 4);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class CrmQuoteLineConfig : IEntityTypeConfiguration<CrmQuoteLine>
{
    public void Configure(EntityTypeBuilder<CrmQuoteLine> b)
    {
        b.ToTable("quote_line", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.QuoteId, x.LineNo });
        b.Property(x => x.ItemCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}

public sealed class CrmPriceListConfig : IEntityTypeConfiguration<CrmPriceList>
{
    public void Configure(EntityTypeBuilder<CrmPriceList> b)
    {
        b.ToTable("price_list", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class CrmPriceListItemConfig : IEntityTypeConfiguration<CrmPriceListItem>
{
    public void Configure(EntityTypeBuilder<CrmPriceListItem> b)
    {
        b.ToTable("price_list_item", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PriceListId, x.ItemCode }).IsUnique();
        b.Property(x => x.ItemCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
    }
}

public sealed class CrmSalesOrderConfig : IEntityTypeConfiguration<CrmSalesOrder>
{
    public void Configure(EntityTypeBuilder<CrmSalesOrder> b)
    {
        b.ToTable("sales_order", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.StockHoldStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.WarehousePushStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.PaidAmount).HasPrecision(18, 2);
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class CrmSalesOrderLineConfig : IEntityTypeConfiguration<CrmSalesOrderLine>
{
    public void Configure(EntityTypeBuilder<CrmSalesOrderLine> b)
    {
        b.ToTable("sales_order_line", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.OrderId, x.LineNo });
        b.Property(x => x.ItemCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}

public sealed class CrmOrderPaymentConfig : IEntityTypeConfiguration<CrmOrderPayment>
{
    public void Configure(EntityTypeBuilder<CrmOrderPayment> b)
    {
        b.ToTable("order_payment", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Method).HasMaxLength(40).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class CrmCampaignConfig : IEntityTypeConfiguration<CrmCampaign>
{
    public void Configure(EntityTypeBuilder<CrmCampaign> b)
    {
        b.ToTable("campaign", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Channel).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.BudgetAmount).HasPrecision(18, 2);
        b.Property(x => x.SpentAmount).HasPrecision(18, 2);
        b.Property(x => x.RevenueGenerated).HasPrecision(18, 2);
        b.Property(x => x.ClosedReason).HasMaxLength(500);
    }
}

public sealed class CrmCampaignExpenseConfig : IEntityTypeConfiguration<CrmCampaignExpense>
{
    public void Configure(EntityTypeBuilder<CrmCampaignExpense> b)
    {
        b.ToTable("campaign_expense", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CampaignId, x.ExpenseDate });
        b.Property(x => x.ExpenseType).HasMaxLength(40).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.InvoiceRef).HasMaxLength(100);
    }
}

public sealed class CrmWebLeadConfig : IEntityTypeConfiguration<CrmWebLead>
{
    public void Configure(EntityTypeBuilder<CrmWebLead> b)
    {
        b.ToTable("web_lead", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.SyncStatus });
        b.Property(x => x.SourceUrl).HasMaxLength(500);
        b.Property(x => x.LandingPage).HasMaxLength(500);
        b.Property(x => x.UtmSource).HasMaxLength(100);
        b.Property(x => x.UtmMedium).HasMaxLength(100);
        b.Property(x => x.UtmCampaign).HasMaxLength(100);
        b.Property(x => x.FormName).HasMaxLength(100);
        b.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Message).HasMaxLength(2000);
        b.Property(x => x.SyncStatus).HasMaxLength(30).IsRequired();
    }
}

public sealed class CrmPromotionConfig : IEntityTypeConfiguration<CrmPromotion>
{
    public void Configure(EntityTypeBuilder<CrmPromotion> b)
    {
        b.ToTable("promotion", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.DiscountType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.DiscountValue).HasPrecision(18, 2);
        b.Property(x => x.MaxDiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.MinOrderValue).HasPrecision(18, 2);
    }
}

public sealed class CrmPromotionConditionConfig : IEntityTypeConfiguration<CrmPromotionCondition>
{
    public void Configure(EntityTypeBuilder<CrmPromotionCondition> b)
    {
        b.ToTable("promotion_condition", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PromotionId });
        b.Property(x => x.ConditionType).HasMaxLength(40).IsRequired();
        b.Property(x => x.ConditionValue).HasMaxLength(200).IsRequired();
        b.Property(x => x.Operator).HasMaxLength(30).IsRequired();
    }
}

public sealed class CrmVoucherConfig : IEntityTypeConfiguration<CrmVoucher>
{
    public void Configure(EntityTypeBuilder<CrmVoucher> b)
    {
        b.ToTable("voucher", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.VoucherCode }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.PromotionId });
        b.Property(x => x.VoucherCode).HasMaxLength(60).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class CrmVoucherUsageConfig : IEntityTypeConfiguration<CrmVoucherUsage>
{
    public void Configure(EntityTypeBuilder<CrmVoucherUsage> b)
    {
        b.ToTable("voucher_usage", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.VoucherId, x.UsedAt });
        b.Property(x => x.DiscountApplied).HasPrecision(18, 2);
    }
}

public sealed class CrmChatHistoryConfig : IEntityTypeConfiguration<CrmChatHistory>
{
    public void Configure(EntityTypeBuilder<CrmChatHistory> b)
    {
        b.ToTable("chat_history", "crm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CustomerId, x.SentAt });
        b.Property(x => x.Channel).HasMaxLength(30).IsRequired();
        b.Property(x => x.ExternalConversationId).HasMaxLength(100);
        b.Property(x => x.Direction).HasMaxLength(20).IsRequired();
        b.Property(x => x.MessageText).HasMaxLength(4000).IsRequired();
        b.Property(x => x.AttachmentUrl).HasMaxLength(500);
    }
}
