namespace Erp.Infrastructure.Implementations.Services.Auth;

/// <summary>Điều kiện UserRole hiệu lực — khớp Digi (IsActive · RevokedAt · ValidFrom/To).</summary>
internal static class AuthzQuery
{
    public static DateTimeOffset Now => DateTimeOffset.UtcNow;

    /// <summary>Biểu thức lọc dùng trong LINQ to Entities.</summary>
    public static bool IsEffective(
        bool isActive,
        bool isDeleted,
        DateTimeOffset? revokedAt,
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo,
        DateTimeOffset now)
        => isActive && !isDeleted
           && revokedAt == null
           && (validFrom == null || validFrom <= now)
           && (validTo == null || validTo >= now);
}
