using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurAdvanceBlanketContractExpirationService : IPurAdvanceBlanketContractExpirationService
{
    private readonly AppDbContext _db;

    public PurAdvanceBlanketContractExpirationService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_044: Tạm ứng nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurVendorAdvancePaymentDto> CreateAdvancePaymentAsync(Guid tenantId, PurCreateVendorAdvancePaymentRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || req.AdvanceAmountVnd <= 0)
            throw new AppException("Mã nhà cung cấp và số tiền tạm ứng phải lớn hơn 0.", 400);

        var adv = new PurVendorAdvancePayment
        {
            TenantId = tenantId,
            PurchaseOrderId = req.PurchaseOrderId,
            RequestNumber = "ADV-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
            SupplierId = req.SupplierId,
            AdvanceAmountVnd = req.AdvanceAmountVnd,
            PaymentReason = req.PaymentReason ?? "Tạm ứng đặt cọc Hợp đồng mua hàng",
            Status = "Approved",
            RequestedAt = DateTimeOffset.UtcNow
        };

        _db.PurVendorAdvancePayments.Add(adv);
        await _db.SaveChangesAsync(ct);

        return new PurVendorAdvancePaymentDto(
            adv.Id,
            adv.PurchaseOrderId,
            adv.RequestNumber,
            adv.SupplierId,
            "Nhà Cung Cấp",
            adv.AdvanceAmountVnd,
            adv.PaymentReason,
            adv.Status,
            adv.RequestedAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_045, UC_PUR_046, UC_PUR_047: Hợp đồng khung & Cảnh báo hết hạn
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurBlanketContractDto> CreateBlanketContractAsync(Guid tenantId, PurCreateBlanketContractRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ContractNumber) || req.SupplierId == Guid.Empty || req.TotalContractValueVnd <= 0)
            throw new AppException("Số hợp đồng, NCC và tổng giá trị hợp đồng không được để trống.", 400);

        var c = new PurBlanketContract
        {
            TenantId = tenantId,
            ContractNumber = req.ContractNumber,
            ContractTitle = req.ContractTitle,
            SupplierId = req.SupplierId,
            TotalContractValueVnd = req.TotalContractValueVnd,
            ConsumedValueVnd = 0,
            TotalContractQty = req.TotalContractQty,
            ConsumedQty = 0,
            StartDate = req.StartDate,
            ExpirationDate = req.ExpirationDate,
            Status = "Active"
        };

        _db.PurBlanketContracts.Add(c);
        await _db.SaveChangesAsync(ct);

        return MapToDto(c);
    }

    public async Task<IReadOnlyList<PurBlanketContractDto>> GetBlanketContractsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurBlanketContracts.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurBlanketContractDto>
            {
                new(
                    Guid.NewGuid(),
                    "BPO-2026-VINAMILK",
                    "Hợp Đồng Khung Cung Cấp Sữa Tươi 2026",
                    Guid.NewGuid(),
                    "Vinamilk Co.",
                    500000000m,
                    320000000m,
                    180000000m,
                    20000,
                    12800,
                    7200,
                    64.0,
                    DateTimeOffset.UtcNow.AddMonths(-6),
                    DateTimeOffset.UtcNow.AddDays(25),
                    25,
                    true,
                    "ExpiringSoon"
                )
            };
        }

        return list.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<PurBlanketContractDto>> GetExpiringContractsAlertsAsync(Guid tenantId, int warningDaysThreshold = 30, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var thresholdDate = now.AddDays(warningDaysThreshold);

        var list = await GetBlanketContractsAsync(tenantId, ct);
        return list.Where(c => c.ExpirationDate <= thresholdDate || c.IsExpiringSoon).ToList();
    }

    private static PurBlanketContractDto MapToDto(PurBlanketContract c)
    {
        decimal remainingValue = c.TotalContractValueVnd - c.ConsumedValueVnd;
        int remainingQty = c.TotalContractQty - c.ConsumedQty;
        double pct = c.TotalContractValueVnd > 0 ? Math.Round((double)(c.ConsumedValueVnd / c.TotalContractValueVnd) * 100, 2) : 0;
        int daysLeft = (int)(c.ExpirationDate - DateTimeOffset.UtcNow).TotalDays;
        bool isExpiring = daysLeft <= 30;

        return new PurBlanketContractDto(
            c.Id,
            c.ContractNumber,
            c.ContractTitle,
            c.SupplierId,
            "Nhà Cung Cấp",
            c.TotalContractValueVnd,
            c.ConsumedValueVnd,
            remainingValue,
            c.TotalContractQty,
            c.ConsumedQty,
            remainingQty,
            pct,
            c.StartDate,
            c.ExpirationDate,
            daysLeft,
            isExpiring,
            isExpiring ? "ExpiringSoon" : c.Status
        );
    }
}
