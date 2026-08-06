using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Dự án (UC_PJM_005–009).</summary>
public class PjmProject : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? ProjectTypeId { get; set; }
    /// <summary>Mã trạng thái chuẩn (Draft/Active/...).</summary>
    public string StatusCode { get; set; } = "Draft";
    public string? CustomerName { get; set; }
    public string? ContractCode { get; set; }
    /// <summary>Cơ hội CRM nguồn (Cap-1 nhập mã).</summary>
    public string? SourceOpportunityCode { get; set; }
    public Guid? PmUserId { get; set; }
    public string? PmName { get; set; }
    public decimal Budget { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Note { get; set; }
    public Guid CreatedByUserId { get; set; }

    /// <summary>Doanh thu ghi nhận (UC_PJM_034) — soft local.</summary>
    public decimal RecognizedRevenue { get; set; }
    public DateTimeOffset? RevenueRecognizedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByUserId { get; set; }
}
