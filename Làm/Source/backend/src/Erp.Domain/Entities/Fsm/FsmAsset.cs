using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Thiết bị tại khách — install base (UC_FSM_008–009).</summary>
public class FsmAsset : TenantEntity
{
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public string SerialNo { get; set; } = "";
    public string? Model { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? WarrantyEndAt { get; set; }
    /// <summary>Active · Inactive · Scrapped</summary>
    public string Status { get; set; } = "Active";
    public string? Address { get; set; }
    public string? Note { get; set; }
}
