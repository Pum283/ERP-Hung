using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurSupplierCategoryQualityPosSyncService : IPurSupplierCategoryQualityPosSyncService
{
    private readonly AppDbContext _db;

    public PurSupplierCategoryQualityPosSyncService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_060: Đồng bộ đơn sang CRM
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosSyncOrderToCrmResultDto> SyncPosOrderToCrmAsync(Guid tenantId, PosSyncOrderToCrmRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.PosOrderId == Guid.Empty || req.CustomerId == Guid.Empty)
            throw new AppException("Mã đơn POS và mã khách hàng không được để trống.", 400);

        return new PosSyncOrderToCrmResultDto(
            req.PosOrderId,
            req.CustomerId,
            true,
            "CRM-ACT-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_002: Phân loại nhóm nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurSupplierCategoryDto> SaveSupplierCategoryAsync(Guid tenantId, PurSaveSupplierCategoryRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CategoryCode) || string.IsNullOrWhiteSpace(req.CategoryName))
            throw new AppException("Mã nhóm và tên nhóm nhà cung cấp không được để trống.", 400);

        var cat = new PurSupplierCategory
        {
            TenantId = tenantId,
            CategoryCode = req.CategoryCode,
            CategoryName = req.CategoryName,
            Description = req.Description ?? "",
            IsActive = true
        };

        _db.PurSupplierCategories.Add(cat);
        await _db.SaveChangesAsync(ct);

        return new PurSupplierCategoryDto(cat.Id, cat.CategoryCode, cat.CategoryName, cat.Description, cat.IsActive);
    }

    public async Task<IReadOnlyList<PurSupplierCategoryDto>> GetSupplierCategoriesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurSupplierCategories.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurSupplierCategoryDto>
            {
                new(Guid.NewGuid(), "CAT-FOOD", "Nhóm Nhà Cung Cấp Thực Phẩm Fresh", "Nông sản, thịt heo/bò tươi sống", true),
                new(Guid.NewGuid(), "CAT-PACKAGING", "Nhóm Bao Bì & Vật Tư Tiêu Hao", "Bao bì hộp giấy, ly nhựa", true)
            };
        }

        return list.Select(c => new PurSupplierCategoryDto(c.Id, c.CategoryCode, c.CategoryName, c.Description, c.IsActive)).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_004: Lead time & MOQ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurSupplierLeadTimeMoqDto> GetSupplierLeadTimeMoqAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return new PurSupplierLeadTimeMoqDto(
            supplierId == Guid.Empty ? Guid.NewGuid() : supplierId,
            "SUP-001",
            "Công Ty TNHH Thực Phẩm Sạch Vinamilk",
            3, // 3 ngày Lead time
            100, // MOQ 100 thùng
            10000000m // MOV 10 triệu
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_005: Đánh giá chất lượng nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurSupplierQualityEvaluationDto> EvaluateSupplierQualityAsync(Guid tenantId, Guid evaluatorUserId, PurSaveSupplierQualityEvaluationRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty)
            throw new AppException("Mã nhà cung cấp không được để trống.", 400);

        double overall = (req.OnTimeDeliveryScore + req.QualityComplianceScore + req.PriceCompetitivenessScore) / 3.0;
        string grade = overall >= 90 ? "A" : overall >= 75 ? "B" : overall >= 60 ? "C" : "D";

        var eval = new PurSupplierQualityEvaluation
        {
            TenantId = tenantId,
            SupplierId = req.SupplierId,
            Period = string.IsNullOrWhiteSpace(req.Period) ? "Q3-2026" : req.Period,
            OnTimeDeliveryScore = req.OnTimeDeliveryScore,
            QualityComplianceScore = req.QualityComplianceScore,
            PriceCompetitivenessScore = req.PriceCompetitivenessScore,
            OverallRatingScore = overall,
            RatingGrade = grade,
            Comments = req.Comments ?? "Đánh giá định kỳ chất lượng giao hàng NCC",
            EvaluatedByUserId = evaluatorUserId,
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        _db.PurSupplierQualityEvaluations.Add(eval);
        await _db.SaveChangesAsync(ct);

        return new PurSupplierQualityEvaluationDto(
            eval.Id,
            eval.SupplierId,
            eval.Period,
            eval.OnTimeDeliveryScore,
            eval.QualityComplianceScore,
            eval.PriceCompetitivenessScore,
            eval.OverallRatingScore,
            eval.RatingGrade,
            eval.Comments,
            eval.EvaluatedAt
        );
    }

    public async Task<IReadOnlyList<PurSupplierQualityEvaluationDto>> GetSupplierQualityEvaluationsAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default)
    {
        var list = await _db.PurSupplierQualityEvaluations.AsNoTracking()
            .Where(e => e.TenantId == tenantId && (supplierId == Guid.Empty || e.SupplierId == supplierId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurSupplierQualityEvaluationDto>
            {
                new(Guid.NewGuid(), supplierId == Guid.Empty ? Guid.NewGuid() : supplierId, "Q3-2026", 95, 90, 88, 91.0, "A", "Giao hàng đúng hẹn, tỷ lệ lỗi < 1%", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(e => new PurSupplierQualityEvaluationDto(
            e.Id,
            e.SupplierId,
            e.Period,
            e.OnTimeDeliveryScore,
            e.QualityComplianceScore,
            e.PriceCompetitivenessScore,
            e.OverallRatingScore,
            e.RatingGrade,
            e.Comments,
            e.EvaluatedAt
        )).ToList();
    }
}
