namespace Erp.Application.DTOs.Crm;

public sealed record CrmLeadSourceDto(
    Guid Id, string Code, string Name, string ChannelType, string Status, string? Note, int LeadCount);
public sealed record CrmLeadSourceUpsertRequest(
    Guid? Id, string Code, string Name, string ChannelType, string? Status, string? Note);

public sealed record CrmLeadDto(
    Guid Id, string Code, string Name, string? Phone, string? Email, string? CompanyName,
    Guid? SourceId, string? SourceName, Guid? OwnerUserId, string? OwnerName,
    Guid? CustomerId, string PipelineStatus, int Score, DateTimeOffset? NextFollowUpAt,
    string? LostReason, Guid? OpportunityId, string IntakeChannel, string? Note,
    int OpenTaskCount, int ActivityCount);
public sealed record CrmLeadUpsertRequest(
    Guid? Id, string? Code, string Name, string? Phone, string? Email, string? CompanyName,
    Guid? SourceId, Guid? OwnerUserId, Guid? CustomerId, string? PipelineStatus,
    int? Score, DateTimeOffset? NextFollowUpAt, string? Note, string? IntakeChannel);

public sealed record CrmLeadAssignRequest(Guid OwnerUserId);
public sealed record CrmLeadStatusRequest(string PipelineStatus, string? Note);
public sealed record CrmLeadLostRequest(string LostReason);
public sealed record CrmLeadAutoIntakeRequest(
    string Name, string? Phone, string? Email, string? CompanyName,
    string? SourceCode, Guid? OwnerUserId, string? Note);

public sealed record CrmLeadTaskDto(
    Guid Id, Guid LeadId, string Title, DateTimeOffset DueAt, Guid? AssigneeUserId,
    string? AssigneeName, string Status, bool IsReminder, string? Note);
public sealed record CrmLeadTaskUpsertRequest(
    Guid? Id, Guid LeadId, string Title, DateTimeOffset DueAt, Guid? AssigneeUserId,
    string? Status, bool? IsReminder, string? Note);

public sealed record CrmLeadActivityDto(
    Guid Id, Guid LeadId, string ActivityType, string Content, Guid CreatedByUserId,
    string? CreatedByName, DateTimeOffset ActivityAt);
public sealed record CrmLeadActivityUpsertRequest(
    Guid LeadId, string ActivityType, string Content, DateTimeOffset? ActivityAt);

public sealed record CrmLeadDetailDto(
    CrmLeadDto Lead, IReadOnlyList<CrmLeadTaskDto> Tasks, IReadOnlyList<CrmLeadActivityDto> Activities);

public sealed record CrmLeadImportRequest(string CsvContent);
public sealed record CrmLeadImportResult(int Created, int Skipped, IReadOnlyList<string> Errors);

public sealed record CrmLeadConversionRowDto(
    string PipelineStatus, int Count, decimal ConversionRatePercent);
public sealed record CrmLeadConversionReportDto(
    int TotalLeads, int Converted, int Lost, decimal ConversionRatePercent,
    IReadOnlyList<CrmLeadConversionRowDto> ByStatus);

public sealed record CrmOpportunityDto(
    Guid Id, string Code, string Name, Guid? LeadId, string? LeadCode,
    Guid? CustomerId, string? CustomerName, Guid? OwnerUserId, string? OwnerName,
    string Stage, decimal EstimatedValue, decimal ProbabilityPercent,
    DateTimeOffset? ExpectedCloseDate, Guid? QuoteId, string? QuoteCode,
    string? LostReason, string? Note, int LineCount);
public sealed record CrmOpportunityUpsertRequest(
    Guid? Id, string? Code, string Name, Guid? LeadId, Guid? CustomerId, Guid? OwnerUserId,
    string? Stage, decimal? EstimatedValue, decimal? ProbabilityPercent,
    DateTimeOffset? ExpectedCloseDate, string? Note);
public sealed record CrmOpportunityLineDto(
    Guid Id, Guid OpportunityId, string ItemCode, string ItemName,
    decimal Quantity, decimal UnitPrice, decimal LineAmount, int LineNo);
public sealed record CrmOpportunityLineUpsertRequest(
    Guid? Id, string ItemCode, string ItemName, decimal Quantity, decimal UnitPrice);
public sealed record CrmOpportunityDetailDto(
    CrmOpportunityDto Opportunity, IReadOnlyList<CrmOpportunityLineDto> Lines);
public sealed record CrmOpportunityStageRequest(string Stage, string? LostReason);
