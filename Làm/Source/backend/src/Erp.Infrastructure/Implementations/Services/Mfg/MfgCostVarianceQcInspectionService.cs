using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class MfgCostVarianceQcInspectionService : IMfgCostVarianceQcInspectionService
{
    private readonly AppDbContext _db;

    public MfgCostVarianceQcInspectionService(AppDbContext db)
    {
        _db = db;
    }

    // UC_MFG_030: Đối chiếu lý thuyết vs thực tế
    public async Task<MfgCostVarianceAnalysisDto> AnalyzeCostVarianceAsync(Guid tenantId, MfgAnalyzeCostVarianceRequest req, CancellationToken ct = default)
    {
        decimal variance = req.ActualIncurredCostVnd - req.StandardTheoreticalCostVnd;
        double pct = req.StandardTheoreticalCostVnd > 0 ? (double)(variance / req.StandardTheoreticalCostVnd) * 100 : 0;

        string analNum = "VAR-COST-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new MfgCostVarianceAnalysis
        {
            TenantId = tenantId,
            AnalysisNumber = analNum,
            WorkOrderId = req.WorkOrderId == Guid.Empty ? Guid.NewGuid() : req.WorkOrderId,
            WorkOrderNumber = req.WorkOrderNumber ?? "WO-DEFAULT",
            StandardTheoreticalCostVnd = req.StandardTheoreticalCostVnd,
            ActualIncurredCostVnd = req.ActualIncurredCostVnd,
            CostVarianceVnd = variance,
            VariancePercentage = pct,
            VarianceRootCause = req.VarianceRootCause ?? "Chênh lệch giá NVL thực tế và hao hụt công đoạn",
            AnalyzedAt = DateTimeOffset.UtcNow
        };

        _db.MfgCostVarianceAnalyses.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgCostVarianceAnalysisDto(entity.Id, entity.AnalysisNumber, entity.WorkOrderId, entity.WorkOrderNumber, entity.StandardTheoreticalCostVnd, entity.ActualIncurredCostVnd, entity.CostVarianceVnd, entity.VariancePercentage, entity.VarianceRootCause, entity.AnalyzedAt);
    }

    // UC_MFG_032: Tiêu chí QC đầu vào
    public async Task<MfgIncomingQcCriterionDto> CreateIncomingQcCriterionAsync(Guid tenantId, MfgCreateIncomingQcCriterionRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CriterionCode) || string.IsNullOrWhiteSpace(req.CriterionName))
            throw new AppException("Mã và tên tiêu chí QC không được để trống.", 400);

        var entity = new MfgIncomingQcCriterion
        {
            TenantId = tenantId,
            CriterionCode = req.CriterionCode,
            CriterionName = req.CriterionName,
            MaterialGroup = req.MaterialGroup ?? "Kim Loại Tấm",
            StandardSpecification = req.StandardSpecification ?? "Độ dày chuẩn 2.0mm ± 0.05mm",
            InspectionMethod = req.InspectionMethod ?? "Thước kẹp điện tử Mitutoyo",
            MinAcceptableValue = req.MinAcceptableValue,
            MaxAcceptableValue = req.MaxAcceptableValue,
            IsMandatory = req.IsMandatory
        };

        _db.MfgIncomingQcCriteria.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgIncomingQcCriterionDto(entity.Id, entity.CriterionCode, entity.CriterionName, entity.MaterialGroup, entity.StandardSpecification, entity.InspectionMethod, entity.MinAcceptableValue, entity.MaxAcceptableValue, entity.IsMandatory);
    }

    public async Task<IReadOnlyList<MfgIncomingQcCriterionDto>> GetIncomingQcCriteriaAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgIncomingQcCriteria.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<MfgIncomingQcCriterionDto>
            {
                new(Guid.NewGuid(), "QC-STEEL-THICK", "Độ Dày Thép Tấm", "Kim Loại Tấm", "2.0mm (Dung sai ± 0.05mm)", "Thước kẹp Panme", 1.95m, 2.05m, true),
                new(Guid.NewGuid(), "QC-PAINT-ADHESION", "Độ Bám Dính Bề Mặt Sơn", "Sơn Tĩnh Điện", "TCVN 2097:1993", "Thử nghiệm dao cắt ô cờ", 100m, 100m, true)
            };
        }

        return list.Select(c => new MfgIncomingQcCriterionDto(c.Id, c.CriterionCode, c.CriterionName, c.MaterialGroup, c.StandardSpecification, c.InspectionMethod, c.MinAcceptableValue, c.MaxAcceptableValue, c.IsMandatory)).ToList();
    }

    // UC_MFG_033: QC thành phẩm
    public async Task<MfgFinishedGoodsQcCheckDto> PerformFinishedGoodsQcAsync(Guid tenantId, MfgPerformFinishedGoodsQcRequest req, CancellationToken ct = default)
    {
        string fqcNum = "FQC-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new MfgFinishedGoodsQcCheck
        {
            TenantId = tenantId,
            InspectionNumber = fqcNum,
            WorkOrderId = req.WorkOrderId == Guid.Empty ? Guid.NewGuid() : req.WorkOrderId,
            WorkOrderNumber = req.WorkOrderNumber ?? "WO-DEFAULT",
            FinishedProductCode = req.FinishedProductCode ?? "FG-DEFAULT",
            SampleSizeQty = req.SampleSizeQty > 0 ? req.SampleSizeQty : 10,
            DefectFoundQty = req.DefectFoundQty,
            InspectionResult = req.InspectionResult ?? (req.DefectFoundQty == 0 ? "Pass" : "Fail"),
            InspectorName = req.InspectorName ?? "Kỹ Sư QC Trưởng",
            InspectedAt = DateTimeOffset.UtcNow
        };

        _db.MfgFinishedGoodsQcChecks.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgFinishedGoodsQcCheckDto(entity.Id, entity.InspectionNumber, entity.WorkOrderId, entity.WorkOrderNumber, entity.FinishedProductCode, entity.SampleSizeQty, entity.DefectFoundQty, entity.InspectionResult, entity.InspectorName, entity.InspectedAt);
    }

    // UC_MFG_034: Ghi nhận lô đạt / không đạt
    public async Task<MfgInspectionLotDispositionDto> DecideLotDispositionAsync(Guid tenantId, MfgDecideLotDispositionRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.LotNumber))
            throw new AppException("Mã số lô hàng không được để trống.", 400);

        var entity = new MfgInspectionLotDisposition
        {
            TenantId = tenantId,
            LotNumber = req.LotNumber,
            ItemCode = req.ItemCode ?? "ITEM-DEFAULT",
            TotalLotQuantity = req.TotalLotQuantity,
            AcceptedQuantity = req.AcceptedQuantity,
            RejectedQuantity = req.RejectedQuantity,
            DispositionDecision = req.DispositionDecision ?? "ReleaseToStock",
            QualityManagerNote = req.QualityManagerNote ?? "Cho phép nhập kho thành phẩm",
            DecidedAt = DateTimeOffset.UtcNow
        };

        _db.MfgInspectionLotDispositions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgInspectionLotDispositionDto(entity.Id, entity.LotNumber, entity.ItemCode, entity.TotalLotQuantity, entity.AcceptedQuantity, entity.RejectedQuantity, entity.DispositionDecision, entity.QualityManagerNote, entity.DecidedAt);
    }
}
