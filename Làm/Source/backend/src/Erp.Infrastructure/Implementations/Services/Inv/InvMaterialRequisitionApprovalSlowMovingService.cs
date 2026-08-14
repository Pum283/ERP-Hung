using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class InvMaterialRequisitionApprovalSlowMovingService : IInvMaterialRequisitionApprovalSlowMovingService
{
    private readonly AppDbContext _db;

    public InvMaterialRequisitionApprovalSlowMovingService(AppDbContext db)
    {
        _db = db;
    }

    // UC_INV_057: Đề nghị cấp hàng
    public async Task<InvMaterialRequisitionDto> CreateRequisitionAsync(Guid tenantId, InvCreateMaterialRequisitionRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RequesterName) || req.RequestedQuantity <= 0)
            throw new AppException("Người đề nghị và số lượng yêu cầu không hợp lệ.", 400);

        string reqNum = "REQ-MAT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvMaterialRequisition
        {
            TenantId = tenantId,
            RequisitionNumber = reqNum,
            RequesterName = req.RequesterName,
            DepartmentName = req.DepartmentName ?? "Phòng Vận Hành Sản Xuất",
            WarehouseId = req.WarehouseId == Guid.Empty ? Guid.NewGuid() : req.WarehouseId,
            ProductId = req.ProductId == Guid.Empty ? Guid.NewGuid() : req.ProductId,
            RequestedQuantity = req.RequestedQuantity,
            Status = "Submitted",
            ApproverName = "",
            ConvertedIssueNumber = "",
            RequestedAt = DateTimeOffset.UtcNow,
            ApprovedAt = null
        };

        _db.InvMaterialRequisitions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return MapDto(entity);
    }

    // UC_INV_058: Duyệt đề nghị
    public async Task<InvMaterialRequisitionDto> DecideRequisitionAsync(Guid tenantId, InvDecideMaterialRequisitionRequest req, CancellationToken ct = default)
    {
        var entity = await _db.InvMaterialRequisitions.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == req.RequisitionId, ct);
        if (entity == null)
            throw new AppException("Không tìm thấy đề nghị cấp hàng.", 404);

        entity.Status = req.IsApproved ? "Approved" : "Rejected";
        entity.ApproverName = req.ApproverName ?? "Trưởng Bộ Phận Kho";
        entity.ApprovedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return MapDto(entity);
    }

    // UC_INV_059: Chuyển đề nghị thành phiếu xuất
    public async Task<InvMaterialRequisitionDto> ConvertToStockIssueAsync(Guid tenantId, InvConvertRequisitionToIssueRequest req, CancellationToken ct = default)
    {
        var entity = await _db.InvMaterialRequisitions.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == req.RequisitionId, ct);
        if (entity == null)
            throw new AppException("Không tìm thấy đề nghị cấp hàng.", 404);

        if (entity.Status != "Approved")
            throw new AppException("Đề nghị cấp hàng phải được phê duyệt trước khi chuyển thành phiếu xuất kho.", 400);

        string issueNum = "ISSUE-MAT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        entity.Status = "ConvertedToIssue";
        entity.ConvertedIssueNumber = issueNum;

        await _db.SaveChangesAsync(ct);

        return MapDto(entity);
    }

    // UC_INV_066: Hàng chậm luân chuyển
    public async Task<InvSlowMovingSummaryDto> GetSlowMovingAnalysisAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvSlowMovingAnalyses.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.DaysWithoutIssueMovement)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var sample = new List<InvSlowMovingItemDto>
            {
                new(Guid.NewGuid(), "SKU-OLD-BOARD", "Bo Mạch Chủ Server Gen 8", 40, 210, 80000000m, "HighRisk"),
                new(Guid.NewGuid(), "SKU-FAN-COOLER", "Quạt Tản Nhiệt Rack Công Suất Lớn", 120, 110, 36000000m, "MediumRisk"),
                new(Guid.NewGuid(), "SKU-CABLE-CAT6", "Cuộn Dây Cáp Mạng Cat6 305m", 15, 60, 15000000m, "LowRisk")
            };

            return new InvSlowMovingSummaryDto(sample.Count, sample.Sum(x => x.TiedUpCapitalVnd), sample);
        }

        var items = list.Select(s => new InvSlowMovingItemDto(s.ProductId, s.ProductCode, s.ProductName, s.CurrentStockQuantity, s.DaysWithoutIssueMovement, s.TiedUpCapitalVnd, s.RiskLevel)).ToList();
        return new InvSlowMovingSummaryDto(items.Count, items.Sum(x => x.TiedUpCapitalVnd), items);
    }

    private static InvMaterialRequisitionDto MapDto(InvMaterialRequisition e)
        => new(e.Id, e.RequisitionNumber, e.RequesterName, e.DepartmentName, e.WarehouseId, e.ProductId, e.RequestedQuantity, e.Status, e.ApproverName, e.ConvertedIssueNumber, e.RequestedAt, e.ApprovedAt);
}
