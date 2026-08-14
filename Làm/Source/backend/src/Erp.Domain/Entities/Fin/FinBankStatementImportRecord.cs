using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Lịch sử import file sao kê ngân hàng và kết quả đối soát tự động (UC_FIN_028).</summary>
public class FinBankStatementImportRecord : TenantEntity
{
    public string BankAccountNumber { get; set; } = "190388889999";
    public string BankName { get; set; } = "Techcombank";
    public string ImportedFileName { get; set; } = "VCB_Statement_202608.xlsx";
    public int TotalTransactionsCount { get; set; } = 48;
    public decimal TotalCreditAmountVnd { get; set; } = 450000000;
    public decimal TotalDebitAmountVnd { get; set; } = 280000000;
    public string ImportStatus { get; set; } = "Success"; // Success | Failed | Partial
    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
}
