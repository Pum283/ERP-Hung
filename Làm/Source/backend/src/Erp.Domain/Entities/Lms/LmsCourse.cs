using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Khóa học (UC_LMS_002–003, 009).</summary>
public class LmsCourse : TenantEntity
{
    public Guid? ProgramId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Summary { get; set; }
    /// <summary>Online · Offline · Blended</summary>
    public string DeliveryMode { get; set; } = "Online";
    /// <summary>Draft · Published · Hidden</summary>
    public string Status { get; set; } = "Draft";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public string? CoverUrl { get; set; }
}
