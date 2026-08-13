using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class HrmLmsEvalCatalogPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmLmsEvalCatalogService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _evaluationCycleId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();

    public HrmLmsEvalCatalogPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-lms-eval-catalog-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T160", Name = "Tenant 160" });
        _db.Employees.Add(new Employee
        {
            Id = _employeeId,
            TenantId = _tenant,
            EmployeeCode = "EMP160",
            FullName = "Vũ Thị I",
            Status = "Active"
        });
        _db.HrmEvaluationCycles.Add(new HrmEvaluationCycle
        {
            Id = _evaluationCycleId,
            TenantId = _tenant,
            CycleName = "Kỳ đánh giá Năm 2026",
            PeriodKey = "2026-FULL",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            Status = "Active"
        });
        _db.HrmManagerEvaluations.Add(new HrmManagerEvaluation
        {
            TenantId = _tenant,
            EvaluationCycleId = _evaluationCycleId,
            EmployeeId = _employeeId,
            EvaluatorId = _employeeId,
            KpiScore = 90m,
            CompetencyScore = 80m,
            FinalGrade = "A",
            Status = "Completed"
        });

        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _courseId,
            TenantId = _tenant,
            Name = "Khóa học Lập trình C# .NET nâng cao",
            Code = "CRS_DOTNET_ADV"
        });
        _db.LmsQuestions.Add(new LmsQuestion
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant,
            Code = "Q001",
            Stem = "Async/Await trong C# hoạt động như thế nào?"
        });

        _db.SaveChanges();

        _svc = new HrmLmsEvalCatalogService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_181: Tổng hợp kết quả đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC181_GetEvaluationSummaryReport_Succeeds()
    {
        var rpt = await _svc.GetEvaluationSummaryReportAsync(_tenant, _evaluationCycleId);

        Assert.NotNull(rpt);
        Assert.Equal(1, rpt.TotalEvaluatedCount);
        Assert.Equal(90m, rpt.AverageKpiScore);
        Assert.Equal(80m, rpt.AverageCompetencyScore);
        Assert.Equal(4, rpt.GradeDistributions.Count);
        Assert.Equal(100m, rpt.GradeDistributions.First(g => g.Grade == "A").Percentage);
    }

    [Fact]
    public async Task UC181_GetEvaluationSummaryReport_EmptyEvaluations_ReturnsZeroReport()
    {
        var emptyCycleId = Guid.NewGuid();
        _db.HrmEvaluationCycles.Add(new HrmEvaluationCycle
        {
            Id = emptyCycleId,
            TenantId = _tenant,
            CycleName = "Kỳ rỗng",
            PeriodKey = "2026-EMPTY",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 31)
        });
        await _db.SaveChangesAsync();

        var rpt = await _svc.GetEvaluationSummaryReportAsync(_tenant, emptyCycleId);
        Assert.Equal(0, rpt.TotalEvaluatedCount);
        Assert.Equal(0m, rpt.AverageKpiScore);
    }

    [Fact]
    public async Task UC181_GetEvaluationSummaryReport_CycleNotFound_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.GetEvaluationSummaryReportAsync(_tenant, Guid.NewGuid()));
        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_007: Gắn tag kỹ năng / vị trí
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC007_CreateCourseSkillTag_Succeeds()
    {
        var req = new LmsCourseSkillTagUpsertRequest(_courseId, "C#", "Skill");
        var dto = await _svc.CreateCourseSkillTagAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("C#", dto.TagName);
        Assert.Equal("Skill", dto.TagType);
    }

    [Fact]
    public async Task UC007_CreateCourseSkillTag_CourseNotFound_ThrowsAppException()
    {
        var req = new LmsCourseSkillTagUpsertRequest(Guid.NewGuid(), "Dotnet", "Skill");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateCourseSkillTagAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC007_DeleteCourseSkillTag_Succeeds()
    {
        var created = await _svc.CreateCourseSkillTagAsync(_tenant, new LmsCourseSkillTagUpsertRequest(_courseId, "Backend", "Position"));
        await _svc.DeleteCourseSkillTagAsync(_tenant, created.Id);

        var list = await _svc.GetCourseSkillTagsAsync(_tenant, _courseId);
        Assert.DoesNotContain(list, x => x.Id == created.Id);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_008: Phiên bản nội dung khóa học
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC008_CreateCourseVersion_Succeeds()
    {
        var req = new LmsCourseVersionUpsertRequest(_courseId, "2.0", "Cập nhật tài liệu .NET 8");
        var dto = await _svc.CreateCourseVersionAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("2.0", dto.VersionNumber);
        Assert.Equal("Cập nhật tài liệu .NET 8", dto.Changelog);
    }

    [Fact]
    public async Task UC008_CreateCourseVersion_CourseNotFound_ThrowsAppException()
    {
        var req = new LmsCourseVersionUpsertRequest(Guid.NewGuid(), "1.0");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateCourseVersionAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC008_GetCourseVersions_ReturnsList()
    {
        await _svc.CreateCourseVersionAsync(_tenant, new LmsCourseVersionUpsertRequest(_courseId, "1.1", "Sửa bài tập 1"));
        var list = await _svc.GetCourseVersionsAsync(_tenant, _courseId);

        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_013: Tạo đề thi random
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC013_GenerateRandomExam_Succeeds()
    {
        var req = new LmsRandomExamRequest(_courseId, "Kiểm tra Cuối khóa .NET", 5, 80m, 60);
        var res = await _svc.GenerateRandomExamAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Kiểm tra Cuối khóa .NET", res.ExamTitle);
        Assert.Equal(1, res.SelectedQuestionCount); // Chỉ có 1 câu trong database
    }

    [Fact]
    public async Task UC013_GenerateRandomExam_CourseNotFound_ThrowsAppException()
    {
        var req = new LmsRandomExamRequest(Guid.NewGuid(), "Đề thi lỗi");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.GenerateRandomExamAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC013_GenerateRandomExam_EmptyQuestions_ReturnsZeroSelected()
    {
        var emptyTenant = Guid.NewGuid();
        _db.Tenants.Add(new Tenant { Id = emptyTenant, Code = "T_EMP", Name = "Tenant Empty" });
        var emptyCourseId = Guid.NewGuid();
        _db.LmsCourses.Add(new LmsCourse { Id = emptyCourseId, TenantId = emptyTenant, Name = "Khóa học rỗng", Code = "CRS_EMPTY" });
        await _db.SaveChangesAsync();

        var req = new LmsRandomExamRequest(emptyCourseId, "Đề thi rỗng", 10);
        var res = await _svc.GenerateRandomExamAsync(emptyTenant, req);

        Assert.Equal(0, res.SelectedQuestionCount);
    }
}
