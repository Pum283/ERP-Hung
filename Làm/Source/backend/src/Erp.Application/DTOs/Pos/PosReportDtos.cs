namespace Erp.Application.DTOs.Pos;

public sealed record PosRevenueByTimeRowDto(
    string Bucket, DateTimeOffset? BucketStart, Guid? ShiftId, string? ShiftCode,
    int SaleCount, decimal Revenue, decimal Discount);

public sealed record PosRevenueByProductRowDto(
    string ProductCode, string ProductName, decimal Qty, decimal Revenue, int LineCount);

public sealed record PosRevenueByCashierRowDto(
    Guid CashierUserId, string CashierName, int SaleCount, decimal Revenue, decimal Discount);

public sealed record PosCancelDiscountReportDto(
    int TotalSales, int PaidSales, int CancelledSales, int DiscountedSales,
    decimal CancelRatePercent, decimal DiscountRatePercent,
    decimal TotalRevenue, decimal TotalDiscount);

// ── UC_POS_066 — top SP bán chạy ──
public sealed record PosTopProductRowDto(
    int Rank, string ProductCode, string ProductName,
    decimal Qty, decimal Revenue, int LineCount);

// ── UC_POS_067 — so sánh điểm bán ──
public sealed record PosStoreCompareRowDto(
    Guid StoreId, string StoreCode, string StoreName,
    int SaleCount, decimal Revenue, decimal Discount,
    decimal AvgTicket, decimal RevenueSharePercent);

// ── UC_POS_065 — cost lý thuyết (BOM) vs thực tế (INV Issue) ──
public sealed record PosCostVarianceRowDto(
    string MaterialCode, string MaterialName,
    decimal TheoreticalQty, decimal ActualQty,
    decimal StandardCost, decimal TheoreticalCost, decimal ActualCost,
    decimal VarianceCost, decimal VariancePercent);

// ── UC_POS_069 — giám sát doanh thu chuỗi realtime + UC_POS_072 target ──
public sealed record PosChainLiveRowDto(
    Guid StoreId, string StoreCode, string StoreName, string Status,
    int OpenShiftCount, int TodaySaleCount, decimal TodayRevenue,
    decimal MonthRevenue, decimal MonthlyTarget,
    decimal TargetAttainmentPercent, decimal MonthElapsedPercent);

public sealed record PosChainLiveReportDto(
    DateTimeOffset AsOf, int StoreCount, int OpenShiftCount,
    decimal TotalTodayRevenue, decimal TotalMonthRevenue, decimal TotalTarget,
    decimal TotalAttainmentPercent,
    IReadOnlyList<PosChainLiveRowDto> Rows);

public sealed record PosCostVarianceReportDto(
    decimal TotalTheoreticalCost, decimal TotalActualCost,
    decimal TotalVarianceCost, decimal TotalVariancePercent,
    IReadOnlyList<PosCostVarianceRowDto> Rows);
