using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Tài khoản ngân hàng (UC_FIN_024).</summary>
public class FinBankAccount : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string? BranchName { get; set; }
    public Guid GlAccountId { get; set; }
    public decimal OpeningBalance { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

/// <summary>Giấy báo Có/Nợ ngân hàng (UC_FIN_025).</summary>
public class FinBankVoucher : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid BankAccountId { get; set; }
    /// <summary>Credit · Debit (báo Có / báo Nợ)</summary>
    public string VoucherType { get; set; } = "Credit";
    public DateTimeOffset DocDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string? BankRef { get; set; }
    public string? PartnerCode { get; set; }
    public Guid? CounterAccountId { get; set; }
    public Guid? PeriodId { get; set; }
    /// <summary>Draft · Posted · Void</summary>
    public string Status { get; set; } = "Draft";
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? TransferRequestId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}

/// <summary>Đề nghị chuyển khoản (UC_FIN_027).</summary>
public class FinBankTransferRequest : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid FromBankAccountId { get; set; }
    public string BeneficiaryName { get; set; } = "";
    public string BeneficiaryAccount { get; set; } = "";
    public string BeneficiaryBank { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public DateTimeOffset RequestDate { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CounterAccountId { get; set; }
    public Guid? PeriodId { get; set; }
    /// <summary>Draft · Submitted · Approved · Executed · Rejected · Void</summary>
    public string Status { get; set; } = "Draft";
    public Guid? ExecutedVoucherId { get; set; }
    public string? ExecutedVoucherCode { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? Note { get; set; }
}

/// <summary>Dòng sao kê để đối soát (UC_FIN_026) — nhập tay Cap-2 (import Should sau).</summary>
public class FinBankStatementLine : TenantEntity
{
    public Guid BankAccountId { get; set; }
    public DateTimeOffset StmtDate { get; set; }
    public string Description { get; set; } = "";
    public string? BankRef { get; set; }
    /// <summary>Credit · Debit</summary>
    public string Direction { get; set; } = "Credit";
    public decimal Amount { get; set; }
    /// <summary>Unmatched · Matched · Ignored</summary>
    public string Status { get; set; } = "Unmatched";
    public Guid? MatchedVoucherId { get; set; }
    public string? MatchedVoucherCode { get; set; }
    public DateTimeOffset? MatchedAt { get; set; }
    public string? Note { get; set; }
}
