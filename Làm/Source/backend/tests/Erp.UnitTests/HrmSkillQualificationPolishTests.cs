using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class HrmSkillQualificationPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmSkillQualificationService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _orgUnit = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _jobPostingId = Guid.NewGuid();
    private readonly Guid _contractId = Guid.NewGuid();

    public HrmSkillQualificationPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-skill-qualification-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T157", Name = "Tenant 157" });
        _db.OrgUnits.Add(new OrgUnit { Id = _orgUnit, TenantId = _tenant, Code = "OU1", Name = "Chi nhánh TP.HCM" });
        _db.Employees.Add(new Employee
        {
            Id = _employeeId,
            TenantId = _tenant,
            EmployeeCode = "EMP157",
            FullName = "Lê Văn C",
            OrgUnitId = _orgUnit,
            Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15))
        });
        _db.JobPostings.Add(new JobPosting { Id = _jobPostingId, TenantId = _tenant, Title = "Kỹ sư Backend .NET" });
        _db.Contracts.Add(new Contract
        {
            Id = _contractId,
            TenantId = _tenant,
            EmployeeId = _employeeId,
            ContractNo = "HD-2026-001",
            ContractType = "Indefinite",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            BaseSalary = 25000000m,
            Status = "Active"
        });

        _db.SaveChanges();

        _svc = new HrmSkillQualificationService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_024: Quản lý trình độ / kỹ năng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC024_CreateSkill_Succeeds()
    {
        var req = new HrmEmployeeSkillUpsertRequest(_employeeId, "C# .NET Core", "Expert", "Cert-001");
        var dto = await _svc.CreateSkillAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("C# .NET Core", dto.SkillName);
        Assert.Equal("Expert", dto.ProficiencyLevel);
        Assert.Equal("Cert-001", dto.CertificateRef);
    }

    [Fact]
    public async Task UC024_CreateSkill_EmployeeNotFound_ThrowsAppException()
    {
        var req = new HrmEmployeeSkillUpsertRequest(Guid.NewGuid(), "ReactJS", "Advanced", null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateSkillAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC024_UpdateSkill_Succeeds()
    {
        var created = await _svc.CreateSkillAsync(_tenant, new HrmEmployeeSkillUpsertRequest(_employeeId, "SQL", "Basic", null));
        var updated = await _svc.UpdateSkillAsync(_tenant, created.Id, new HrmEmployeeSkillUpsertRequest(_employeeId, "SQL Server Tuning", "Advanced", "Cert-SQL"));

        Assert.Equal("SQL Server Tuning", updated.SkillName);
        Assert.Equal("Advanced", updated.ProficiencyLevel);
    }

    [Fact]
    public async Task UC024_DeleteSkill_Succeeds()
    {
        var created = await _svc.CreateSkillAsync(_tenant, new HrmEmployeeSkillUpsertRequest(_employeeId, "Docker", "Intermediate", null));
        await _svc.DeleteSkillAsync(_tenant, created.Id);

        var list = await _svc.GetSkillsAsync(_tenant, _employeeId);
        Assert.Empty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_037: Báo cáo biến động nhân sự
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC037_GetPersonnelMovementReport_CalculatesStatsAndTurnoverCorrectly()
    {
        var filter = new HrmPersonnelMovementFilter(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        var rpt = await _svc.GetPersonnelMovementReportAsync(_tenant, filter);

        Assert.NotNull(rpt);
        Assert.Equal(1, rpt.TotalEmployees);
        Assert.Equal(1, rpt.ActiveCount);
        Assert.Equal(1, rpt.JoinersInPeriod);
        Assert.Equal(0, rpt.LeaversInPeriod);
        Assert.Equal(0m, rpt.TurnoverRatePercentage);
    }

    [Fact]
    public async Task UC037_GetPersonnelMovementReport_FiltersByOrgUnit_Succeeds()
    {
        var filter = new HrmPersonnelMovementFilter(OrgUnitId: _orgUnit);
        var rpt = await _svc.GetPersonnelMovementReportAsync(_tenant, filter);

        Assert.Equal(1, rpt.TotalEmployees);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_044: In / xuất mẫu hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC044_PrintContractTemplate_Succeeds()
    {
        var req = new HrmContractExportRequest(_contractId, "Standard");
        var dto = await _svc.PrintContractTemplateAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("HD-2026-001", dto.ContractNo);
        Assert.Equal("Lê Văn C", dto.EmployeeName);
        Assert.Contains("HỢP ĐỒNG LAO ĐỘNG", dto.FormattedTemplateText);
    }

    [Fact]
    public async Task UC044_PrintContractTemplate_NotFound_ThrowsAppException()
    {
        var req = new HrmContractExportRequest(Guid.NewGuid());
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.PrintContractTemplateAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC044_ExportContractText_ReturnsNonEmptyBytes()
    {
        var req = new HrmContractExportRequest(_contractId);
        var bytes = await _svc.ExportContractTextAsync(_tenant, req);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_058: Import ứng viên hàng loạt
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC058_ImportCandidatesBulk_Succeeds()
    {
        var items = new List<HrmBulkCandidateImportItem>
        {
            new("Phạm Văn D", "d.pham@example.com", "0911223344", _jobPostingId),
            new("Hoàng Thị E", "e.hoang@example.com", "0955667788", _jobPostingId)
        };

        var result = await _svc.ImportCandidatesBulkAsync(_tenant, items);

        Assert.Equal(2, result.TotalProcessed);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(2, result.ImportedCandidateIds.Count);
    }

    [Fact]
    public async Task UC058_ImportCandidatesBulk_EmptyList_ThrowsAppException()
    {
        await Assert.ThrowsAsync<AppException>(() => _svc.ImportCandidatesBulkAsync(_tenant, new List<HrmBulkCandidateImportItem>()));
    }

    [Fact]
    public async Task UC058_ImportCandidatesBulk_InvalidPostingOrMissingName_ReturnsErrors()
    {
        var items = new List<HrmBulkCandidateImportItem>
        {
            new("", "valid@example.com", "0911111111", _jobPostingId), // missing name
            new("Trần Văn F", "f@example.com", "0922222222", Guid.NewGuid()) // invalid job posting
        };

        var result = await _svc.ImportCandidatesBulkAsync(_tenant, items);

        Assert.Equal(2, result.TotalProcessed);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(2, result.FailedCount);
        Assert.Equal(2, result.Errors.Count);
    }
}
