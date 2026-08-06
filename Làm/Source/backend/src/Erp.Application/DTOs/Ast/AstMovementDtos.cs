namespace Erp.Application.DTOs.Ast;

public sealed record AstMovementDocDto(
    Guid Id, string Code, string DocType, DateTimeOffset DocDate,
    Guid AssetId, string? AssetCode, string? AssetName,
    Guid? FromLocationId, string? FromLocationName, Guid? ToLocationId, string? ToLocationName,
    Guid? FromEmployeeId, string? FromEmployeeName, Guid? ToEmployeeId, string? ToEmployeeName,
    string? DisposalKind, decimal? DisposalAmount, decimal? BookValueSnapshot,
    string Status, DateTimeOffset? PostedAt, string? Note);

public sealed record AstMovementUpsertRequest(
    Guid? Id, string? Code, string DocType, DateTimeOffset? DocDate, Guid AssetId,
    Guid? ToLocationId, Guid? ToEmployeeId, string? ToEmployeeName,
    string? DisposalKind, decimal? DisposalAmount, string? Note);

public sealed record AstMovementNoteRequest(string? Note);
