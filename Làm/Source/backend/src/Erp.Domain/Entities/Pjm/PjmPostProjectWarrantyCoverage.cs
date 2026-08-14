using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Bảo hành sau dự án và chuyển giao chăm sóc khách hàng (UC_PJM_037).</summary>
public class PjmPostProjectWarrantyCoverage : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string CustomerName { get; set; } = "Công Ty Viễn Thông Viettel";
    public DateTimeOffset WarrantyStartDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset WarrantyEndDate { get; set; } = DateTimeOffset.UtcNow.AddMonths(24);
    public int WarrantyPeriodMonths { get; set; } = 24;
    public string SupportHotline { get; set; } = "1900-8888";
    public bool IsActive { get; set; } = true;
}
