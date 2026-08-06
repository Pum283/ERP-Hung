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
