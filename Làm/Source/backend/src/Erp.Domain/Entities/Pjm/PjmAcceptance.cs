using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Biên bản nghiệm thu giai đoạn / cuối (UC_PJM_031–033).</summary>
public class PjmAcceptance : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = "";
    /// <summary>Phase | Final</summary>
    public string Kind { get; set; } = "Phase";
    public string Title { get; set; } = "";
    /// <summary>Draft | Signed</summary>
    public string Status { get; set; } = "Draft";
    public string? SignerName { get; set; }
    public DateTimeOffset? SignedAt { get; set; }
    public string? Note { get; set; }
}