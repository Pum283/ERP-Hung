using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Câu hỏi trong NHCH (UC_LMS_010).</summary>
public class LmsQuestion : TenantEntity
{
    public string Code { get; set; } = "";
    public string Stem { get; set; } = "";
    /// <summary>SingleChoice · TrueFalse</summary>
    public string QuestionType { get; set; } = "SingleChoice";
    /// <summary>JSON [{ "key":"A","text":"..." }, ...]</summary>
    public string OptionsJson { get; set; } = "[]";
    /// <summary>JSON ["A"] hoặc ["A","B"]</summary>
    public string CorrectKeysJson { get; set; } = "[]";
    public decimal Points { get; set; } = 1;
    public string? Tag { get; set; }
    public bool IsActive { get; set; } = true;
}
