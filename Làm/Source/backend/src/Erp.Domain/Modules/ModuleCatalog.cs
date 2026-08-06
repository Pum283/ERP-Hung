namespace Erp.Domain.Modules;

/// <summary>
/// Catalog mã module bán được — khớp <c>Làm/Source/MODULES.json</c>.
/// Seed license / menu dùng danh sách này; không hard-code mảng rải rác.
/// </summary>
public static class ModuleCatalog
{
    /// <summary>SYS luôn có trong mọi SKU.</summary>
    public const string Sys = "SYS";

    /// <summary>Kit Day-1 (masters/docs) — nền, không bán riêng.</summary>
    public const string ModKit = "MOD";

    /// <summary>16 mã license (không gồm MOD kit).</summary>
    public static readonly IReadOnlyList<string> SellableCodes =
    [
        "SYS", "HRM", "CRM", "INV", "FIN", "WF", "LMS", "AST",
        "POS", "PUR", "LOG", "MFG", "FSM", "PJM", "BI", "PRT"
    ];

    public static bool IsSellable(string code) =>
        SellableCodes.Any(c => string.Equals(c, code, StringComparison.OrdinalIgnoreCase));
}
