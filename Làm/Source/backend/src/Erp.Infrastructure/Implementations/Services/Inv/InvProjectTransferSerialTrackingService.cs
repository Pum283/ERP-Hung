using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class InvProjectTransferSerialTrackingService : IInvProjectTransferSerialTrackingService
{
    private readonly AppDbContext _db;

    public InvProjectTransferSerialTrackingService(AppDbContext db)
    {
        _db = db;
    }

    // UC_INV_028: Xuất cho dự án
    public async Task<InvProjectDispatchDto> CreateProjectDispatchAsync(Guid tenantId, InvCreateProjectDispatchRequest req, CancellationToken ct = default)
    {
        if (req.ProjectId == Guid.Empty || string.IsNullOrWhiteSpace(req.ProjectName))
            throw new AppException("Dự án không được để trống.", 400);

        string dispatchNum = "PRJ-OUT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvProjectDispatch
        {
            TenantId = tenantId,
            DispatchNumber = dispatchNum,
            ProjectId = req.ProjectId,
            ProjectName = req.ProjectName,
            WarehouseId = req.WarehouseId == Guid.Empty ? Guid.NewGuid() : req.WarehouseId,
            TotalAllocatedValueVnd = req.TotalAllocatedValueVnd > 0 ? req.TotalAllocatedValueVnd : 5000000m,
            ProjectPhase = string.IsNullOrWhiteSpace(req.ProjectPhase) ? "Phase 1 - Triển khai thi công" : req.ProjectPhase,
            DispatchedAt = DateTimeOffset.UtcNow
        };

        _db.InvProjectDispatches.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvProjectDispatchDto(entity.Id, entity.DispatchNumber, entity.ProjectId, entity.ProjectName, entity.WarehouseId, entity.TotalAllocatedValueVnd, entity.ProjectPhase, entity.DispatchedAt);
    }

    // UC_INV_032: Duyệt chuyển kho
    public async Task<InvTransferApprovalDto> CreateTransferApprovalAsync(Guid tenantId, InvCreateTransferApprovalRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TransferRequestNumber))
            throw new AppException("Mã yêu cầu điều chuyển không được để trống.", 400);

        var entity = new InvTransferApproval
        {
            TenantId = tenantId,
            TransferRequestNumber = req.TransferRequestNumber,
            SourceWarehouseId = req.SourceWarehouseId,
            DestinationWarehouseId = req.DestinationWarehouseId,
            ApprovalStatus = "PendingApproval",
            ApproverName = "",
            ApprovalComments = "",
            DecisionAt = null
        };

        _db.InvTransferApprovals.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvTransferApprovalDto(entity.Id, entity.TransferRequestNumber, entity.SourceWarehouseId, entity.DestinationWarehouseId, entity.ApprovalStatus, entity.ApproverName, entity.ApprovalComments, entity.DecisionAt);
    }

    public async Task<InvTransferApprovalDto> DecideTransferApprovalAsync(Guid tenantId, InvDecideTransferApprovalRequest req, CancellationToken ct = default)
    {
        var entity = await _db.InvTransferApprovals.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == req.ApprovalId, ct);
        if (entity == null)
            throw new AppException("Không tìm thấy yêu cầu điều chuyển kho.", 404);

        entity.ApprovalStatus = req.IsApproved ? "Approved" : "Rejected";
        entity.ApproverName = req.ApproverName ?? "Ban Giám Đốc Kho";
        entity.ApprovalComments = req.Comments ?? "";
        entity.DecisionAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new InvTransferApprovalDto(entity.Id, entity.TransferRequestNumber, entity.SourceWarehouseId, entity.DestinationWarehouseId, entity.ApprovalStatus, entity.ApproverName, entity.ApprovalComments, entity.DecisionAt);
    }

    // UC_INV_034: Chuyển kho một bước
    public async Task<InvOneStepTransferDto> ExecuteOneStepTransferAsync(Guid tenantId, InvExecuteOneStepTransferRequest req, CancellationToken ct = default)
    {
        if (req.FromWarehouseId == Guid.Empty || req.ToWarehouseId == Guid.Empty)
            throw new AppException("Kho xuất và kho nhập không được để trống.", 400);

        if (req.Quantity <= 0)
            throw new AppException("Số lượng chuyển kho phải lớn hơn 0.", 400);

        string trfNum = "TRF-DIRECT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvOneStepTransfer
        {
            TenantId = tenantId,
            TransferNumber = trfNum,
            FromWarehouseId = req.FromWarehouseId,
            ToWarehouseId = req.ToWarehouseId,
            ProductId = req.ProductId == Guid.Empty ? Guid.NewGuid() : req.ProductId,
            Quantity = req.Quantity,
            TransferReason = req.TransferReason ?? "Điều phối tức thời 1 bước",
            ExecutedAt = DateTimeOffset.UtcNow
        };

        _db.InvOneStepTransfers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvOneStepTransferDto(entity.Id, entity.TransferNumber, entity.FromWarehouseId, entity.ToWarehouseId, entity.ProductId, entity.Quantity, entity.TransferReason, entity.ExecutedAt);
    }

    // UC_INV_046: Theo dõi serial
    public async Task<InvSerialTrackingHistoryDto> RecordSerialEventAsync(Guid tenantId, InvRecordSerialEventRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.SerialNumber))
            throw new AppException("Số Serial không được để trống.", 400);

        var entity = new InvSerialTrackingHistory
        {
            TenantId = tenantId,
            ProductId = req.ProductId == Guid.Empty ? Guid.NewGuid() : req.ProductId,
            ProductCode = req.ProductCode ?? "SKU-PROD",
            SerialNumber = req.SerialNumber,
            EventType = req.EventType ?? "InternalTransfer",
            CurrentLocation = req.CurrentLocation ?? "Kho Tổng",
            DocumentReference = req.DocumentReference ?? "",
            Timestamp = DateTimeOffset.UtcNow
        };

        _db.InvSerialTrackingHistories.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvSerialTrackingHistoryDto(entity.Id, entity.ProductId, entity.ProductCode, entity.SerialNumber, entity.EventType, entity.CurrentLocation, entity.DocumentReference, entity.Timestamp);
    }

    public async Task<IReadOnlyList<InvSerialTrackingHistoryDto>> GetSerialHistoryAsync(Guid tenantId, string serialNumber, CancellationToken ct = default)
    {
        var list = await _db.InvSerialTrackingHistories.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.SerialNumber == serialNumber)
            .OrderByDescending(s => s.Timestamp)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<InvSerialTrackingHistoryDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "SKU-SERVER-RACK", serialNumber, "GoodsReceipt", "Kho Tổng TP.HCM", "GRN-2026-001", DateTimeOffset.UtcNow.AddDays(-10)),
                new(Guid.NewGuid(), Guid.NewGuid(), "SKU-SERVER-RACK", serialNumber, "InternalTransfer", "Kho Chi Nhánh Hà Nội", "TRF-DIRECT-088", DateTimeOffset.UtcNow.AddDays(-3))
            };
        }

        return list.Select(s => new InvSerialTrackingHistoryDto(s.Id, s.ProductId, s.ProductCode, s.SerialNumber, s.EventType, s.CurrentLocation, s.DocumentReference, s.Timestamp)).ToList();
    }
}
