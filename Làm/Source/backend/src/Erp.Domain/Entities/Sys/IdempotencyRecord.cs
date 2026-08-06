using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Idempotency key cho API đồng bộ (header Idempotency-Key).</summary>
public class IdempotencyRecord : TenantEntity
{
    public string Key { get; set; } = "";
    public string RequestPath { get; set; } = "";
    public int ResponseStatus { get; set; }
    public string? ResponseBody { get; set; }
}
