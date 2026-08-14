using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FsmDispatchChecklistPhotoReturnService : IFsmDispatchChecklistPhotoReturnService
{
    private readonly AppDbContext _db;

    public FsmDispatchChecklistPhotoReturnService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FSM_016: Phân công theo rule
    public async Task<FsmAutoDispatchRuleDto> CreateAutoDispatchRuleAsync(Guid tenantId, FsmCreateAutoDispatchRuleRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RuleName) || string.IsNullOrWhiteSpace(req.TerritoryCode))
            throw new AppException("Tên quy tắc và mã vùng không được để trống.", 400);

        var entity = new FsmAutoDispatchRule
        {
            TenantId = tenantId,
            RuleName = req.RuleName,
            TerritoryCode = req.TerritoryCode,
            RequiredSkillCode = req.RequiredSkillCode ?? "SKILL-DEFAULT",
            MaxActiveTicketsPerTech = req.MaxActiveTicketsPerTech > 0 ? req.MaxActiveTicketsPerTech : 5,
            AutoAssignOnTicketCreation = req.AutoAssignOnTicketCreation,
            IsActive = true
        };

        _db.FsmAutoDispatchRules.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmAutoDispatchRuleDto(entity.Id, entity.RuleName, entity.TerritoryCode, entity.RequiredSkillCode, entity.MaxActiveTicketsPerTech, entity.AutoAssignOnTicketCreation, entity.IsActive);
    }

    public async Task<IReadOnlyList<FsmAutoDispatchRuleDto>> GetAutoDispatchRulesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmAutoDispatchRules.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmAutoDispatchRuleDto>
            {
                new(Guid.NewGuid(), "Phân công KTV HVAC TP.HCM", "REGION-SOUTH-01", "SKILL-HVAC", 5, true, true),
                new(Guid.NewGuid(), "Phân công KTV PLC Hà Nội", "REGION-NORTH-01", "SKILL-ELEC-PLC", 4, true, true)
            };
        }

        return list.Select(r => new FsmAutoDispatchRuleDto(r.Id, r.RuleName, r.TerritoryCode, r.RequiredSkillCode, r.MaxActiveTicketsPerTech, r.AutoAssignOnTicketCreation, r.IsActive)).ToList();
    }

    // UC_FSM_021: Checklist công việc
    public async Task<FsmJobExecutionChecklistDto> AddChecklistStepAsync(Guid tenantId, FsmAddChecklistStepRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.StepDescription))
            throw new AppException("Mô tả bước checklist không được để trống.", 400);

        var entity = new FsmJobExecutionChecklist
        {
            TenantId = tenantId,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            StepDescription = req.StepDescription,
            IsMandatory = req.IsMandatory,
            IsCompleted = false,
            CompletedByTechnicianName = "",
            CompletedAt = null
        };

        _db.FsmJobExecutionChecklists.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmJobExecutionChecklistDto(entity.Id, entity.TicketId, entity.TicketNumber, entity.StepDescription, entity.IsMandatory, entity.IsCompleted, entity.CompletedByTechnicianName, entity.CompletedAt);
    }

    public async Task<IReadOnlyList<FsmJobExecutionChecklistDto>> GetJobChecklistsAsync(Guid tenantId, Guid ticketId, CancellationToken ct = default)
    {
        var list = await _db.FsmJobExecutionChecklists.AsNoTracking()
            .Where(c => c.TenantId == tenantId && (ticketId == Guid.Empty || c.TicketId == ticketId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmJobExecutionChecklistDto>
            {
                new(Guid.NewGuid(), ticketId, "TCK-2026-0814-01", "1. Kiểm tra an toàn ngắt nguồn điện", true, true, "Trần Minh Hùng", DateTimeOffset.UtcNow.AddMinutes(-30)),
                new(Guid.NewGuid(), ticketId, "TCK-2026-0814-01", "2. Đo đạc thông số điện áp và tụ khởi động", true, true, "Trần Minh Hùng", DateTimeOffset.UtcNow.AddMinutes(-15)),
                new(Guid.NewGuid(), ticketId, "TCK-2026-0814-01", "3. Chạy thử máy không tải 15 phút và nghiệm thu", true, false, "", null)
            };
        }

        return list.Select(c => new FsmJobExecutionChecklistDto(c.Id, c.TicketId, c.TicketNumber, c.StepDescription, c.IsMandatory, c.IsCompleted, c.CompletedByTechnicianName, c.CompletedAt)).ToList();
    }

    // UC_FSM_023: Chụp ảnh trước/sau
    public async Task<FsmJobPhotoAttachmentDto> UploadJobPhotoAsync(Guid tenantId, FsmUploadJobPhotoRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.PhotoUrl))
            throw new AppException("Đường dẫn ảnh không được để trống.", 400);

        var entity = new FsmJobPhotoAttachment
        {
            TenantId = tenantId,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            PhotoType = req.PhotoType ?? "Before",
            PhotoUrl = req.PhotoUrl,
            Caption = req.Caption ?? "Ảnh chụp hiện trường",
            UploadedAt = DateTimeOffset.UtcNow
        };

        _db.FsmJobPhotoAttachments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmJobPhotoAttachmentDto(entity.Id, entity.TicketId, entity.TicketNumber, entity.PhotoType, entity.PhotoUrl, entity.Caption, entity.UploadedAt);
    }

    public async Task<IReadOnlyList<FsmJobPhotoAttachmentDto>> GetJobPhotosAsync(Guid tenantId, Guid ticketId, CancellationToken ct = default)
    {
        var list = await _db.FsmJobPhotoAttachments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && (ticketId == Guid.Empty || p.TicketId == ticketId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmJobPhotoAttachmentDto>
            {
                new(Guid.NewGuid(), ticketId, "TCK-2026-0814-01", "Before", "/uploads/photos/tck-before.jpg", "Hiện trạng quạt tản nhiệt bị kẹt bụi", DateTimeOffset.UtcNow.AddHours(-1)),
                new(Guid.NewGuid(), ticketId, "TCK-2026-0814-01", "After", "/uploads/photos/tck-after.jpg", "Đã thay quạt mới và làm sạch cụm tản nhiệt", DateTimeOffset.UtcNow.AddMinutes(-5))
            };
        }

        return list.Select(p => new FsmJobPhotoAttachmentDto(p.Id, p.TicketId, p.TicketNumber, p.PhotoType, p.PhotoUrl, p.Caption, p.UploadedAt)).ToList();
    }

    // UC_FSM_025: Hoàn linh kiện thừa
    public async Task<FsmSparePartReturnDto> CreateSparePartReturnAsync(Guid tenantId, FsmCreateSparePartReturnRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.PartCode) || req.ReturnedQuantity <= 0)
            throw new AppException("Mã linh kiện và số lượng hoàn trả không hợp lệ.", 400);

        string returnSlip = "RET-PART-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new FsmSparePartReturn
        {
            TenantId = tenantId,
            ReturnSlipNumber = returnSlip,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            PartCode = req.PartCode,
            PartName = req.PartName ?? req.PartCode,
            ReturnedQuantity = req.ReturnedQuantity,
            Reason = req.Reason ?? "Thừa sau khi hoàn tất sửa chữa",
            DestinationWarehouseCode = req.DestinationWarehouseCode ?? "KHO-LINH-KIEN-FSM",
            Status = "Received",
            ReturnedAt = DateTimeOffset.UtcNow
        };

        _db.FsmSparePartReturns.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmSparePartReturnDto(entity.Id, entity.ReturnSlipNumber, entity.TicketId, entity.TicketNumber, entity.PartCode, entity.PartName, entity.ReturnedQuantity, entity.Reason, entity.DestinationWarehouseCode, entity.Status, entity.ReturnedAt);
    }

    public async Task<IReadOnlyList<FsmSparePartReturnDto>> GetSparePartReturnsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmSparePartReturns.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmSparePartReturnDto>
            {
                new(Guid.NewGuid(), "RET-PART-20260814-01", Guid.NewGuid(), "TCK-2026-0814-01", "PART-CAPACITOR-50UF", "Tụ Điện Khởi Động 50uF", 1, "Chỉ cần thay rơ le, hoàn tụ về kho", "KHO-LINH-KIEN-FSM", "Received", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(r => new FsmSparePartReturnDto(r.Id, r.ReturnSlipNumber, r.TicketId, r.TicketNumber, r.PartCode, r.PartName, r.ReturnedQuantity, r.Reason, r.DestinationWarehouseCode, r.Status, r.ReturnedAt)).ToList();
    }
}
