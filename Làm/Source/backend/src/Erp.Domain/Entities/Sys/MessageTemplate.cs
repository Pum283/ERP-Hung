using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class MessageTemplate : TenantEntity
{
    public string Code { get; set; } = "";
    public string Channel { get; set; } = "Email";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
