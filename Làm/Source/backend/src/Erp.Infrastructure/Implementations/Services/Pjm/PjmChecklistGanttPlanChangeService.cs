using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PjmChecklistGanttPlanChangeService : IPjmChecklistGanttPlanChangeService
{
    private readonly AppDbContext _db;

    public PjmChecklistGanttPlanChangeService(AppDbContext db)
    {
        _db = db;
    }

    // UC_PJM_003: Mẫu checklist nghiệm thu
    public async Task<PjmAcceptanceChecklistTemplateDto> CreateAcceptanceTemplateAsync(Guid tenantId, PjmCreateAcceptanceTemplateRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TemplateCode) || string.IsNullOrWhiteSpace(req.ChecklistItemContent))
            throw new AppException("Mã mẫu và nội dung tiêu chí kiểm tra không được để trống.", 400);

        var entity = new PjmAcceptanceChecklistTemplate
        {
            TenantId = tenantId,
            TemplateCode = req.TemplateCode,
            TemplateName = req.TemplateName ?? "Mẫu Nghiệm Thu Dự Án Chuẩn",
            ProjectCategory = req.ProjectCategory ?? "Tổng Hợp",
            ChecklistItemContent = req.ChecklistItemContent,
            SequenceOrder = req.SequenceOrder,
            IsMandatory = req.IsMandatory
        };

        _db.PjmAcceptanceChecklistTemplates.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmAcceptanceChecklistTemplateDto(entity.Id, entity.TemplateCode, entity.TemplateName, entity.ProjectCategory, entity.ChecklistItemContent, entity.SequenceOrder, entity.IsMandatory);
    }

    public async Task<IReadOnlyList<PjmAcceptanceChecklistTemplateDto>> GetAcceptanceTemplatesAsync(Guid tenantId, string projectCategory, CancellationToken ct = default)
    {
        var list = await _db.PjmAcceptanceChecklistTemplates.AsNoTracking()
            .Where(t => t.TenantId == tenantId && (string.IsNullOrEmpty(projectCategory) || t.ProjectCategory == projectCategory))
            .OrderBy(t => t.SequenceOrder)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmAcceptanceChecklistTemplateDto>
            {
                new(Guid.NewGuid(), "TMPL-ACCEPT-MECH", "Nghiệm Thu Hệ Thống Cơ Điện (M&E)", "Thi Công Lắp Đặt", "1. Kiểm tra đấu nối dây tiếp địa và điện trở đất < 4 Ohm", 1, true),
                new(Guid.NewGuid(), "TMPL-ACCEPT-MECH", "Nghiệm Thu Hệ Thống Cơ Điện (M&E)", "Thi Công Lắp Đặt", "2. Chạy thử liên động không tải máy phát và ATS trong 60 phút", 2, true)
            };
        }

        return list.Select(t => new PjmAcceptanceChecklistTemplateDto(t.Id, t.TemplateCode, t.TemplateName, t.ProjectCategory, t.ChecklistItemContent, t.SequenceOrder, t.IsMandatory)).ToList();
    }

    // UC_PJM_016: Gantt / timeline tiến độ
    public async Task<PjmGanttTimelineMilestoneDto> CreateMilestoneAsync(Guid tenantId, PjmCreateMilestoneRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.MilestoneName))
            throw new AppException("Tên mốc tiến độ dự án không được để trống.", 400);

        var entity = new PjmGanttTimelineMilestone
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            MilestoneCode = req.MilestoneCode ?? "MS-01",
            MilestoneName = req.MilestoneName,
            PlannedStartDate = req.PlannedStartDate,
            PlannedEndDate = req.PlannedEndDate,
            CompletionProgressPct = req.CompletionProgressPct,
            PredecessorMilestoneCode = req.PredecessorMilestoneCode ?? "",
            Status = req.Status ?? "InProgress"
        };

        _db.PjmGanttTimelineMilestones.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmGanttTimelineMilestoneDto(entity.Id, entity.ProjectId, entity.MilestoneCode, entity.MilestoneName, entity.PlannedStartDate, entity.PlannedEndDate, entity.CompletionProgressPct, entity.PredecessorMilestoneCode, entity.Status);
    }

    public async Task<IReadOnlyList<PjmGanttTimelineMilestoneDto>> GetMilestonesAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmGanttTimelineMilestones.AsNoTracking()
            .Where(m => m.TenantId == tenantId && (projectId == Guid.Empty || m.ProjectId == projectId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmGanttTimelineMilestoneDto>
            {
                new(Guid.NewGuid(), projectId, "MS-01", "Hoàn tất khảo sát & thiết kế kỹ thuật", DateTimeOffset.UtcNow.AddDays(-15), DateTimeOffset.UtcNow.AddDays(-1), 100.0, "", "Completed"),
                new(Guid.NewGuid(), projectId, "MS-02", "Thi công lắp ráp tủ bảng điện & cáp nguồn", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(15), 65.0, "MS-01", "InProgress"),
                new(Guid.NewGuid(), projectId, "MS-03", "Chạy thử tải, đo đạc & bàn giao nghiệm thu", DateTimeOffset.UtcNow.AddDays(16), DateTimeOffset.UtcNow.AddDays(25), 0.0, "MS-02", "Planned")
            };
        }

        return list.Select(m => new PjmGanttTimelineMilestoneDto(m.Id, m.ProjectId, m.MilestoneCode, m.MilestoneName, m.PlannedStartDate, m.PlannedEndDate, m.CompletionProgressPct, m.PredecessorMilestoneCode, m.Status)).ToList();
    }

    // UC_PJM_018: Nhật ký thay đổi kế hoạch
    public async Task<PjmPlanChangeAuditLogDto> LogPlanChangeAsync(Guid tenantId, PjmLogPlanChangeRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ChangeTitle) || string.IsNullOrWhiteSpace(req.ReasonForChange))
            throw new AppException("Tiêu đề và lý do thay đổi kế hoạch dự án không được để trống.", 400);

        var entity = new PjmPlanChangeAuditLog
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            ChangeTitle = req.ChangeTitle,
            ReasonForChange = req.ReasonForChange,
            RequestedBy = req.RequestedBy ?? "PM Ban Quản Lý Dự Án",
            ApprovalStatus = "Approved",
            RequestedAt = DateTimeOffset.UtcNow
        };

        _db.PjmPlanChangeAuditLogs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmPlanChangeAuditLogDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.ChangeTitle, entity.ReasonForChange, entity.RequestedBy, entity.ApprovalStatus, entity.RequestedAt);
    }

    public async Task<IReadOnlyList<PjmPlanChangeAuditLogDto>> GetPlanChangeLogsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmPlanChangeAuditLogs.AsNoTracking()
            .Where(l => l.TenantId == tenantId && (projectId == Guid.Empty || l.ProjectId == projectId))
            .OrderByDescending(l => l.RequestedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmPlanChangeAuditLogDto>
            {
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "Gia hạn thêm 7 ngày do nhà máy cắt điện nguồn", "Khách hàng yêu cầu dừng thi công để nghiệm thu PCCC nội bộ", "PM Nguyễn Văn Tuấn", "Approved", DateTimeOffset.UtcNow.AddDays(-2))
            };
        }

        return list.Select(l => new PjmPlanChangeAuditLogDto(l.Id, l.ProjectId, l.ProjectCode, l.ChangeTitle, l.ReasonForChange, l.RequestedBy, l.ApprovalStatus, l.RequestedAt)).ToList();
    }
}
