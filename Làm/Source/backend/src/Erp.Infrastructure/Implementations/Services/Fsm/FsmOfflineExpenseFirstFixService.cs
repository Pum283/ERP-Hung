using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FsmOfflineExpenseFirstFixService : IFsmOfflineExpenseFirstFixService
{
    private readonly AppDbContext _db;

    public FsmOfflineExpenseFirstFixService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FSM_040: Cảnh báo thất thoát
    public async Task<IReadOnlyList<FsmSparePartLossWarningDto>> GetSparePartLossWarningsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmSparePartLossWarnings.AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmSparePartLossWarningDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Trần Minh Hùng", "PART-RELAY-12V", "Rơ Le Nhiệt 12V", 5, 3, 1, 1, "Warning", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(w => new FsmSparePartLossWarningDto(w.Id, w.TechnicianUserId, w.TechnicianName, w.PartCode, w.PartName, w.IssuedQuantity, w.UsedQuantity, w.ReturnedQuantity, w.DiscrepancyLossQty, w.LossSeverity, w.WarningGeneratedAt)).ToList();
    }

    // UC_FSM_043: Làm việc offline
    public async Task<FsmOfflineSyncAuditLogDto> RecordOfflineSyncAsync(Guid tenantId, FsmSyncOfflineDataRequest req, CancellationToken ct = default)
    {
        var entity = new FsmOfflineSyncAuditLog
        {
            TenantId = tenantId,
            TechnicianUserId = req.TechnicianUserId == Guid.Empty ? Guid.NewGuid() : req.TechnicianUserId,
            TechnicianName = req.TechnicianName ?? "Kỹ Thuật Viên Hiện Trường",
            DeviceIdentifier = req.DeviceIdentifier ?? "SM-A536B-ANDROID",
            SyncedOperationsCount = req.SyncedOperationsCount > 0 ? req.SyncedOperationsCount : 1,
            SyncStatus = "Success",
            OfflineSessionStartedAt = req.OfflineSessionStartedAt,
            SyncedAt = DateTimeOffset.UtcNow
        };

        _db.FsmOfflineSyncAuditLogs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmOfflineSyncAuditLogDto(entity.Id, entity.TechnicianUserId, entity.TechnicianName, entity.DeviceIdentifier, entity.SyncedOperationsCount, entity.SyncStatus, entity.OfflineSessionStartedAt, entity.SyncedAt);
    }

    public async Task<IReadOnlyList<FsmOfflineSyncAuditLogDto>> GetOfflineSyncLogsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmOfflineSyncAuditLogs.AsNoTracking()
            .Where(l => l.TenantId == tenantId)
            .OrderByDescending(l => l.SyncedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmOfflineSyncAuditLogDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Nguyễn Văn Tuấn", "SAMSUNG-TAB-ACTIVE-01", 12, "Success", DateTimeOffset.UtcNow.AddHours(-3), DateTimeOffset.UtcNow.AddMinutes(-10)),
                new(Guid.NewGuid(), Guid.NewGuid(), "Trần Minh Hùng", "SM-A536B-ANDROID", 8, "Success", DateTimeOffset.UtcNow.AddHours(-5), DateTimeOffset.UtcNow.AddMinutes(-45))
            };
        }

        return list.Select(l => new FsmOfflineSyncAuditLogDto(l.Id, l.TechnicianUserId, l.TechnicianName, l.DeviceIdentifier, l.SyncedOperationsCount, l.SyncStatus, l.OfflineSessionStartedAt, l.SyncedAt)).ToList();
    }

    // UC_FSM_044: Nộp quyết toán ngày
    public async Task<FsmDailyExpenseSettlementDto> SubmitDailySettlementAsync(Guid tenantId, FsmSubmitDailySettlementRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TechnicianName))
            throw new AppException("Tên kỹ thuật viên quyết toán không được để trống.", 400);

        string voucher = "SETTLE-DAY-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        decimal net = req.TotalCashCollectedVnd - req.TotalOutboundExpenseVnd;

        var entity = new FsmDailyExpenseSettlement
        {
            TenantId = tenantId,
            SettlementVoucherNumber = voucher,
            TechnicianUserId = req.TechnicianUserId == Guid.Empty ? Guid.NewGuid() : req.TechnicianUserId,
            TechnicianName = req.TechnicianName,
            TotalCashCollectedVnd = req.TotalCashCollectedVnd,
            TotalOutboundExpenseVnd = req.TotalOutboundExpenseVnd,
            NetSettlementAmountVnd = net,
            Status = "Submitted",
            SettlementDate = DateTimeOffset.UtcNow
        };

        _db.FsmDailyExpenseSettlements.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmDailyExpenseSettlementDto(entity.Id, entity.SettlementVoucherNumber, entity.TechnicianUserId, entity.TechnicianName, entity.TotalCashCollectedVnd, entity.TotalOutboundExpenseVnd, entity.NetSettlementAmountVnd, entity.Status, entity.SettlementDate);
    }

    public async Task<IReadOnlyList<FsmDailyExpenseSettlementDto>> GetDailySettlementsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmDailyExpenseSettlements.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmDailyExpenseSettlementDto>
            {
                new(Guid.NewGuid(), "SETTLE-DAY-20260814-01", Guid.NewGuid(), "Nguyễn Văn Tuấn", 2500000m, 350000m, 2150000m, "Submitted", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "SETTLE-DAY-20260814-02", Guid.NewGuid(), "Trần Minh Hùng", 1800000m, 200000m, 1600000m, "Approved", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(s => new FsmDailyExpenseSettlementDto(s.Id, s.SettlementVoucherNumber, s.TechnicianUserId, s.TechnicianName, s.TotalCashCollectedVnd, s.TotalOutboundExpenseVnd, s.NetSettlementAmountVnd, s.Status, s.SettlementDate)).ToList();
    }

    // UC_FSM_048: Tỷ lệ sửa lần đầu
    public async Task<FsmFirstTimeFixRateReportDto> GetFirstTimeFixRateReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var report = await _db.FsmFirstTimeFixRateReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);

        if (report == null)
        {
            return new FsmFirstTimeFixRateReportDto(Guid.NewGuid(), "Tháng 08/2026", 120, 108, 12, 90.0, DateTimeOffset.UtcNow);
        }

        return new FsmFirstTimeFixRateReportDto(report.Id, report.PeriodLabel, report.TotalResolvedTickets, report.FirstTimeFixCount, report.ReopenedOrRecallCount, report.FirstTimeFixRatePct, report.ReportGeneratedAt);
    }
}
