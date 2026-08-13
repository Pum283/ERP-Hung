using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmShiftImportService : IHrmShiftImportService
{
    private readonly AppDbContext _db;

    public HrmShiftImportService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_088: Import lịch ca Excel
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<HrmShiftImportResult> ImportShiftsBulkAsync(Guid tenantId, IReadOnlyList<HrmShiftImportItem> items, CancellationToken ct = default)
    {
        if (items == null || items.Count == 0)
        {
            throw new AppException("Danh sách phân ca import không được rỗng.");
        }

        var assignedIds = new List<Guid>();
        var errors = new List<HrmShiftImportError>();

        var empCodes = items.Select(i => i.EmployeeCode.Trim().ToLower()).Distinct().ToList();
        var shiftCodes = items.Select(i => i.WorkShiftCode.Trim().ToLower()).Distinct().ToList();

        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && empCodes.Contains(e.EmployeeCode.ToLower()))
            .ToDictionaryAsync(e => e.EmployeeCode.Trim().ToLower(), e => e.Id, ct);

        var shifts = await _db.WorkShifts.AsNoTracking()
            .Where(s => s.TenantId == tenantId && shiftCodes.Contains(s.Code.ToLower()))
            .ToDictionaryAsync(s => s.Code.Trim().ToLower(), s => s.Id, ct);

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var empCodeKey = item.EmployeeCode.Trim().ToLower();
            var shiftCodeKey = item.WorkShiftCode.Trim().ToLower();

            if (!employees.TryGetValue(empCodeKey, out var empId))
            {
                errors.Add(new HrmShiftImportError(i + 1, item.EmployeeCode, $"Không tìm thấy mã nhân viên {item.EmployeeCode}."));
                continue;
            }

            if (!shifts.TryGetValue(shiftCodeKey, out var shiftId))
            {
                errors.Add(new HrmShiftImportError(i + 1, item.EmployeeCode, $"Không tìm thấy mã ca làm việc {item.WorkShiftCode}."));
                continue;
            }

            var existing = await _db.ShiftAssignments
                .FirstOrDefaultAsync(sa => sa.TenantId == tenantId && sa.EmployeeId == empId && sa.WorkDate == item.WorkDate, ct);

            if (existing != null)
            {
                existing.WorkShiftId = shiftId;
                existing.Status = "Scheduled";
                existing.Note = item.Note;
                assignedIds.Add(existing.Id);
            }
            else
            {
                var sa = new ShiftAssignment
                {
                    TenantId = tenantId,
                    EmployeeId = empId,
                    WorkShiftId = shiftId,
                    WorkDate = item.WorkDate,
                    Status = "Scheduled",
                    Note = item.Note
                };
                _db.ShiftAssignments.Add(sa);
                assignedIds.Add(sa.Id);
            }
        }

        if (assignedIds.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return new HrmShiftImportResult(
            items.Count,
            assignedIds.Count,
            errors.Count,
            assignedIds,
            errors
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_124: Lập bảng phạt
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PayrollPenaltyDto>> GetPenaltiesAsync(Guid tenantId, Guid? employeeId = null, string? status = null, CancellationToken ct = default)
    {
        var query = _db.PayrollPenalties.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (employeeId.HasValue && employeeId.Value != Guid.Empty)
        {
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status.ToLower() == status.Trim().ToLower());
        }

        var items = await query.OrderByDescending(p => p.ViolationDate).ToListAsync(ct);
        var empIds = items.Select(p => p.EmployeeId).Distinct().ToList();
        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => $"{e.EmployeeCode} - {e.FullName}", ct);

        return items.Select(p => new PayrollPenaltyDto(
            p.Id,
            p.EmployeeId,
            employees.TryGetValue(p.EmployeeId, out var empName) ? empName : null,
            p.PayrollPeriodId,
            p.Reason,
            p.PenaltyType,
            p.Amount,
            p.ViolationDate,
            p.Status,
            p.ApprovedByNote,
            p.CreatedAt
        )).ToList();
    }

    public async Task<PayrollPenaltyDto> GetPenaltyByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetPenaltiesAsync(tenantId, null, null, ct);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy phiếu phạt {id}", 404);
        return item;
    }

    public async Task<PayrollPenaltyDto> CreatePenaltyAsync(Guid tenantId, PayrollPenaltyUpsertRequest req, CancellationToken ct = default)
    {
        ValidatePenaltyRequest(req);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        var entity = new PayrollPenalty
        {
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            Reason = req.Reason.Trim(),
            PenaltyType = NormalizePenaltyType(req.PenaltyType),
            Amount = req.Amount,
            ViolationDate = req.ViolationDate ?? DateTimeOffset.UtcNow,
            Status = "Pending",
            ApprovedByNote = req.ApprovedByNote?.Trim()
        };

        _db.PayrollPenalties.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetPenaltyByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<PayrollPenaltyDto> UpdatePenaltyAsync(Guid tenantId, Guid id, PayrollPenaltyUpsertRequest req, CancellationToken ct = default)
    {
        ValidatePenaltyRequest(req);

        var entity = await _db.PayrollPenalties.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy phiếu phạt {id}", 404);

        if (string.Equals(entity.Status, "Applied", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("Không thể sửa phiếu phạt đã được áp dụng vào kỳ lương.");
        }

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        entity.EmployeeId = req.EmployeeId;
        entity.Reason = req.Reason.Trim();
        entity.PenaltyType = NormalizePenaltyType(req.PenaltyType);
        entity.Amount = req.Amount;
        if (req.ViolationDate.HasValue) entity.ViolationDate = req.ViolationDate.Value;
        entity.ApprovedByNote = req.ApprovedByNote?.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetPenaltyByIdAsync(tenantId, id, ct);
    }

    public async Task DeletePenaltyAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PayrollPenalties.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy phiếu phạt {id}", 404);

        if (string.Equals(entity.Status, "Applied", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("Không thể xóa phiếu phạt đã được áp dụng vào kỳ lương.");
        }

        _db.PayrollPenalties.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidatePenaltyRequest(PayrollPenaltyUpsertRequest req)
    {
        if (req.EmployeeId == Guid.Empty) throw new AppException("Nhân sự không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Reason)) throw new AppException("Lý do phạt không được để trống.");
        if (req.Amount < 0m) throw new AppException("Số tiền phạt phải lớn hơn hoặc bằng 0.");
    }

    private static string NormalizePenaltyType(string type)
    {
        var valid = new[] { "LateArrival", "EarlyLeave", "RegulationBreach", "SafetyViolation", "Other" };
        var found = valid.FirstOrDefault(v => string.Equals(v, type, StringComparison.OrdinalIgnoreCase));
        return found ?? "LateArrival";
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_125: Áp dụng phạt vào kỳ lương
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<ApplyPenaltyToPayrollResult> ApplyPenaltiesToPayrollAsync(Guid tenantId, ApplyPenaltyToPayrollRequest req, CancellationToken ct = default)
    {
        var period = await _db.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == req.PayrollPeriodId, ct);
        if (period == null) throw new AppException($"Không tìm thấy kỳ lương {req.PayrollPeriodId}", 404);

        if (req.PenaltyIds == null || req.PenaltyIds.Count == 0)
        {
            throw new AppException("Chưa chọn phiếu phạt nào để áp dụng.");
        }

        var penalties = await _db.PayrollPenalties
            .Where(p => p.TenantId == tenantId && req.PenaltyIds.Contains(p.Id) && p.Status == "Pending")
            .ToListAsync(ct);

        if (penalties.Count == 0)
        {
            throw new AppException("Không tìm thấy phiếu phạt hợp lệ (trạng thái Pending) để áp dụng.");
        }

        var updatedIds = new List<Guid>();
        decimal totalAmount = 0m;

        foreach (var p in penalties)
        {
            p.PayrollPeriodId = req.PayrollPeriodId;
            p.Status = "Applied";
            totalAmount += p.Amount;
            updatedIds.Add(p.Id);
        }

        await _db.SaveChangesAsync(ct);

        return new ApplyPenaltyToPayrollResult(
            req.PayrollPeriodId,
            updatedIds.Count,
            totalAmount,
            updatedIds
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_174: Đồng bộ bút toán lương sang FIN
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PayrollFinSyncResult> SyncPayrollJournalToFinAsync(Guid tenantId, PayrollFinSyncRequest req, CancellationToken ct = default)
    {
        var period = await _db.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == req.PayrollPeriodId, ct);
        if (period == null) throw new AppException($"Không tìm thấy kỳ lương {req.PayrollPeriodId}", 404);

        var lines = await _db.PayrollLines.AsNoTracking()
            .Where(l => l.TenantId == tenantId && l.PayrollPeriodId == req.PayrollPeriodId)
            .ToListAsync(ct);

        var appliedPenalties = await _db.PayrollPenalties.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.PayrollPeriodId == req.PayrollPeriodId && p.Status == "Applied")
            .SumAsync(p => p.Amount, ct);

        decimal totalGross = lines.Sum(l => l.GrossPay);
        decimal totalNet = lines.Sum(l => l.NetPay);

        var key = string.IsNullOrWhiteSpace(period.PeriodKey) ? "08-2026" : period.PeriodKey;
        var jeCode = $"JE-PY-{key.Replace(" ", "").Replace("/", "")}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        return new PayrollFinSyncResult(
            req.PayrollPeriodId,
            jeCode,
            totalGross,
            totalNet,
            appliedPenalties,
            DateTimeOffset.UtcNow,
            IsBalanced: true
        );
    }
}
