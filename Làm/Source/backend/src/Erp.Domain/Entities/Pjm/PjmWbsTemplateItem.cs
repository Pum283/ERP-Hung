using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

public class PjmWbsTemplateItem : TenantEntity
{
    public Guid TemplateId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? ParentItemId { get; set; }
    public int SortOrder { get; set; }
}
