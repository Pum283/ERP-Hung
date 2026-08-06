namespace Erp.Application.DTOs.Ast;

public sealed record AstAssetGroupDto(
    Guid Id, string Code, string Name, int DefaultUsefulLifeMonths,
    decimal DefaultDepreciationRate, string Status, string? Note, int AssetCount);
public sealed record AstAssetGroupUpsertRequest(
    Guid? Id, string Code, string Name, int? DefaultUsefulLifeMonths,
    decimal? DefaultDepreciationRate, string? Status, string? Note);

public sealed record AstLocationDto(
    Guid Id, string Code, string Name, string? BranchName, string Status, string? Note);
public sealed record AstLocationUpsertRequest(
    Guid? Id, string Code, string Name, string? BranchName, string? Status, string? Note);

public sealed record AstDepreciationMethodDto(
    Guid Id, string Code, string Name, string MethodType,
    int DefaultUsefulLifeMonths, decimal DefaultRatePercent, string Status, string? Note);
public sealed record AstDepreciationMethodUpsertRequest(
    Guid? Id, string Code, string Name, string MethodType,
    int? DefaultUsefulLifeMonths, decimal? DefaultRatePercent, string? Status, string? Note);

public sealed record AstAssetDto(
    Guid Id, string Code, string Name, Guid? GroupId, string? GroupName,
    Guid? LocationId, string? LocationName, Guid? DepreciationMethodId, string? MethodName,
    Guid? AssignedEmployeeId, string? AssignedEmployeeName,
    decimal OriginalCost, DateTimeOffset? CapitalizeDate, int UsefulLifeMonths,
    decimal DepreciationRatePercent, decimal AccumulatedDepreciation, decimal BookValue,
    string Status, DateTimeOffset? DisposedAt, decimal? DisposalAmount,
    string? PurchaseRef, string? Note);
public sealed record AstAssetUpsertRequest(
    Guid? Id, string? Code, string Name, Guid? GroupId, Guid? LocationId, Guid? DepreciationMethodId,
    Guid? AssignedEmployeeId, string? AssignedEmployeeName,
    decimal OriginalCost, DateTimeOffset? CapitalizeDate, int? UsefulLifeMonths,
    decimal? DepreciationRatePercent, string? Status, string? PurchaseRef, string? Note,
    bool? CapitalizeFromPurchase);

public sealed record AstDepreciationLineDto(
    Guid Id, Guid RunId, Guid AssetId, string? AssetCode, string? AssetName,
    decimal Amount, decimal BookValueBefore, decimal BookValueAfter, int LineNo);

public sealed record AstDepreciationRunDto(
    Guid Id, string Code, int Year, int Month, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd,
    string Status, decimal TotalAmount, int LineCount, Guid? FinJournalId, DateTimeOffset? PostedAt);

public sealed record AstDepreciationRunDetailDto(
    AstDepreciationRunDto Run, IReadOnlyList<AstDepreciationLineDto> Lines);

public sealed record AstDepreciationCalcRequest(int Year, int Month);
public sealed record AstPushFinRequest(Guid? ExpenseAccountId, Guid? AccumAccountId, Guid? PeriodId);
