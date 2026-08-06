using Erp.Application.DTOs.Crm;

namespace Erp.Application.Interfaces.Services.Crm;

public interface ICrmLeadService
{
    Task<IReadOnlyList<CrmLeadSourceDto>> ListSourcesAsync(Guid tenantId, CancellationToken ct = default);
    Task<CrmLeadSourceDto> UpsertSourceAsync(Guid tenantId, Guid userId, CrmLeadSourceUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<CrmLeadDto>> ListLeadsAsync(Guid tenantId, string? q, string? status, Guid? ownerUserId, CancellationToken ct = default);
    Task<CrmLeadDetailDto> GetLeadDetailAsync(Guid tenantId, Guid leadId, CancellationToken ct = default);
    Task<CrmLeadDto> UpsertLeadAsync(Guid tenantId, Guid userId, CrmLeadUpsertRequest req, CancellationToken ct = default);
    Task<CrmLeadDto> AutoIntakeAsync(Guid tenantId, Guid userId, CrmLeadAutoIntakeRequest req, CancellationToken ct = default);
    Task<CrmLeadDto> AssignAsync(Guid tenantId, Guid userId, Guid leadId, CrmLeadAssignRequest req, CancellationToken ct = default);
    Task<CrmLeadDto> SetStatusAsync(Guid tenantId, Guid userId, Guid leadId, CrmLeadStatusRequest req, CancellationToken ct = default);
    Task<CrmLeadDto> MarkLostAsync(Guid tenantId, Guid userId, Guid leadId, CrmLeadLostRequest req, CancellationToken ct = default);
    Task<CrmOpportunityDto> ConvertToOpportunityAsync(Guid tenantId, Guid userId, Guid leadId, CancellationToken ct = default);

    Task<CrmLeadTaskDto> UpsertTaskAsync(Guid tenantId, Guid userId, CrmLeadTaskUpsertRequest req, CancellationToken ct = default);
    Task<CrmLeadActivityDto> AddActivityAsync(Guid tenantId, Guid userId, CrmLeadActivityUpsertRequest req, CancellationToken ct = default);

    Task<CrmLeadImportResult> ImportCsvAsync(Guid tenantId, Guid userId, CrmLeadImportRequest req, CancellationToken ct = default);
    Task<CrmLeadConversionReportDto> GetConversionReportAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<CrmOpportunityDto>> ListOpportunitiesAsync(Guid tenantId, string? q, string? stage, CancellationToken ct = default);
    Task<CrmOpportunityDetailDto> GetOpportunityDetailAsync(Guid tenantId, Guid opportunityId, CancellationToken ct = default);
    Task<CrmOpportunityDto> UpsertOpportunityAsync(Guid tenantId, Guid userId, CrmOpportunityUpsertRequest req, CancellationToken ct = default);
    Task<CrmOpportunityLineDto> UpsertOpportunityLineAsync(Guid tenantId, Guid userId, Guid opportunityId, CrmOpportunityLineUpsertRequest req, CancellationToken ct = default);
    Task<CrmOpportunityDto> SetOpportunityStageAsync(Guid tenantId, Guid userId, Guid opportunityId, CrmOpportunityStageRequest req, CancellationToken ct = default);
    Task<CrmQuoteDto> CreateQuoteFromOpportunityAsync(Guid tenantId, Guid userId, Guid opportunityId, CancellationToken ct = default);
}
