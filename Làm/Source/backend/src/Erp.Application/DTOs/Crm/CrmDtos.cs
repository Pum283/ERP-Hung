namespace Erp.Application.DTOs.Crm;

public sealed record CrmCustomerDto(
    Guid Id,
    string Code,
    string CustomerType,
    string DisplayName,
    string? CompanyName,
    string? Phone,
    string? Email,
    string? TaxCode,
    string Segment,
    Guid? OwnerUserId,
    string? OwnerName,
    string Status,
    Guid? MergedIntoId,
    string? Address,
    string? Note,
    int? PotentialScore,
    int ContactCount);

public sealed record CrmCustomerUpsertRequest(
    Guid? Id,
    string Code,
    string CustomerType,
    string DisplayName,
    string? CompanyName,
    string? Phone,
    string? Email,
    string? TaxCode,
    string? Segment,
    Guid? OwnerUserId,
    string? Address,
    string? Note,
    int? PotentialScore,
    string? Status);

public sealed record CrmCustomerSearchRequest(
    string? Q,
    string? CustomerType,
    string? Segment,
    string? Status,
    Guid? OwnerUserId,
    string? Phone,
    string? TaxCode,
    bool IncludeMerged = false);

public sealed record CrmDuplicateHitDto(
    Guid Id,
    string Code,
    string DisplayName,
    string? Phone,
    string? TaxCode,
    string MatchField);

public sealed record CrmContactDto(
    Guid Id,
    Guid CustomerId,
    string FullName,
    string? Title,
    string? Phone,
    string? Email,
    bool IsPrimary);

public sealed record CrmContactUpsertRequest(
    Guid? Id,
    string FullName,
    string? Title,
    string? Phone,
    string? Email,
    bool? IsPrimary);

public sealed record CrmHandoverDto(
    Guid Id,
    Guid CustomerId,
    Guid? FromUserId,
    string? FromUserName,
    Guid ToUserId,
    string? ToUserName,
    string? Note,
    DateTimeOffset HandedAt);

public sealed record CrmHandoverRequest(Guid ToUserId, string? Note);

public sealed record CrmAssignOwnerRequest(Guid OwnerUserId);

public sealed record CrmMergeRequest(Guid SourceCustomerId, Guid TargetCustomerId);

public sealed record CrmCustomer360Dto(
    CrmCustomerDto Customer,
    IReadOnlyList<CrmContactDto> Contacts,
    IReadOnlyList<CrmHandoverDto> Handovers,
    IReadOnlyList<CrmDuplicateHitDto> PossibleDuplicates);

public sealed record CrmImportRowResult(string Code, bool Ok, string Message);

public sealed record CrmImportResult(int Total, int Success, int Failed, IReadOnlyList<CrmImportRowResult> Rows);

public sealed record CrmImportRequest(string CsvText);
