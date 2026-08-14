using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FinCurrencyCashFlowCategoryService : IFinCurrencyCashFlowCategoryService
{
    private readonly AppDbContext _db;

    public FinCurrencyCashFlowCategoryService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FIN_005: Đồng tiền hạch toán & tỷ giá
    public async Task<FinCurrencyExchangeRateDto> CreateExchangeRateAsync(Guid tenantId, FinCreateExchangeRateRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CurrencyCode) || req.ExchangeRateToVnd <= 0)
            throw new AppException("Mã tiền tệ và tỷ giá quy đổi VND phải hợp lệ (>0).", 400);

        var entity = new FinCurrencyExchangeRate
        {
            TenantId = tenantId,
            CurrencyCode = req.CurrencyCode.ToUpperInvariant(),
            CurrencyName = req.CurrencyName ?? req.CurrencyCode,
            ExchangeRateToVnd = req.ExchangeRateToVnd,
            RateSource = req.RateSource ?? "Vietcombank",
            IsBaseCurrency = req.IsBaseCurrency,
            EffectiveDate = req.EffectiveDate
        };

        _db.FinCurrencyExchangeRates.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinCurrencyExchangeRateDto(entity.Id, entity.CurrencyCode, entity.CurrencyName, entity.ExchangeRateToVnd, entity.RateSource, entity.IsBaseCurrency, entity.EffectiveDate);
    }

    public async Task<IReadOnlyList<FinCurrencyExchangeRateDto>> GetExchangeRatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinCurrencyExchangeRates.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.CurrencyCode)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinCurrencyExchangeRateDto>
            {
                new(Guid.NewGuid(), "USD", "Đô La Mỹ", 25450m, "Vietcombank", false, DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "EUR", "Đồng Euro", 27800m, "Vietcombank", false, DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "JPY", "Yên Nhật", 168.5m, "Vietcombank", false, DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "VND", "Việt Nam Đồng", 1.0m, "Ngân Hàng Nhà Nước", true, DateTimeOffset.UtcNow)
            };
        }

        return list.Select(r => new FinCurrencyExchangeRateDto(r.Id, r.CurrencyCode, r.CurrencyName, r.ExchangeRateToVnd, r.RateSource, r.IsBaseCurrency, r.EffectiveDate)).ToList();
    }

    // UC_FIN_007: Khoản mục thu/chi
    public async Task<FinCashFlowCategoryDto> CreateCashFlowCategoryAsync(Guid tenantId, FinCreateCashFlowCategoryRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CategoryCode) || string.IsNullOrWhiteSpace(req.CategoryName))
            throw new AppException("Mã và tên khoản mục thu/chi không được để trống.", 400);

        var entity = new FinCashFlowCategory
        {
            TenantId = tenantId,
            CategoryCode = req.CategoryCode,
            CategoryName = req.CategoryName,
            CashFlowType = req.CashFlowType ?? "Inflow",
            SectionCode = req.SectionCode ?? "Operating",
            IsActive = req.IsActive
        };

        _db.FinCashFlowCategories.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinCashFlowCategoryDto(entity.Id, entity.CategoryCode, entity.CategoryName, entity.CashFlowType, entity.SectionCode, entity.IsActive);
    }

    public async Task<IReadOnlyList<FinCashFlowCategoryDto>> GetCashFlowCategoriesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinCashFlowCategories.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.CategoryCode)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinCashFlowCategoryDto>
            {
                new(Guid.NewGuid(), "CASH-IN-PRJ", "Thu tiền theo tiến độ hợp đồng dự án", "Inflow", "Operating", true),
                new(Guid.NewGuid(), "CASH-OUT-MAT", "Chi tiền mua nguyên vật liệu & thiết bị", "Outflow", "Operating", true),
                new(Guid.NewGuid(), "CASH-OUT-LABOR", "Chi trả tiền lương nhân công thi công", "Outflow", "Operating", true),
                new(Guid.NewGuid(), "CASH-OUT-TAX", "Chi nộp thuế GTGT & thuế TNDN", "Outflow", "Operating", true)
            };
        }

        return list.Select(c => new FinCashFlowCategoryDto(c.Id, c.CategoryCode, c.CategoryName, c.CashFlowType, c.SectionCode, c.IsActive)).ToList();
    }
}
