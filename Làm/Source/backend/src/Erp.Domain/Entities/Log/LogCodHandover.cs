using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Bàn giao / nộp tiền COD (UC_LOG_023–026).</summary>
public class LogCodHandover : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Draft · Submitted · Reconciled · Variance</summary>
    public string Status { get; set; } = "Draft";
    public Guid? DriverUserId { get; set; }
    public string? DriverName { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal RemittedAmount { get; set; }
    public decimal VarianceAmount { get; set; }
    public string? Note { get; set; }
    public string? VarianceNote { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReconciledAt { get; set; }
    public Guid CreatedByUserId { get; set; }
}

/// <summary>Dòng bàn giao COD gắn lệnh giao.</summary>
public class LogCodHandoverLine : TenantEntity
{
    public Guid HandoverId { get; set; }
    public Guid DeliveryOrderId { get; set; }
    public decimal CodAmount { get; set; }
    public string? Note { get; set; }
}
