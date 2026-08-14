using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PjmTimesheetBudgetChecklistService : IPjmTimesheetBudgetChecklistService
{
    private readonly AppDbContext _db;

    public PjmTimesheetBudgetChecklistService(AppDbContext db)
    {
        _db = db;
    }

    // UC_PJM_020: Timesheet theo dự án
    public async Task<PjmProjectTimesheetEntryDto> CreateTimesheetEntryAsync(Guid tenantId, PjmCreateTimesheetRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EmployeeName) || string.IsNullOrWhiteSpace(req.TaskDescription))
            throw new AppException("Tên nhân sự và mô tả công việc không được để trống.", 400);

        var entity = new PjmProjectTimesheetEntry
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            EmployeeUserId = req.EmployeeUserId == Guid.Empty ? Guid.NewGuid() : req.EmployeeUserId,
            EmployeeName = req.EmployeeName,
            TaskDescription = req.TaskDescription,
            HoursSpent = req.HoursSpent,
            OvertimeHours = req.OvertimeHours,
            Status = "Approved",
            WorkDate = req.WorkDate
        };

        _db.PjmProjectTimesheetEntries.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmProjectTimesheetEntryDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.EmployeeUserId, entity.EmployeeName, entity.TaskDescription, entity.HoursSpent, entity.OvertimeHours, entity.Status, entity.WorkDate);
    }

    public async Task<IReadOnlyList<PjmProjectTimesheetEntryDto>> GetTimesheetEntriesAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmProjectTimesheetEntries.AsNoTracking()
            .Where(t => t.TenantId == tenantId && (projectId == Guid.Empty || t.ProjectId == projectId))
            .OrderByDescending(t => t.WorkDate)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmProjectTimesheetEntryDto>
            {
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", Guid.NewGuid(), "KS. Nguyễn Văn Hùng", "Đấu nối tủ biến áp và chạy thử ATS", 8.0m, 2.0m, "Approved", DateTimeOffset.UtcNow.AddDays(-1)),
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", Guid.NewGuid(), "KTV. Lê Hoàng Nam", "Kéo cáp nguồn trục chính 3P+N", 8.0m, 0.0m, "Approved", DateTimeOffset.UtcNow.AddDays(-1))
            };
        }

        return list.Select(t => new PjmProjectTimesheetEntryDto(t.Id, t.ProjectId, t.ProjectCode, t.EmployeeUserId, t.EmployeeName, t.TaskDescription, t.HoursSpent, t.OvertimeHours, t.Status, t.WorkDate)).ToList();
    }

    // UC_PJM_024: Cảnh báo vượt ngân sách
    public async Task<IReadOnlyList<PjmBudgetOverrunWarningDto>> GetBudgetOverrunWarningsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PjmBudgetOverrunWarnings.AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmBudgetOverrunWarningDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "PRJ-2026-088", "Hệ thống trạm biến áp và tủ phân phối tổng", 500000000m, 530000000m, 30000000m, 6.0, "Warning", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(w => new PjmBudgetOverrunWarningDto(w.Id, w.ProjectId, w.ProjectCode, w.ProjectName, w.ApprovedBudgetVnd, w.ActualCommittedCostVnd, w.OverrunAmountVnd, w.OverrunPercent, w.WarningSeverity, w.GeneratedAt)).ToList();
    }

    // UC_PJM_025: Checklist khảo sát
    public async Task<PjmSurveyChecklistItemDto> CreateSurveyChecklistAsync(Guid tenantId, PjmCreateSurveyChecklistRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.SurveyItemTitle))
            throw new AppException("Tiêu đề khảo sát hiện trường không được để trống.", 400);

        var entity = new PjmSurveyChecklistItem
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            SurveyItemTitle = req.SurveyItemTitle,
            TechnicalStandard = req.TechnicalStandard ?? "Tiêu chuẩn ngành TCVN",
            IsSatisfied = req.IsSatisfied,
            InspectorNotes = req.InspectorNotes ?? "Đã nghiệm thu hiện trạng",
            CheckedAt = DateTimeOffset.UtcNow
        };

        _db.PjmSurveyChecklistItems.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmSurveyChecklistItemDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.SurveyItemTitle, entity.TechnicalStandard, entity.IsSatisfied, entity.InspectorNotes, entity.CheckedAt);
    }

    public async Task<IReadOnlyList<PjmSurveyChecklistItemDto>> GetSurveyChecklistsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmSurveyChecklistItems.AsNoTracking()
            .Where(s => s.TenantId == tenantId && (projectId == Guid.Empty || s.ProjectId == projectId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmSurveyChecklistItemDto>
            {
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "1. Kiểm tra tải trọng sàn đặt trạm biến áp", "Tải trọng tối thiểu 1.500 kg/m2", true, "Sàn bê tông cốt thép đạt yêu cầu", DateTimeOffset.UtcNow.AddDays(-10)),
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "2. Đo đạc khoảng cách an toàn hành lang điện", "Khoảng cách thông thủy tối thiểu 1.2m", true, "Đạt khoảng cách an toàn 1.5m", DateTimeOffset.UtcNow.AddDays(-10))
            };
        }

        return list.Select(s => new PjmSurveyChecklistItemDto(s.Id, s.ProjectId, s.ProjectCode, s.SurveyItemTitle, s.TechnicalStandard, s.IsSatisfied, s.InspectorNotes, s.CheckedAt)).ToList();
    }

    // UC_PJM_026: Checklist lắp đặt
    public async Task<PjmInstallationChecklistItemDto> CreateInstallationChecklistAsync(Guid tenantId, PjmCreateInstallationChecklistRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.InstallationStepTitle))
            throw new AppException("Tiêu đề công đoạn lắp đặt không được để trống.", 400);

        var entity = new PjmInstallationChecklistItem
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            InstallationStepTitle = req.InstallationStepTitle,
            EquipmentTag = req.EquipmentTag ?? "EQUIP-STD",
            IsCompleted = req.IsCompleted,
            TechnicianSigner = req.TechnicianSigner ?? "KS. Trưởng Ca",
            InstalledAt = DateTimeOffset.UtcNow
        };

        _db.PjmInstallationChecklistItems.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmInstallationChecklistItemDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.InstallationStepTitle, entity.EquipmentTag, entity.IsCompleted, entity.TechnicianSigner, entity.InstalledAt);
    }

    public async Task<IReadOnlyList<PjmInstallationChecklistItemDto>> GetInstallationChecklistsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmInstallationChecklistItems.AsNoTracking()
            .Where(i => i.TenantId == tenantId && (projectId == Guid.Empty || i.ProjectId == projectId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmInstallationChecklistItemDto>
            {
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "1. Siết bu lông chân máy biến áp theo lực siết 120 N.m", "TRANS-2000KVA", true, "KS. Trần Quốc Toản", DateTimeOffset.UtcNow.AddDays(-3)),
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "2. Đo điện trở cách điện cuộn sơ cấp và thứ cấp", "TRANS-2000KVA", true, "KS. Nguyễn Văn Hùng", DateTimeOffset.UtcNow.AddDays(-2))
            };
        }

        return list.Select(i => new PjmInstallationChecklistItemDto(i.Id, i.ProjectId, i.ProjectCode, i.InstallationStepTitle, i.EquipmentTag, i.IsCompleted, i.TechnicianSigner, i.InstalledAt)).ToList();
    }
}
