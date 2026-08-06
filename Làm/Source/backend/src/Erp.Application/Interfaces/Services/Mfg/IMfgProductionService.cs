using Erp.Application.DTOs.Mfg;

namespace Erp.Application.Interfaces.Services.Mfg;

public interface IMfgProductionService
{
    Task<IReadOnlyList<MfgItemDto>> ListItemsAsync(Guid tenantId, string? type, string? q, CancellationToken ct = default);
    Task<MfgItemDto> UpsertItemAsync(Guid tenantId, Guid userId, MfgItemUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<MfgWorkshopDto>> ListWorkshopsAsync(Guid tenantId, CancellationToken ct = default);
    Task<MfgWorkshopDto> UpsertWorkshopAsync(Guid tenantId, Guid userId, MfgWorkshopUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<MfgBomDto>> ListBomsAsync(Guid tenantId, CancellationToken ct = default);
    Task<MfgBomDetailDto> GetBomDetailAsync(Guid tenantId, Guid bomId, CancellationToken ct = default);
    Task<MfgBomDto> UpsertBomAsync(Guid tenantId, Guid userId, MfgBomUpsertRequest req, CancellationToken ct = default);
    Task<MfgBomLineDto> UpsertBomLineAsync(Guid tenantId, Guid userId, Guid bomId, MfgBomLineUpsertRequest req, CancellationToken ct = default);
    Task<MfgBomDto> ActivateBomAsync(Guid tenantId, Guid userId, Guid bomId, CancellationToken ct = default);

    Task<IReadOnlyList<MfgPlanDto>> ListPlansAsync(Guid tenantId, CancellationToken ct = default);
    Task<MfgPlanDetailDto> GetPlanDetailAsync(Guid tenantId, Guid planId, CancellationToken ct = default);
    Task<MfgPlanDto> UpsertPlanAsync(Guid tenantId, Guid userId, MfgPlanUpsertRequest req, CancellationToken ct = default);
    Task<MfgPlanLineDto> UpsertPlanLineAsync(Guid tenantId, Guid userId, Guid planId, MfgPlanLineUpsertRequest req, CancellationToken ct = default);
    Task<MfgPlanDto> ConfirmPlanAsync(Guid tenantId, Guid userId, Guid planId, CancellationToken ct = default);

    Task<IReadOnlyList<MfgWorkOrderDto>> ListWorkOrdersAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<MfgWorkOrderDetailDto> GetWorkOrderDetailAsync(Guid tenantId, Guid woId, CancellationToken ct = default);
    Task<MfgWorkOrderDto> UpsertWorkOrderAsync(Guid tenantId, Guid userId, MfgWorkOrderUpsertRequest req, CancellationToken ct = default);
    Task<MfgWorkOrderDto> ApproveWorkOrderAsync(Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default);
    Task<MfgWorkOrderDto> ReleaseWorkOrderAsync(Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default);
    Task<MfgWorkOrderDto> IssueMaterialsAsync(Guid tenantId, Guid userId, Guid woId, MfgMaterialIssueRequest req, CancellationToken ct = default);
    Task<MfgWorkOrderDto> ReceiveFgAsync(Guid tenantId, Guid userId, Guid woId, MfgFgReceiptRequest req, CancellationToken ct = default);
    Task<MfgWorkOrderDto> RecordScrapAsync(Guid tenantId, Guid userId, Guid woId, MfgScrapRequest req, CancellationToken ct = default);
    Task<MfgWorkOrderDto> PauseWorkOrderAsync(Guid tenantId, Guid userId, Guid woId, MfgWoNoteRequest? req = null, CancellationToken ct = default);
    Task<MfgWorkOrderDto> ResumeWorkOrderAsync(Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default);
    Task<MfgWorkOrderDto> CancelWorkOrderAsync(Guid tenantId, Guid userId, Guid woId, MfgWoCancelRequest req, CancellationToken ct = default);
    Task<MfgWorkOrderDto> CloseWorkOrderAsync(Guid tenantId, Guid userId, Guid woId, MfgWoNoteRequest? req = null, CancellationToken ct = default);

    Task<MfgCostSheetDto?> GetCostSheetAsync(Guid tenantId, Guid woId, CancellationToken ct = default);
    Task<MfgCostSheetDto> CalculateCostAsync(Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default);
    Task<MfgCostSheetDto> PushCostAsync(Guid tenantId, Guid userId, Guid woId, MfgCostPushRequest? req = null, CancellationToken ct = default);
}
