using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmStep158Service
{
    // UC_HRM_088: Import lịch ca Excel
    Task<HrmShiftImportResult> ImportShiftsBulkAsync(Guid tenantId, IReadOnlyList<HrmShiftImportItem> items, CancellationToken ct = default);

    // UC_HRM_124: Lập bảng phạt
    Task<IReadOnlyList<PayrollPenaltyDto>> GetPenaltiesAsync(Guid tenantId, Guid? employeeId = null, string? status = null, CancellationToken ct = default);
    Task<PayrollPenaltyDto> GetPenaltyByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PayrollPenaltyDto> CreatePenaltyAsync(Guid tenantId, PayrollPenaltyUpsertRequest req, CancellationToken ct = default);
    Task<PayrollPenaltyDto> UpdatePenaltyAsync(Guid tenantId, Guid id, PayrollPenaltyUpsertRequest req, CancellationToken ct = default);
    Task DeletePenaltyAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_125: Áp dụng phạt vào kỳ lương
    Task<ApplyPenaltyToPayrollResult> ApplyPenaltiesToPayrollAsync(Guid tenantId, ApplyPenaltyToPayrollRequest req, CancellationToken ct = default);

    // UC_HRM_174: Đồng bộ bút toán lương sang FIN
    Task<PayrollFinSyncResult> SyncPayrollJournalToFinAsync(Guid tenantId, PayrollFinSyncRequest req, CancellationToken ct = default);
}
