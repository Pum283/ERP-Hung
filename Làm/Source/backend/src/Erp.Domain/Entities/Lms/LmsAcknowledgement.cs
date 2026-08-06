using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Xác nhận đã đọc nội quy / tài liệu (UC_LMS_058).</summary>
public class LmsAcknowledgement : TenantEntity
{
    public Guid EmployeeId { get; set; }
    /// <summary>ID của tài liệu / nội quy cần xác nhận.</summary>
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = "";
    public DateTimeOffset AcknowledgedAt { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Thời hạn hết hiệu lực (cần xác nhận lại).</summary>
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? IpAddress { get; set; }
}
