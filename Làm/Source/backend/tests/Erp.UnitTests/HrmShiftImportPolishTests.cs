using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class HrmShiftImportPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmShiftImportService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _workShiftId = Guid.NewGuid();
    private readonly Guid _payrollPeriodId = Guid.NewGuid();

    public HrmShiftImportPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-shift-import-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T158", Name = "Tenant 158" });
        _db.Employees.Add(new Employee
        {
            Id = _employeeId,
            TenantId = _tenant,
            EmployeeCode = "EMP158",
            FullName = "Hoàng Văn E",
            Status = "Active"
        });
        _db.WorkShifts.Add(new WorkShift
        {
            Id = _workShiftId,
            TenantId = _tenant,
            Code = "HC_01",
            Name = "Ca Hành chính 8h-17h"
        });
        _db.PayrollPeriods.Add(new PayrollPeriod
        {
            Id = _payrollPeriodId,
            TenantId = _tenant,
            PeriodKey = "2026-08",
            PeriodFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            PeriodTo = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = "Draft"
        });

        _db.SaveChanges();

        _svc = new HrmShiftImportService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_088: Import lịch ca Excel
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC088_ImportShiftsBulk_Succeeds()
    {
        var items = new List<HrmShiftImportItem>
        {
            new("EMP158", "HC_01", DateOnly.FromDateTime(DateTime.UtcNow), "Phân ca HC")
        };

        var result = await _svc.ImportShiftsBulkAsync(_tenant, items);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
    }

    [Fact]
    public async Task UC088_ImportShiftsBulk_EmptyList_ThrowsAppException()
    {
        await Assert.ThrowsAsync<AppException>(() => _svc.ImportShiftsBulkAsync(_tenant, new List<HrmShiftImportItem>()));
    }

    [Fact]
    public async Task UC088_ImportShiftsBulk_InvalidEmployeeOrShift_ReturnsErrors()
    {
        var items = new List<HrmShiftImportItem>
        {
            new("INVALID_EMP", "HC_01", DateOnly.FromDateTime(DateTime.UtcNow), null),
            new("EMP158", "INVALID_SHIFT", DateOnly.FromDateTime(DateTime.UtcNow), null)
        };

        var result = await _svc.ImportShiftsBulkAsync(_tenant, items);

        Assert.Equal(2, result.TotalProcessed);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(2, result.FailedCount);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_124: Lập bảng phạt
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC124_CreatePenalty_Succeeds()
    {
        var req = new PayrollPenaltyUpsertRequest(_employeeId, "Đi trễ 30 phút", "LateArrival", 100000m, null, "Đã xác nhận");
        var dto = await _svc.CreatePenaltyAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("Đi trễ 30 phút", dto.Reason);
        Assert.Equal(100000m, dto.Amount);
        Assert.Equal("Pending", dto.Status);
    }

    [Fact]
    public async Task UC124_CreatePenalty_EmployeeNotFound_ThrowsAppException()
    {
        var req = new PayrollPenaltyUpsertRequest(Guid.NewGuid(), "Vi phạm quy định", "RegulationBreach", 500000m);
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreatePenaltyAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC124_UpdatePenalty_AppliedStatus_ThrowsAppException()
    {
        var created = await _svc.CreatePenaltyAsync(_tenant, new PayrollPenaltyUpsertRequest(_employeeId, "Về sớm", "EarlyLeave", 50000m));
        await _svc.ApplyPenaltiesToPayrollAsync(_tenant, new ApplyPenaltyToPayrollRequest(_payrollPeriodId, new[] { created.Id }));

        var req = new PayrollPenaltyUpsertRequest(_employeeId, "Sửa số tiền", "EarlyLeave", 200000m);
        await Assert.ThrowsAsync<AppException>(() => _svc.UpdatePenaltyAsync(_tenant, created.Id, req));
    }

    [Fact]
    public async Task UC124_DeletePenalty_Succeeds()
    {
        var created = await _svc.CreatePenaltyAsync(_tenant, new PayrollPenaltyUpsertRequest(_employeeId, "Lỗi nhỏ", "Other", 20000m));
        await _svc.DeletePenaltyAsync(_tenant, created.Id);

        var list = await _svc.GetPenaltiesAsync(_tenant, _employeeId);
        Assert.Empty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_125: Áp dụng phạt vào kỳ lương
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC125_ApplyPenaltiesToPayroll_Succeeds()
    {
        var p1 = await _svc.CreatePenaltyAsync(_tenant, new PayrollPenaltyUpsertRequest(_employeeId, "Phạt 1", "LateArrival", 100000m));
        var p2 = await _svc.CreatePenaltyAsync(_tenant, new PayrollPenaltyUpsertRequest(_employeeId, "Phạt 2", "EarlyLeave", 200000m));

        var req = new ApplyPenaltyToPayrollRequest(_payrollPeriodId, new[] { p1.Id, p2.Id });
        var res = await _svc.ApplyPenaltiesToPayrollAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(2, res.TotalPenaltiesApplied);
        Assert.Equal(300000m, res.TotalDeductionAmount);
    }

    [Fact]
    public async Task UC125_ApplyPenaltiesToPayroll_PeriodNotFound_ThrowsAppException()
    {
        var p = await _svc.CreatePenaltyAsync(_tenant, new PayrollPenaltyUpsertRequest(_employeeId, "Phạt 3", "Other", 50000m));
        var req = new ApplyPenaltyToPayrollRequest(Guid.NewGuid(), new[] { p.Id });
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.ApplyPenaltiesToPayrollAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC125_ApplyPenaltiesToPayroll_EmptyPenalties_ThrowsAppException()
    {
        var req = new ApplyPenaltyToPayrollRequest(_payrollPeriodId, new List<Guid>());
        await Assert.ThrowsAsync<AppException>(() => _svc.ApplyPenaltiesToPayrollAsync(_tenant, req));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_174: Đồng bộ bút toán lương sang FIN
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC174_SyncPayrollJournalToFin_Succeeds()
    {
        _db.PayrollLines.Add(new PayrollLine
        {
            TenantId = _tenant,
            PayrollPeriodId = _payrollPeriodId,
            EmployeeId = _employeeId,
            GrossPay = 20000000m,
            NetPay = 18000000m
        });
        await _db.SaveChangesAsync();

        var req = new PayrollFinSyncRequest(_payrollPeriodId, "Đồng bộ bút toán kỳ 08/2026");
        var res = await _svc.SyncPayrollJournalToFinAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsBalanced);
        Assert.Equal(20000000m, res.TotalGrossSalaryAmount);
        Assert.Equal(18000000m, res.TotalNetSalaryAmount);
        Assert.StartsWith("JE-PY-2026-08-", res.JournalEntryCode);
    }

    [Fact]
    public async Task UC174_SyncPayrollJournalToFin_PeriodNotFound_ThrowsAppException()
    {
        var req = new PayrollFinSyncRequest(Guid.NewGuid());
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.SyncPayrollJournalToFinAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }
}
