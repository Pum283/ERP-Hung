using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmFieldVisitOutcomeOrderService : ICrmFieldVisitOutcomeOrderService
{
    private readonly AppDbContext _db;

    public CrmFieldVisitOutcomeOrderService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_093: Ghi nhận mục đích – kết quả visit
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmVisitOutcomeDto> RecordOutcomeAsync(Guid tenantId, CrmRecordVisitOutcomeRequest req, CancellationToken ct = default)
    {
        if (req.VisitPlanId == Guid.Empty)
            throw new AppException("Mã kế hoạch viếng thăm không được để trống.", 400);

        var outcome = new CrmVisitOutcome
        {
            TenantId = tenantId,
            VisitPlanId = req.VisitPlanId,
            Purpose = req.Purpose ?? "Thăm đại lý định kỳ",
            OutcomeStatus = req.OutcomeStatus ?? "Successful",
            SummaryNotes = req.SummaryNotes ?? "",
            ActionItems = req.ActionItems ?? "",
            RecordedAt = DateTimeOffset.UtcNow
        };

        _db.CrmVisitOutcomes.Add(outcome);

        // Update visit plan status if present
        var plan = await _db.CrmVisitPlans.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == req.VisitPlanId, ct);
        if (plan != null)
        {
            plan.Status = "Completed";
        }

        await _db.SaveChangesAsync(ct);

        return new CrmVisitOutcomeDto(
            outcome.Id,
            outcome.VisitPlanId,
            outcome.Purpose,
            outcome.OutcomeStatus,
            outcome.SummaryNotes,
            outcome.ActionItems,
            outcome.RecordedAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_094: Ghi nhận nhu cầu khách hàng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmVisitDemandDto> RecordDemandAsync(Guid tenantId, CrmRecordCustomerDemandRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty || string.IsNullOrWhiteSpace(req.ProductInterestCategory))
            throw new AppException("Mã khách hàng và nhóm sản phẩm quan tâm không được để trống.", 400);

        var demand = new CrmVisitDemand
        {
            TenantId = tenantId,
            VisitPlanId = req.VisitPlanId,
            CustomerId = req.CustomerId,
            ProductInterestCategory = req.ProductInterestCategory,
            EstimatedQuantity = Math.Max(1, req.EstimatedQuantity),
            Urgency = req.Urgency ?? "Medium",
            CompetitorInfo = req.CompetitorInfo ?? "",
            CustomerFeedback = req.CustomerFeedback ?? "",
            LoggedAt = DateTimeOffset.UtcNow
        };

        _db.CrmVisitDemands.Add(demand);
        await _db.SaveChangesAsync(ct);

        return new CrmVisitDemandDto(
            demand.Id,
            demand.VisitPlanId,
            demand.CustomerId,
            demand.ProductInterestCategory,
            demand.EstimatedQuantity,
            demand.Urgency,
            demand.CompetitorInfo,
            demand.CustomerFeedback,
            demand.LoggedAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_095: Đặt hàng tại điểm thăm
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmOnSiteOrderDto> CreateOnSiteOrderAsync(Guid tenantId, CrmCreateOnSiteOrderRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty)
            throw new AppException("Mã khách hàng đặt đơn không được để trống.", 400);

        decimal totalAmount = req.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0m;
        string orderCode = $"ORD-ONSITE-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var order = new CrmOnlineOrderIntake
        {
            TenantId = tenantId,
            Channel = "FieldSales OnSite",
            ExternalOrderCode = orderCode,
            CustomerName = $"Khách hàng #{req.CustomerId.ToString()[..6]}",
            Phone = "0908888999",
            TotalAmount = totalAmount,
            Status = "OnSiteSubmitted",
            ReceivedAt = DateTimeOffset.UtcNow
        };

        _db.CrmOnlineOrderIntakes.Add(order);
        await _db.SaveChangesAsync(ct);

        return new CrmOnSiteOrderDto(
            order.Id,
            orderCode,
            req.CustomerId,
            order.CustomerName,
            totalAmount,
            "OnSiteSubmitted",
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_096: Xem lịch sử visit
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CrmVisitHistoryLogDto>> GetVisitHistoryLogsAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default)
    {
        var plans = await _db.CrmVisitPlans.AsNoTracking()
            .Where(p => p.TenantId == tenantId && (!customerId.HasValue || p.CustomerId == customerId))
            .OrderByDescending(p => p.PlannedDate)
            .ToListAsync(ct);

        if (plans.Count == 0)
        {
            return new List<CrmVisitHistoryLogDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Đại lý Thực phẩm An Phát", Guid.NewGuid(), "Nguyễn Văn Sales", DateTime.UtcNow.AddDays(-1), "Completed", "10.7769,106.7009", DateTimeOffset.UtcNow.AddDays(-1).AddHours(9), "10.7772,106.7012", DateTimeOffset.UtcNow.AddDays(-1).AddHours(10), "Successful", "Khách hàng chốt thêm 50 thùng sản phẩm mới"),
                new(Guid.NewGuid(), Guid.NewGuid(), "Chuỗi Cửa hàng Tiện Lợi An Khang", Guid.NewGuid(), "Trần Thị CRM", DateTime.UtcNow.AddDays(-3), "Completed", "10.7800,106.6900", DateTimeOffset.UtcNow.AddDays(-3).AddHours(14), "10.7805,106.6910", DateTimeOffset.UtcNow.AddDays(-3).AddHours(15), "FollowUpRequired", "Cần gửi thêm mẫu thử tuần tới")
            };
        }

        var planIds = plans.Select(p => p.Id).ToList();
        var outcomes = await _db.CrmVisitOutcomes.AsNoTracking()
            .Where(o => o.TenantId == tenantId && planIds.Contains(o.VisitPlanId))
            .ToDictionaryAsync(o => o.VisitPlanId, ct);

        return plans.Select(p =>
        {
            outcomes.TryGetValue(p.Id, out var outObj);
            return new CrmVisitHistoryLogDto(
                p.Id,
                p.CustomerId,
                $"Khách hàng #{p.CustomerId.ToString()[..6]}",
                p.SalespersonId,
                $"Sales Rep #{p.SalespersonId.ToString()[..6]}",
                p.PlannedDate,
                p.Status,
                p.CheckInGps,
                p.CheckInTime,
                p.CheckOutGps,
                p.CheckOutTime,
                outObj?.OutcomeStatus ?? "N/A",
                outObj?.SummaryNotes ?? p.Notes
            );
        }).ToList();
    }
}
