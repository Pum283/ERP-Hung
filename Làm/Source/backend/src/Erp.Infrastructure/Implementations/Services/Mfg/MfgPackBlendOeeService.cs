using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class MfgPackBlendOeeService : IMfgPackBlendOeeService
{
    private readonly AppDbContext _db;

    public MfgPackBlendOeeService(AppDbContext db)
    {
        _db = db;
    }

    // UC_MFG_039: Đóng gói & gắn tem
    public async Task<MfgPackagingLabelTagDto> CreatePackagingLabelTagAsync(Guid tenantId, MfgCreatePackagingLabelRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ProductCode))
            throw new AppException("Mã sản phẩm đóng gói không được để trống.", 400);

        var entity = new MfgPackagingLabelTag
        {
            TenantId = tenantId,
            ProductCode = req.ProductCode,
            PackagingType = req.PackagingType ?? "Thùng Carton 5 Lớp",
            UnitsPerPackage = req.UnitsPerPackage > 0 ? req.UnitsPerPackage : 24,
            BarcodeLabelFormat = req.BarcodeLabelFormat ?? "GS1-128 / QR Code",
            LabelTemplatePath = req.LabelTemplatePath ?? "/templates/labels/mfg-standard-100x150.prn",
            IsActive = true
        };

        _db.MfgPackagingLabelTags.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgPackagingLabelTagDto(entity.Id, entity.ProductCode, entity.PackagingType, entity.UnitsPerPackage, entity.BarcodeLabelFormat, entity.LabelTemplatePath, entity.IsActive);
    }

    public async Task<IReadOnlyList<MfgPackagingLabelTagDto>> GetPackagingLabelTagsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgPackagingLabelTags.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<MfgPackagingLabelTagDto>
            {
                new(Guid.NewGuid(), "FG-SERVER-42U", "Kiện Gỗ Pallet Xuất Khẩu", 1, "GS1-128 Serialized", "/labels/rack-42u.prn", true),
                new(Guid.NewGuid(), "FG-DESK-WOOD", "Thùng Carton Chèn Xốp 5 Lớp", 1, "QR Code Truy Xuất Nguồn Gốc", "/labels/desk-wood.prn", true)
            };
        }

        return list.Select(p => new MfgPackagingLabelTagDto(p.Id, p.ProductCode, p.PackagingType, p.UnitsPerPackage, p.BarcodeLabelFormat, p.LabelTemplatePath, p.IsActive)).ToList();
    }

    // UC_MFG_040: Định mức phối trộn
    public async Task<MfgBlendingRecipeRatioDto> CreateBlendingRecipeRatioAsync(Guid tenantId, MfgCreateBlendingRecipeRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RecipeCode) || string.IsNullOrWhiteSpace(req.IngredientProductCode))
            throw new AppException("Mã công thức và mã thành phần không được để trống.", 400);

        var entity = new MfgBlendingRecipeRatio
        {
            TenantId = tenantId,
            RecipeCode = req.RecipeCode,
            RecipeName = req.RecipeName ?? "Công Thức Phối Trộn Tiêu Chuẩn",
            IngredientProductCode = req.IngredientProductCode,
            IngredientProductName = req.IngredientProductName ?? req.IngredientProductCode,
            MixingRatioPercentage = req.MixingRatioPercentage,
            TolerancePercentage = req.TolerancePercentage,
            MixingOrderStep = req.MixingOrderStep ?? "Bước 1"
        };

        _db.MfgBlendingRecipeRatios.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgBlendingRecipeRatioDto(entity.Id, entity.RecipeCode, entity.RecipeName, entity.IngredientProductCode, entity.IngredientProductName, entity.MixingRatioPercentage, entity.TolerancePercentage, entity.MixingOrderStep);
    }

    public async Task<IReadOnlyList<MfgBlendingRecipeRatioDto>> GetBlendingRecipeRatiosAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgBlendingRecipeRatios.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<MfgBlendingRecipeRatioDto>
            {
                new(Guid.NewGuid(), "RECIPE-PAINT-BLACK", "Sơn Đen Mờ Tĩnh Điện", "MAT-EPOXY-RESIN", "Nhựa Epoxy Nền", 60.0m, 0.5m, "Bước 1: Nạp hạt nhựa"),
                new(Guid.NewGuid(), "RECIPE-PAINT-BLACK", "Sơn Đen Mờ Tĩnh Điện", "MAT-BLACK-PIGMENT", "Bột Màu Carbon Đen", 30.0m, 0.2m, "Bước 2: Phối màu phân tán"),
                new(Guid.NewGuid(), "RECIPE-PAINT-BLACK", "Sơn Đen Mờ Tĩnh Điện", "MAT-HARDENER-AG", "Chất Đóng Rắn Kháng Xước", 10.0m, 0.1m, "Bước 3: Gia nhiệt trộn đều")
            };
        }

        return list.Select(r => new MfgBlendingRecipeRatioDto(r.Id, r.RecipeCode, r.RecipeName, r.IngredientProductCode, r.IngredientProductName, r.MixingRatioPercentage, r.TolerancePercentage, r.MixingOrderStep)).ToList();
    }

    // UC_MFG_044: Hiệu suất / OEE
    public async Task<MfgOverallEquipmentEffectivenessDto> CalculateOeeAsync(Guid tenantId, MfgCalculateOeeRequest req, CancellationToken ct = default)
    {
        double oee = (req.AvailabilityRatePct / 100.0) * (req.PerformanceRatePct / 100.0) * (req.QualityRatePct / 100.0) * 100.0;

        var entity = new MfgOverallEquipmentEffectiveness
        {
            TenantId = tenantId,
            WorkCenterCode = req.WorkCenterCode ?? "WC-DEFAULT",
            WorkCenterName = req.WorkCenterName ?? "Trung Tâm Máy Tiêu Chuẩn",
            AvailabilityRatePct = req.AvailabilityRatePct,
            PerformanceRatePct = req.PerformanceRatePct,
            QualityRatePct = req.QualityRatePct,
            OverallOeePct = Math.Round(oee, 2),
            CalculationPeriod = DateTimeOffset.UtcNow
        };

        _db.MfgOverallEquipmentEffectivenesses.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgOverallEquipmentEffectivenessDto(entity.Id, entity.WorkCenterCode, entity.WorkCenterName, entity.AvailabilityRatePct, entity.PerformanceRatePct, entity.QualityRatePct, entity.OverallOeePct, entity.CalculationPeriod);
    }
}
