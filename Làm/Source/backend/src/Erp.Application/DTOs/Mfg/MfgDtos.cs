namespace Erp.Application.DTOs.Mfg;

public sealed record MfgItemDto(Guid Id, string Code, string Name, string ItemType, string Unit, decimal StandardCost, string Status, string? Note);
public sealed record MfgItemUpsertRequest(Guid? Id, string Code, string Name, string ItemType, string? Unit, decimal? StandardCost, string? Status, string? Note);

public sealed record MfgWorkshopDto(Guid Id, string Code, string Name, string WorkshopType, string Status, string? Note);
public sealed record MfgWorkshopUpsertRequest(Guid? Id, string Code, string Name, string? WorkshopType, string? Status, string? Note);

public sealed record MfgBomDto(
    Guid Id, string Code, Guid ParentItemId, string? ParentItemCode, string? ParentItemName,
    string Version, string Status, string? Note, int LineCount);
public sealed record MfgBomUpsertRequest(Guid? Id, string? Code, Guid ParentItemId, string Version, string? Status, string? Note);
public sealed record MfgBomLineDto(
    Guid Id, Guid BomId, Guid ComponentItemId, string? ComponentCode, string? ComponentName,
    string? ComponentType, decimal Qty, string Unit, int Level, string? Note);
public sealed record MfgBomLineUpsertRequest(
    Guid? Id, Guid ComponentItemId, decimal Qty, string? Unit, int? Level, string? Note);
public sealed record MfgBomDetailDto(MfgBomDto Bom, IReadOnlyList<MfgBomLineDto> Lines);

public sealed record MfgPlanDto(
    Guid Id, string Code, string SourceOrderCode, string Status, string? Note, int LineCount);
public sealed record MfgPlanUpsertRequest(Guid? Id, string? Code, string SourceOrderCode, string? Note);
public sealed record MfgPlanLineDto(
    Guid Id, Guid PlanId, Guid ItemId, string? ItemCode, string? ItemName, decimal Qty,
    Guid? WorkshopId, string? WorkshopName, string? Note);
public sealed record MfgPlanLineUpsertRequest(
    Guid? Id, Guid ItemId, decimal Qty, Guid? WorkshopId, string? Note);
public sealed record MfgPlanDetailDto(MfgPlanDto Plan, IReadOnlyList<MfgPlanLineDto> Lines);

public sealed record MfgWorkOrderDto(
    Guid Id, string Code, Guid ItemId, string? ItemCode, string? ItemName, decimal Qty,
    Guid? WorkshopId, string? WorkshopName, Guid? BomId, string? BomCode, Guid? PlanId,
    string Status, string? Note, decimal QtyIssuedMaterial, decimal QtyFgReceived, decimal QtyScrap,
    DateTimeOffset? ApprovedAt, DateTimeOffset? ReleasedAt, DateTimeOffset? PrintedAt,
    DateTimeOffset? PausedAt, DateTimeOffset? ClosedAt, string? CancelReason);

public sealed record MfgWorkOrderUpsertRequest(
    Guid? Id, string? Code, Guid ItemId, decimal Qty, Guid? WorkshopId, Guid? BomId, Guid? PlanId, string? Note);

public sealed record MfgMaterialIssueDto(
    Guid Id, Guid WorkOrderId, Guid ItemId, string? ItemCode, string? ItemName,
    decimal Qty, decimal UnitCost, decimal Amount, string Unit, DateTimeOffset IssuedAt, string? Note);
public sealed record MfgMaterialIssueRequest(Guid ItemId, decimal Qty, string? Unit, decimal? UnitCost, string? Note);

public sealed record MfgFgReceiptDto(
    Guid Id, Guid WorkOrderId, Guid ItemId, string? ItemCode, string? ItemName,
    decimal Qty, string Unit, DateTimeOffset ReceivedAt, string? Note);
public sealed record MfgFgReceiptRequest(decimal Qty, string? Note);

public sealed record MfgScrapDto(
    Guid Id, Guid WorkOrderId, Guid? ItemId, string? ItemCode, string? ItemName,
    decimal Qty, string Unit, string ScrapType, DateTimeOffset RecordedAt, string? Note);
public sealed record MfgScrapRequest(Guid? ItemId, decimal Qty, string? Unit, string ScrapType, string? Note);
public sealed record MfgWoNoteRequest(string? Note);
public sealed record MfgWoCancelRequest(string Reason);

public sealed record MfgCostSheetLineDto(
    Guid Id, Guid? MaterialIssueId, Guid ItemId, string? ItemCode, string? ItemName,
    string Source, decimal Qty, decimal UnitCost, decimal Amount, string? Note);

public sealed record MfgCostSheetDto(
    Guid Id, string Code, Guid WorkOrderId, string? WorkOrderCode, string Status,
    decimal MaterialCost, decimal LaborCost, decimal OverheadCost, decimal TotalCost,
    decimal GoodQty, decimal UnitCost,
    Guid? InvSkuId, string? InvSkuCode, Guid? FinJournalId, string? FinJournalCode,
    DateTimeOffset? CalculatedAt, DateTimeOffset? PushedAt, string? Note,
    IReadOnlyList<MfgCostSheetLineDto> Lines);

public sealed record MfgCostPushRequest(
    Guid? PeriodId, Guid? WipAccountId, Guid? FgAccountId, string? Note);

public sealed record MfgWorkOrderDetailDto(
    MfgWorkOrderDto Order,
    IReadOnlyList<MfgMaterialIssueDto> Issues,
    IReadOnlyList<MfgFgReceiptDto> Receipts,
    IReadOnlyList<MfgScrapDto> Scraps,
    IReadOnlyList<MfgBomLineDto> RequiredMaterials,
    MfgCostSheetDto? CostSheet);
