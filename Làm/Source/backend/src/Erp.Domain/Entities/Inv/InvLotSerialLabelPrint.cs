using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>In tem lô / serial (UC_INV_023).</summary>
public class InvLotSerialLabelPrint : TenantEntity
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string LotNumber { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public DateTimeOffset ManufactureDate { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public string LabelTemplate { get; set; } = "LotSerial-60x40mm";
    public DateTimeOffset PrintedAt { get; set; } = DateTimeOffset.UtcNow;
}
