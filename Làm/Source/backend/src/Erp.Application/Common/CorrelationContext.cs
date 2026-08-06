namespace Erp.Application.Common;

/// <summary>Correlation id theo request (AsyncLocal) — set bởi middleware API.</summary>
public static class CorrelationContext
{
    private static readonly AsyncLocal<Guid?> CurrentLocal = new();

    public static Guid? Current
    {
        get => CurrentLocal.Value;
        set => CurrentLocal.Value = value;
    }
}
