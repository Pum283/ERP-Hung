namespace Erp.Application.DTOs.Ast;

public sealed record AstStocktakeDto(
    Guid Id, string Code, Guid? LocationId, string? LocationName, string Status,
    int LineCount, int CountedCount, int VarianceCount,
    DateTimeOffset? CountedAt, DateTimeOffset? ReviewedAt, string? Note);

public sealed record AstStocktakeLineDto(
    Guid Id, Guid StocktakeId, Guid AssetId, string AssetCode, string AssetName,
    Guid? LocationId, string? LocationName, int ExpectedPresent, bool? CountedPresent,
    int Variance, string? Note);

public sealed record AstStocktakeDetailDto(
    AstStocktakeDto Header, IReadOnlyList<AstStocktakeLineDto> Lines);

public sealed record AstStocktakeCreateRequest(Guid? LocationId, string? Note);
public sealed record AstStocktakeCountRequest(Guid LineId, bool CountedPresent, string? Note);

public sealed record AstRegisterRowDto(
    Guid Id, string Code, string Name, string? GroupName, string? LocationName,
    string? MethodName, string? AssignedEmployeeName, decimal OriginalCost,
    decimal AccumulatedDepreciation, decimal BookValue, string Status,
    DateTimeOffset? CapitalizeDate, DateTimeOffset? DisposedAt);

public sealed record AstByLocationRowDto(
    Guid? LocationId, string LocationName, int AssetCount,
    decimal OriginalCost, decimal AccumulatedDepreciation, decimal BookValue);

public sealed record AstDepreciationReportDto(
    Guid? RunId, string? RunCode, int Year, int Month, string? Status,
    decimal TotalAmount, int LineCount, IReadOnlyList<AstDepreciationLineDto> Lines);
