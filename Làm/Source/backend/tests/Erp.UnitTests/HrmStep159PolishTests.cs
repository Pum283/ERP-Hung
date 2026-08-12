using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class HrmStep159PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmStep159Service _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _evaluatorId = Guid.NewGuid();
    private readonly Guid _kpiTemplateId = Guid.NewGuid();
    private readonly Guid _evaluationCycleId = Guid.NewGuid();

    public HrmStep159PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step159-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T159", Name = "Tenant 159" });
        _db.Employees.Add(new Employee
        {
            Id = _employeeId,
            TenantId = _tenant,
            EmployeeCode = "EMP159",
            FullName = "Nguyễn Văn G",
            Status = "Active"
        });
        _db.Employees.Add(new Employee
        {
            Id = _evaluatorId,
            TenantId = _tenant,
            EmployeeCode = "MGR159",
            FullName = "Trần Thị H",
            Status = "Active"
        });
        _db.HrmKpiTemplates.Add(new HrmKpiTemplate
        {
            Id = _kpiTemplateId,
            TenantId = _tenant,
            Code = "TMPL_DEV_2026",
            Title = "Đánh giá Kỹ sư phần mềm 2026",
            TargetRole = "Software Engineer",
            MaxScore = 100m,
            WeightPercentage = 100m
        });
        _db.HrmEvaluationCycles.Add(new HrmEvaluationCycle
        {
            Id = _evaluationCycleId,
            TenantId = _tenant,
            CycleName = "Kỳ đánh giá Quý 3/2026",
            PeriodKey = "2026-Q3",
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 9, 30),
            KpiTemplateId = _kpiTemplateId,
            Status = "Active"
        });

        _db.SaveChanges();

        _svc = new HrmStep159Service(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_177: Mẫu đánh giá KPI / năng lực
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC177_CreateKpiTemplate_Succeeds()
    {
        var req = new HrmKpiTemplateUpsertRequest("TMPL_SALES_2026", "Đánh giá Kinh doanh 2026", "Sales Executive", "Doanh số & CSKH", 100m, 100m);
        var dto = await _svc.CreateKpiTemplateAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("TMPL_SALES_2026", dto.Code);
        Assert.Equal("Đánh giá Kinh doanh 2026", dto.Title);
    }

    [Fact]
    public async Task UC177_CreateKpiTemplate_DuplicateCode_ThrowsAppException()
    {
        var req = new HrmKpiTemplateUpsertRequest("TMPL_DEV_2026", "Trùng mã", null, "", 100m, 100m);
        await Assert.ThrowsAsync<AppException>(() => _svc.CreateKpiTemplateAsync(_tenant, req));
    }

    [Fact]
    public async Task UC177_UpdateKpiTemplate_Succeeds()
    {
        var req = new HrmKpiTemplateUpsertRequest("TMPL_DEV_2026_V2", "Đánh giá Kỹ sư V2", "Senior Engineer", "Nâng cấp tiêu chuẩn", 100m, 100m);
        var dto = await _svc.UpdateKpiTemplateAsync(_tenant, _kpiTemplateId, req);

        Assert.Equal("TMPL_DEV_2026_V2", dto.Code);
        Assert.Equal("Đánh giá Kỹ sư V2", dto.Title);
    }

    [Fact]
    public async Task UC177_DeleteKpiTemplate_Succeeds()
    {
        var created = await _svc.CreateKpiTemplateAsync(_tenant, new HrmKpiTemplateUpsertRequest("TMPL_TEMP", "Tạm thời"));
        await _svc.DeleteKpiTemplateAsync(_tenant, created.Id);

        var list = await _svc.GetKpiTemplatesAsync(_tenant);
        Assert.DoesNotContain(list, x => x.Id == created.Id);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_178: Tạo kỳ đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC178_CreateEvaluationCycle_Succeeds()
    {
        var req = new HrmEvaluationCycleUpsertRequest(
            "Kỳ đánh giá Năm 2026",
            "2026-FULL",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            _kpiTemplateId,
            "Draft"
        );

        var dto = await _svc.CreateEvaluationCycleAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("Kỳ đánh giá Năm 2026", dto.CycleName);
        Assert.Equal("2026-FULL", dto.PeriodKey);
    }

    [Fact]
    public async Task UC178_CreateEvaluationCycle_InvalidDateRange_ThrowsAppException()
    {
        var req = new HrmEvaluationCycleUpsertRequest(
            "Sai ngày",
            "2026-ERR",
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 1, 1)
        );

        await Assert.ThrowsAsync<AppException>(() => _svc.CreateEvaluationCycleAsync(_tenant, req));
    }

    [Fact]
    public async Task UC178_UpdateEvaluationCycle_Succeeds()
    {
        var req = new HrmEvaluationCycleUpsertRequest(
            "Kỳ đánh giá Q3/2026 (Cập nhật)",
            "2026-Q3-REV",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 9, 30),
            _kpiTemplateId,
            "Active"
        );

        var dto = await _svc.UpdateEvaluationCycleAsync(_tenant, _evaluationCycleId, req);
        Assert.Equal("Kỳ đánh giá Q3/2026 (Cập nhật)", dto.CycleName);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_179: Quản lý đánh giá nhân viên
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC179_CreateManagerEvaluation_Succeeds()
    {
        var req = new HrmManagerEvaluationUpsertRequest(
            _evaluationCycleId,
            _employeeId,
            _evaluatorId,
            92m,
            88m,
            "A",
            "Hoàn thành xuất sắc nhiệm vụ",
            "Completed"
        );

        var dto = await _svc.CreateManagerEvaluationAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal(92m, dto.KpiScore);
        Assert.Equal("A", dto.FinalGrade);
        Assert.Equal("Completed", dto.Status);
    }

    [Fact]
    public async Task UC179_CreateManagerEvaluation_CycleNotFound_ThrowsAppException()
    {
        var req = new HrmManagerEvaluationUpsertRequest(Guid.NewGuid(), _employeeId, _evaluatorId);
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateManagerEvaluationAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC179_UpdateManagerEvaluation_Succeeds()
    {
        var created = await _svc.CreateManagerEvaluationAsync(_tenant, new HrmManagerEvaluationUpsertRequest(_evaluationCycleId, _employeeId, _evaluatorId, 75m, 70m, "B"));

        var req = new HrmManagerEvaluationUpsertRequest(_evaluationCycleId, _employeeId, _evaluatorId, 85m, 80m, "A", "Tăng điểm sau phúc khảo", "Completed");
        var updated = await _svc.UpdateManagerEvaluationAsync(_tenant, created.Id, req);

        Assert.Equal(85m, updated.KpiScore);
        Assert.Equal("A", updated.FinalGrade);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_180: Nhân viên tự đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC180_CreateSelfEvaluation_Succeeds()
    {
        var req = new HrmSelfEvaluationUpsertRequest(
            _employeeId,
            "2026-Q3",
            "Hoàn thành module HRM Step 159 đúng hạn",
            "Cần cải thiện kỹ năng quản lý thời gian",
            5,
            "Submitted"
        );

        var dto = await _svc.CreateSelfEvaluationAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("2026-Q3", dto.AppraisalPeriod);
        Assert.Equal(5, dto.SelfRating);
        Assert.Equal("Submitted", dto.Status);
    }

    [Fact]
    public async Task UC180_CreateSelfEvaluation_InvalidRating_ThrowsAppException()
    {
        var req = new HrmSelfEvaluationUpsertRequest(_employeeId, "2026-Q3", "Đánh giá 10 sao", "", 10, "Submitted");
        await Assert.ThrowsAsync<AppException>(() => _svc.CreateSelfEvaluationAsync(_tenant, req));
    }
}
