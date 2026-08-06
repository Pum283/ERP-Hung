using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmTransferService : IHrmTransferService
{
    private readonly AppDbContext _db;

    public HrmTransferService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<StaffTransferDto>> ListAsync(
        Guid tenantId, string? kind, string? status, Guid? orgUnitId, CancellationToken ct = default)
    {
        var q = _db.StaffTransfers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(kind))
            q = q.Where(x => x.Kind == kind);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status);
        if (orgUnitId is Guid ou)
            q = q.Where(x => x.FromOrgUnitId == ou || x.ToOrgUnitId == ou);

        var rows = await q.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return await MapManyAsync(rows, ct);
    }

    public async Task<IReadOnlyList<StaffTransferDto>> MyOrdersAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct);
        if (emp is null) return Array.Empty<StaffTransferDto>();

        var rows = await _db.StaffTransfers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.Kind == "Order" && x.EmployeeId == emp.Id
                        && x.Status != "Cancelled" && x.Status != "Draft")
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(ct);
        return await MapManyAsync(rows, ct);
    }

    public async Task<IReadOnlyList<StaffTransferDto>> ActiveTrackingAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.StaffTransfers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Kind == "Order"
                        && (x.Status == "Issued" || x.Status == "Acknowledged" || x.Status == "Active"))
            .OrderBy(x => x.StartDate)
            .ToListAsync(ct);
        return await MapManyAsync(rows, ct);
    }

    public async Task<StaffTransferDto> CreateRequestAsync(
        Guid tenantId, Guid userId, TransferRequestCreateRequest req, CancellationToken ct = default)
    {
        await EnsureOrgAsync(tenantId, req.FromOrgUnitId, ct);
        await EnsureOrgAsync(tenantId, req.ToOrgUnitId, ct);
        if (req.FromOrgUnitId == req.ToOrgUnitId) throw new AppException("Đơn vị nguồn và đích phải khác nhau.");
        if (req.RequestedHeadcount is < 1 or > 1000) throw new AppException("Số người đề xuất không hợp lệ.");
        var reason = (req.Reason ?? "").Trim();
        if (reason.Length is < 3 or > 500) throw new AppException("Lý do 3–500 ký tự.");

        var entity = new StaffTransfer
        {
            TenantId = tenantId,
            DocNo = await NextDocNoAsync(tenantId, ct),
            Kind = "Request",
            FromOrgUnitId = req.FromOrgUnitId,
            ToOrgUnitId = req.ToOrgUnitId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            RequestedHeadcount = req.RequestedHeadcount,
            Reason = reason,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
            Status = "Draft",
            AttendanceTagged = false,
            RequestedByUserId = userId,
            CreatedBy = userId
        };
        _db.StaffTransfers.Add(entity);
        await _db.SaveChangesAsync(ct);

        if (req.Submit)
        {
            entity.Status = "Submitted";
            entity.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
        }

        return (await MapManyAsync(new[] { entity }, ct))[0];
    }

    public async Task<StaffTransferDto> CreateOrderAsync(
        Guid tenantId, Guid userId, TransferOrderCreateRequest req, CancellationToken ct = default)
    {
        var emp = await _db.Employees.FirstOrDefaultAsync(
            x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        await EnsureOrgAsync(tenantId, req.FromOrgUnitId, ct);
        await EnsureOrgAsync(tenantId, req.ToOrgUnitId, ct);
        if (req.FromOrgUnitId == req.ToOrgUnitId) throw new AppException("Đơn vị nguồn và đích phải khác nhau.");
        var reason = (req.Reason ?? "").Trim();
        if (reason.Length is < 3 or > 500) throw new AppException("Lý do 3–500 ký tự.");
        if (req.EndDate is DateOnly ed && ed < req.StartDate)
            throw new AppException("Ngày kết thúc không hợp lệ.");

        if (req.SourceRequestId is Guid sid)
        {
            var src = await _db.StaffTransfers.FirstOrDefaultAsync(
                x => x.Id == sid && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Đề xuất nguồn không tồn tại.", 404);
            if (src.Kind != "Request" || src.Status != "Approved")
                throw new AppException("Chỉ tạo lệnh từ đề xuất đã duyệt.");
            src.Status = "Converted";
            src.UpdatedBy = userId;
        }

        var entity = new StaffTransfer
        {
            TenantId = tenantId,
            DocNo = await NextDocNoAsync(tenantId, ct),
            Kind = "Order",
            EmployeeId = emp.Id,
            FromOrgUnitId = req.FromOrgUnitId,
            ToOrgUnitId = req.ToOrgUnitId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Reason = reason,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
            PlannedHours = req.PlannedHours,
            CostRate = req.CostRate,
            AttendanceTagged = req.AttendanceTagged,
            AttendanceTag = "TRANSFER",
            Status = req.Issue ? "Issued" : "Draft",
            RequestedByUserId = userId,
            SourceRequestId = req.SourceRequestId,
            CreatedBy = userId
        };
        _db.StaffTransfers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { entity }, ct))[0];
    }

    public async Task<StaffTransferDto> SubmitRequestAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Request", ct);
        if (e.Status is not ("Draft" or "Rejected")) throw new AppException("Chỉ gửi Draft/Rejected.");
        e.Status = "Submitted";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> DecideRequestAsync(
        Guid tenantId, Guid userId, Guid id, bool approve, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Request", ct);
        if (e.Status != "Submitted") throw new AppException("Chỉ duyệt phiếu Submitted.");
        e.Status = approve ? "Approved" : "Rejected";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> IssueOrderAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Order", ct);
        if (e.Status != "Draft") throw new AppException("Chỉ phát hành lệnh Draft.");
        e.Status = "Issued";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> AcknowledgeAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Order", ct);
        if (e.Status != "Issued") throw new AppException("Chỉ nhận lệnh ở trạng thái Issued.");

        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct);
        if (emp is null || e.EmployeeId != emp.Id)
            throw new AppException("Chỉ nhân viên được điều động mới nhận lệnh.", 403);

        e.Status = "Acknowledged";
        e.AcknowledgedByUserId = userId;
        e.AcknowledgedAt = DateTimeOffset.UtcNow;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> ActivateAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Order", ct);
        if (e.Status is not ("Issued" or "Acknowledged"))
            throw new AppException("Chỉ kích hoạt lệnh Issued/Acknowledged.");
        e.Status = "Active";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> CompleteAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Order", ct);
        if (e.Status is not ("Active" or "Acknowledged"))
            throw new AppException("Chỉ hoàn thành lệnh Active/Acknowledged.");
        e.Status = "Completed";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> CancelAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.StaffTransfers.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu điều động không tồn tại.", 404);
        if (e.Status is "Completed" or "Cancelled" or "Converted")
            throw new AppException("Không hủy được trạng thái hiện tại.");
        e.Status = "Cancelled";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> SetActualHoursAsync(
        Guid tenantId, Guid userId, Guid id, TransferActualHoursRequest req, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Order", ct);
        if (req.ActualHours < 0 || req.ActualHours > 100000)
            throw new AppException("Giờ thực tế không hợp lệ.");
        e.ActualHours = req.ActualHours;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<StaffTransferDto> SetAttendanceTagAsync(
        Guid tenantId, Guid userId, Guid id, bool tagged, CancellationToken ct = default)
    {
        var e = await RequireAsync(tenantId, id, "Order", ct);
        e.AttendanceTagged = tagged;
        e.AttendanceTag = tagged ? "TRANSFER" : "";
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { e }, ct))[0];
    }

    public async Task<IReadOnlyList<TransferCostReportRowDto>> CostReportAsync(
        Guid tenantId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var q = _db.StaffTransfers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Kind == "Order"
                        && x.Status != "Cancelled" && x.Status != "Draft");
        if (from is DateOnly f) q = q.Where(x => x.StartDate >= f);
        if (to is DateOnly t) q = q.Where(x => x.StartDate <= t);

        var rows = await q.ToListAsync(ct);
        var ouIds = rows.Select(x => x.ToOrgUnitId).Distinct().ToList();
        var names = await _db.OrgUnits.AsNoTracking()
            .Where(x => ouIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows
            .GroupBy(x => x.ToOrgUnitId)
            .Select(g =>
            {
                var planned = g.Sum(x => x.PlannedHours ?? 0);
                var actual = g.Sum(x => x.ActualHours ?? 0);
                var est = g.Sum(x => (x.PlannedHours ?? 0) * (x.CostRate ?? 0));
                var actCost = g.Sum(x => (x.ActualHours ?? x.PlannedHours ?? 0) * (x.CostRate ?? 0));
                return new TransferCostReportRowDto(
                    g.Key, names.GetValueOrDefault(g.Key, "?"), g.Count(),
                    planned, actual, est, actCost);
            })
            .OrderByDescending(x => x.ActualCost)
            .ToList();
    }

    private async Task<StaffTransfer> RequireAsync(Guid tenantId, Guid id, string kind, CancellationToken ct)
    {
        var e = await _db.StaffTransfers.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu điều động không tồn tại.", 404);
        if (e.Kind != kind) throw new AppException($"Phiếu không phải loại {kind}.");
        return e;
    }

    private async Task EnsureOrgAsync(Guid tenantId, Guid orgUnitId, CancellationToken ct)
    {
        if (!await _db.OrgUnits.AnyAsync(x => x.Id == orgUnitId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Đơn vị không hợp lệ.", 404);
    }

    private async Task<string> NextDocNoAsync(Guid tenantId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await _db.NumberSequences.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.DocType == "HRM.TRANSFER" && !x.IsDeleted, ct);
        if (seq is null)
        {
            seq = new NumberSequence
            {
                TenantId = tenantId,
                DocType = "HRM.TRANSFER",
                Pattern = "DD-{yyyy}-{seq:4}",
                NextValue = 1,
                ResetYear = year
            };
            _db.NumberSequences.Add(seq);
        }
        else if (seq.ResetYear != year)
        {
            seq.ResetYear = year;
            seq.NextValue = 1;
        }

        var n = seq.NextValue;
        seq.NextValue = n + 1;
        await _db.SaveChangesAsync(ct);
        return $"DD-{year}-{n:D4}";
    }

    private async Task<IReadOnlyList<StaffTransferDto>> MapManyAsync(
        IReadOnlyList<StaffTransfer> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<StaffTransferDto>();
        var empIds = rows.Where(x => x.EmployeeId is not null).Select(x => x.EmployeeId!.Value).Distinct().ToList();
        var ouIds = rows.Select(x => x.FromOrgUnitId).Concat(rows.Select(x => x.ToOrgUnitId)).Distinct().ToList();
        var userIds = rows.Select(x => x.RequestedByUserId)
            .Concat(rows.Where(x => x.AcknowledgedByUserId is not null).Select(x => x.AcknowledgedByUserId!.Value))
            .Distinct().ToList();

        var emps = empIds.Count == 0
            ? new Dictionary<Guid, Employee>()
            : await _db.Employees.AsNoTracking().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var ous = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var users = await _db.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return rows.Select(r =>
        {
            Employee? emp = r.EmployeeId is Guid eid && emps.TryGetValue(eid, out var e) ? e : null;
            var est = (r.PlannedHours ?? 0) * (r.CostRate ?? 0);
            return new StaffTransferDto(
                r.Id, r.DocNo, r.Kind, r.EmployeeId, emp?.EmployeeCode, emp?.FullName,
                r.FromOrgUnitId, ous.GetValueOrDefault(r.FromOrgUnitId, "?"),
                r.ToOrgUnitId, ous.GetValueOrDefault(r.ToOrgUnitId, "?"),
                r.StartDate, r.EndDate, r.Reason, r.RequestedHeadcount, r.Status,
                r.AttendanceTagged, r.AttendanceTag, r.PlannedHours, r.ActualHours, r.CostRate,
                est == 0 ? null : est,
                r.RequestedByUserId, users.GetValueOrDefault(r.RequestedByUserId, "?"),
                r.AcknowledgedByUserId,
                r.AcknowledgedByUserId is Guid au ? users.GetValueOrDefault(au) : null,
                r.AcknowledgedAt, r.SourceRequestId, r.Note, r.CreatedAt);
        }).ToList();
    }
}
