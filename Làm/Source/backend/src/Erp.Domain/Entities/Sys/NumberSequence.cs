using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class NumberSequence : TenantEntity
{
    public string DocType { get; set; } = "";
    public string Pattern { get; set; } = "{yyyy}-{seq:5}";
    public int NextValue { get; set; } = 1;
    public int ResetYear { get; set; }
}
