using Erp.Application.DTOs.Crm;

namespace Erp.Application.Interfaces.Services.Crm;

/// <summary>Campaign marketing (UC_CRM_016, 019, 023, 026, 029, 031).</summary>
public interface ICrmCampaignService
{
    // ── Campaign CRUD ──
    Task<IReadOnlyList<CrmCampaignDto>> ListAsync(Guid tenantId, CancellationToken ct = default);
    Task<CrmCampaignDto> GetAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<CrmCampaignDto> UpsertAsync(Guid tenantId, Guid userId, CrmCampaignUpsertRequest req, CancellationToken ct = default);
    Task<CrmCampaignDto> CloseAsync(Guid tenantId, Guid userId, Guid campaignId, CrmCampaignCloseRequest req, CancellationToken ct = default);

    // ── Campaign Expense (UC_CRM_019) ──
    Task<IReadOnlyList<CrmCampaignExpenseDto>> ListExpensesAsync(Guid tenantId, Guid campaignId, CancellationToken ct = default);
    Task<CrmCampaignExpenseDto> UpsertExpenseAsync(Guid tenantId, Guid userId, Guid campaignId, CrmCampaignExpenseUpsertRequest req, CancellationToken ct = default);

    // ── Web Lead Sync (UC_CRM_026) ──
    Task<CrmWebLeadDto> SyncWebLeadAsync(Guid tenantId, CrmWebLeadSyncRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmWebLeadDto>> ListWebLeadsAsync(Guid tenantId, string? syncStatus, CancellationToken ct = default);

    // ── Marketing Metrics (UC_CRM_029, 031) ──
    Task<CrmMarketingMetricsDto> GetMetricsAsync(Guid tenantId, Guid campaignId, CancellationToken ct = default);
    Task<CrmMarketingDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken ct = default);
}
