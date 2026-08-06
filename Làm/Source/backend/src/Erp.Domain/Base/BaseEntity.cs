namespace Erp.Domain.Base;

/// <summary>Khóa chính + audit tối thiểu (khớp DDD cột chuẩn).</summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; } = 1;
}

/// <summary>Thực thể thuộc tenant (đa thuê bao).</summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}
