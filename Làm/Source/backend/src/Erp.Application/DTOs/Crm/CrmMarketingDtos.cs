namespace Erp.Application.DTOs.Crm;

// ── Campaign (UC_CRM_016, 019, 023) ──
public sealed record CrmCampaignDto(
    Guid Id, string Code, string Name, string? Description,
    string Channel, string Status,
    DateTimeOffset? StartDate, DateTimeOffset? EndDate,
    decimal BudgetAmount, decimal SpentAmount,
    Guid? OwnerUserId, int LeadCount, decimal RevenueGenerated,
    DateTimeOffset? ClosedAt, string? ClosedReason);

public sealed record CrmCampaignUpsertRequest(
    Guid? Id, string Code, string Name, string? Description,
    string Channel, DateTimeOffset? StartDate, DateTimeOffset? EndDate,
    decimal BudgetAmount, Guid? OwnerUserId);

public sealed record CrmCampaignCloseRequest(string? Reason);

// ── Campaign Expense (UC_CRM_019) ──
public sealed record CrmCampaignExpenseDto(
    Guid Id, Guid CampaignId, string ExpenseType,
    string? Description, decimal Amount,
    DateTimeOffset ExpenseDate, string? InvoiceRef);

public sealed record CrmCampaignExpenseUpsertRequest(
    Guid? Id, string ExpenseType, string? Description,
    decimal Amount, DateTimeOffset? ExpenseDate, string? InvoiceRef);

// ── Marketing Metrics (UC_CRM_029, 031) ──
public sealed record CrmMarketingMetricsDto(
    Guid CampaignId, string CampaignName,
    int LeadCount, decimal TotalSpent, decimal Revenue,
    decimal CostPerLead, decimal CustomerAcquisitionCost,
    decimal Roas, decimal RoiPercent);

public sealed record CrmMarketingDashboardDto(
    int TotalCampaigns, int ActiveCampaigns,
    decimal TotalBudget, decimal TotalSpent, decimal TotalRevenue,
    decimal OverallRoi, IReadOnlyList<CrmMarketingMetricsDto> CampaignMetrics);

// ── Web Lead (UC_CRM_026) ──
public sealed record CrmWebLeadDto(
    Guid Id, string? SourceUrl, string? LandingPage,
    string? UtmSource, string? UtmMedium, string? UtmCampaign,
    string ContactName, string? Phone, string? Email,
    string SyncStatus, Guid? LeadId, Guid? CampaignId);

public sealed record CrmWebLeadSyncRequest(
    string ContactName, string? Phone, string? Email,
    string? SourceUrl, string? LandingPage,
    string? UtmSource, string? UtmMedium, string? UtmCampaign,
    Guid? CampaignId);
