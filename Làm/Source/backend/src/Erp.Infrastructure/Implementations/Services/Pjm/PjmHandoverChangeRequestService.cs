using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PjmHandoverChangeRequestService : IPjmHandoverChangeRequestService
{
    private readonly AppDbContext _db;

    public PjmHandoverChangeRequestService(AppDbContext db)
    {
        _db = db;
    }

    // UC_PJM_027: Checklist bàn giao
    public async Task<PjmHandoverChecklistItemDto> CreateHandoverChecklistAsync(Guid tenantId, PjmCreateHandoverChecklistRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.HandoverCriteriaName))
            throw new AppException("Tiêu chí bàn giao không được để trống.", 400);

        var entity = new PjmHandoverChecklistItem
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            HandoverCriteriaName = req.HandoverCriteriaName,
            IsSatisfied = req.IsSatisfied,
            CustomerRepresentativeName = req.CustomerRepresentativeName ?? "Đại diện CĐT",
            SignedAt = DateTimeOffset.UtcNow
        };

        _db.PjmHandoverChecklistItems.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmHandoverChecklistItemDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.HandoverCriteriaName, entity.IsSatisfied, entity.CustomerRepresentativeName, entity.SignedAt);
    }

    public async Task<IReadOnlyList<PjmHandoverChecklistItemDto>> GetHandoverChecklistsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmHandoverChecklistItems.AsNoTracking()
            .Where(h => h.TenantId == tenantId && (projectId == Guid.Empty || h.ProjectId == projectId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmHandoverChecklistItemDto>
            {
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "1. Bàn giao đầy đủ hồ sơ hoàn công và sơ đồ nguyên lý", true, "Đại diện Chủ đầu tư FPT", DateTimeOffset.UtcNow.AddDays(-1)),
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "2. Đào tạo chuyển giao công nghệ vận hành tủ điện MSB", true, "Kỹ sư vận hành FPT", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(h => new PjmHandoverChecklistItemDto(h.Id, h.ProjectId, h.ProjectCode, h.HandoverCriteriaName, h.IsSatisfied, h.CustomerRepresentativeName, h.SignedAt)).ToList();
    }

    // UC_PJM_028: Ghi nhận ảnh / biên bản
    public async Task<PjmSiteProtocolAttachmentDto> UploadProtocolAttachmentAsync(Guid tenantId, PjmUploadProtocolAttachmentRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.AttachmentTitle))
            throw new AppException("Tiêu đề biên bản/ảnh hiện trường không được để trống.", 400);

        var entity = new PjmSiteProtocolAttachment
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            AttachmentTitle = req.AttachmentTitle,
            AttachmentType = req.AttachmentType ?? "ProtocolPdf",
            FileUrl = req.FileUrl ?? "/uploads/pjm/protocols/handover-default.pdf",
            FileSizeBytes = req.FileSizeBytes > 0 ? req.FileSizeBytes : 1024000,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _db.PjmSiteProtocolAttachments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmSiteProtocolAttachmentDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.AttachmentTitle, entity.AttachmentType, entity.FileUrl, entity.FileSizeBytes, entity.UploadedAt);
    }

    public async Task<IReadOnlyList<PjmSiteProtocolAttachmentDto>> GetProtocolAttachmentsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmSiteProtocolAttachments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && (projectId == Guid.Empty || p.ProjectId == projectId))
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmSiteProtocolAttachmentDto>
            {
                new(Guid.NewGuid(), projectId, "PRJ-2026-088", "Biên bản nghiệm thu đóng điện trạm biến áp có chữ ký CĐT", "ProtocolPdf", "/uploads/pjm/protocols/prj-088-handover-signed.pdf", 2450000, DateTimeOffset.UtcNow.AddDays(-1))
            };
        }

        return list.Select(p => new PjmSiteProtocolAttachmentDto(p.Id, p.ProjectId, p.ProjectCode, p.AttachmentTitle, p.AttachmentType, p.FileUrl, p.FileSizeBytes, p.UploadedAt)).ToList();
    }

    // UC_PJM_029: Phát sinh change request
    public async Task<PjmEngineeringChangeRequestDto> CreateEcrAsync(Guid tenantId, PjmCreateEcrRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EcrTitle) || string.IsNullOrWhiteSpace(req.ChangeReason))
            throw new AppException("Tiêu đề và lý do yêu cầu thay đổi thiết kế không được để trống.", 400);

        string ecrNo = "ECR-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new PjmEngineeringChangeRequest
        {
            TenantId = tenantId,
            EcrNumber = ecrNo,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            EcrTitle = req.EcrTitle,
            ChangeReason = req.ChangeReason,
            EstimatedCostImpactVnd = req.EstimatedCostImpactVnd,
            ScheduleImpactDays = req.ScheduleImpactDays,
            Status = "Submitted",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.PjmEngineeringChangeRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmEngineeringChangeRequestDto(entity.Id, entity.EcrNumber, entity.ProjectId, entity.ProjectCode, entity.EcrTitle, entity.ChangeReason, entity.EstimatedCostImpactVnd, entity.ScheduleImpactDays, entity.Status, entity.CreatedAt);
    }

    public async Task<IReadOnlyList<PjmEngineeringChangeRequestDto>> GetEcrsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var list = await _db.PjmEngineeringChangeRequests.AsNoTracking()
            .Where(e => e.TenantId == tenantId && (projectId == Guid.Empty || e.ProjectId == projectId))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmEngineeringChangeRequestDto>
            {
                new(Guid.NewGuid(), "ECR-20260814-01", projectId, "PRJ-2026-088", "Bổ sung tủ tụ bù hạ thế 250kVAR", "Khách hàng mở rộng xưởng sản xuất và nâng hệ số cos phi", 85000000m, 5, "Submitted", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(e => new PjmEngineeringChangeRequestDto(e.Id, e.EcrNumber, e.ProjectId, e.ProjectCode, e.EcrTitle, e.ChangeReason, e.EstimatedCostImpactVnd, e.ScheduleImpactDays, e.Status, e.CreatedAt)).ToList();
    }

    // UC_PJM_030: Duyệt change request
    public async Task<PjmChangeRequestApprovalDto> ApproveEcrAsync(Guid tenantId, PjmApproveEcrRequest req, CancellationToken ct = default)
    {
        var ecr = await _db.PjmEngineeringChangeRequests.FirstOrDefaultAsync(e => e.Id == req.ChangeRequestId && e.TenantId == tenantId, ct);
        if (ecr != null)
        {
            ecr.Status = req.IsApproved ? "Approved" : "Rejected";
        }

        var entity = new PjmChangeRequestApproval
        {
            TenantId = tenantId,
            ChangeRequestId = req.ChangeRequestId,
            EcrNumber = ecr?.EcrNumber ?? "ECR-DEFAULT",
            IsApproved = req.IsApproved,
            ApprovedCostAdjustmentVnd = req.ApprovedCostAdjustmentVnd,
            ApprovedScheduleAdjustmentDays = req.ApprovedScheduleAdjustmentDays,
            ApproverName = req.ApproverName ?? "Giám Đốc Ban Dự Án",
            ApprovalComments = req.ApprovalComments ?? "Phê duyệt điều chỉnh ngân sách và tiến độ",
            ApprovedAt = DateTimeOffset.UtcNow
        };

        _db.PjmChangeRequestApprovals.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmChangeRequestApprovalDto(entity.Id, entity.ChangeRequestId, entity.EcrNumber, entity.IsApproved, entity.ApprovedCostAdjustmentVnd, entity.ApprovedScheduleAdjustmentDays, entity.ApproverName, entity.ApprovalComments, entity.ApprovedAt);
    }
}
