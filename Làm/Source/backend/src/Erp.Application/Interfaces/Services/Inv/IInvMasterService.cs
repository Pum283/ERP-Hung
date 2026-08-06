using Erp.Application.DTOs.Inv;

namespace Erp.Application.Interfaces.Services.Inv;

public interface IInvMasterService
{
    Task<IReadOnlyList<InvItemGroupDto>> ListGroupsAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvItemGroupDto> UpsertGroupAsync(Guid tenantId, Guid userId, InvItemGroupUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<InvUomDto>> ListUomsAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvUomDto> UpsertUomAsync(Guid tenantId, Guid userId, InvUomUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<InvUnitConversionDto>> ListConversionsAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvUnitConversionDto> UpsertConversionAsync(Guid tenantId, Guid userId, InvUnitConversionUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<InvSkuDto>> ListSkusAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<InvSkuDto> UpsertSkuAsync(Guid tenantId, Guid userId, InvSkuUpsertRequest req, CancellationToken ct = default);
    Task<InvSkuDto> SetSkuStatusAsync(Guid tenantId, Guid userId, Guid skuId, InvSkuStatusRequest req, CancellationToken ct = default);
    Task<string> ExportSkusCsvAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvImportResult> ImportSkusCsvAsync(Guid tenantId, Guid userId, InvImportRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<InvWarehouseTypeDto>> ListWarehouseTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvWarehouseTypeDto> UpsertWarehouseTypeAsync(Guid tenantId, Guid userId, InvWarehouseTypeUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<InvWarehouseDto>> ListWarehousesAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvWarehouseDto> UpsertWarehouseAsync(Guid tenantId, Guid userId, InvWarehouseUpsertRequest req, CancellationToken ct = default);
    Task<InvWarehouseDetailDto> GetWarehouseDetailAsync(Guid tenantId, Guid warehouseId, CancellationToken ct = default);
    Task<InvWarehouseKeeperDto> UpsertKeeperAsync(Guid tenantId, Guid userId, Guid warehouseId, InvWarehouseKeeperUpsertRequest req, CancellationToken ct = default);
}
