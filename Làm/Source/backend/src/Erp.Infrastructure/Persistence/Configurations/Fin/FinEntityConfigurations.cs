using Erp.Domain.Entities.Fin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Fin;

public sealed class FinAccountGroupConfig : IEntityTypeConfiguration<FinAccountGroup>
{
    public void Configure(EntityTypeBuilder<FinAccountGroup> b)
    {
        b.ToTable("account_group", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class FinAccountConfig : IEntityTypeConfiguration<FinAccount>
{
    public void Configure(EntityTypeBuilder<FinAccount> b)
    {
        b.ToTable("account", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.AccountType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinFiscalYearConfig : IEntityTypeConfiguration<FinFiscalYear>
{
    public void Configure(EntityTypeBuilder<FinFiscalYear> b)
    {
        b.ToTable("fiscal_year", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class FinPeriodConfig : IEntityTypeConfiguration<FinPeriod>
{
    public void Configure(EntityTypeBuilder<FinPeriod> b)
    {
        b.ToTable("period", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class FinCostCenterConfig : IEntityTypeConfiguration<FinCostCenter>
{
    public void Configure(EntityTypeBuilder<FinCostCenter> b)
    {
        b.ToTable("cost_center", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinPaymentMethodConfig : IEntityTypeConfiguration<FinPaymentMethod>
{
    public void Configure(EntityTypeBuilder<FinPaymentMethod> b)
    {
        b.ToTable("payment_method", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class FinTaxConfig : IEntityTypeConfiguration<FinTax>
{
    public void Configure(EntityTypeBuilder<FinTax> b)
    {
        b.ToTable("tax", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.RatePercent).HasPrecision(9, 4);
        b.Property(x => x.TaxType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinVatDocumentConfig : IEntityTypeConfiguration<FinVatDocument>
{
    public void Configure(EntityTypeBuilder<FinVatDocument> b)
    {
        b.ToTable("vat_document", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Direction, x.Status });
        b.HasIndex(x => new { x.TenantId, x.PeriodId, x.Direction });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Direction).HasMaxLength(20).IsRequired();
        b.Property(x => x.RatePercent).HasPrecision(9, 4);
        b.Property(x => x.InvoiceNo).HasMaxLength(80).IsRequired();
        b.Property(x => x.InvoiceSeries).HasMaxLength(40);
        b.Property(x => x.PartnerCode).HasMaxLength(40);
        b.Property(x => x.PartnerName).HasMaxLength(200);
        b.Property(x => x.PartnerTaxCode).HasMaxLength(40);
        b.Property(x => x.TaxableAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinRevenueDocumentConfig : IEntityTypeConfiguration<FinRevenueDocument>
{
    public void Configure(EntityTypeBuilder<FinRevenueDocument> b)
    {
        b.ToTable("revenue_document", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Kind, x.Status });
        b.HasIndex(x => new { x.TenantId, x.SourceModule, x.SourceId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Kind).HasMaxLength(20).IsRequired();
        b.Property(x => x.SourceModule).HasMaxLength(20).IsRequired();
        b.Property(x => x.SourceCode).HasMaxLength(40);
        b.Property(x => x.RevenueAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.CogsAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinJournalConfig : IEntityTypeConfiguration<FinJournal>
{
    public void Configure(EntityTypeBuilder<FinJournal> b)
    {
        b.ToTable("journal", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Source).HasMaxLength(20).IsRequired();
        b.Property(x => x.PartnerCode).HasMaxLength(40);
    }
}

public sealed class FinJournalLineConfig : IEntityTypeConfiguration<FinJournalLine>
{
    public void Configure(EntityTypeBuilder<FinJournalLine> b)
    {
        b.ToTable("journal_line", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.JournalId, x.LineNo });
        b.Property(x => x.Debit).HasPrecision(18, 2);
        b.Property(x => x.Credit).HasPrecision(18, 2);
        b.Property(x => x.PartnerCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class FinCashFundConfig : IEntityTypeConfiguration<FinCashFund>
{
    public void Configure(EntityTypeBuilder<FinCashFund> b)
    {
        b.ToTable("cash_fund", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CustodianName).HasMaxLength(120);
        b.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinCashVoucherConfig : IEntityTypeConfiguration<FinCashVoucher>
{
    public void Configure(EntityTypeBuilder<FinCashVoucher> b)
    {
        b.ToTable("cash_voucher", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.FundId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.VoucherType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.PartnerCode).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinBankAccountConfig : IEntityTypeConfiguration<FinBankAccount>
{
    public void Configure(EntityTypeBuilder<FinBankAccount> b)
    {
        b.ToTable("bank_account", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.BankName).HasMaxLength(200).IsRequired();
        b.Property(x => x.AccountNumber).HasMaxLength(60).IsRequired();
        b.Property(x => x.BranchName).HasMaxLength(200);
        b.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinBankVoucherConfig : IEntityTypeConfiguration<FinBankVoucher>
{
    public void Configure(EntityTypeBuilder<FinBankVoucher> b)
    {
        b.ToTable("bank_voucher", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.BankAccountId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.VoucherType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.BankRef).HasMaxLength(80);
        b.Property(x => x.PartnerCode).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinBankTransferRequestConfig : IEntityTypeConfiguration<FinBankTransferRequest>
{
    public void Configure(EntityTypeBuilder<FinBankTransferRequest> b)
    {
        b.ToTable("bank_transfer_request", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.FromBankAccountId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.BeneficiaryName).HasMaxLength(200).IsRequired();
        b.Property(x => x.BeneficiaryAccount).HasMaxLength(60).IsRequired();
        b.Property(x => x.BeneficiaryBank).HasMaxLength(200).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ExecutedVoucherCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinBankStatementLineConfig : IEntityTypeConfiguration<FinBankStatementLine>
{
    public void Configure(EntityTypeBuilder<FinBankStatementLine> b)
    {
        b.ToTable("bank_statement_line", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.BankAccountId, x.Status });
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.BankRef).HasMaxLength(80);
        b.Property(x => x.Direction).HasMaxLength(20).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.MatchedVoucherCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinApInvoiceConfig : IEntityTypeConfiguration<FinApInvoice>
{
    public void Configure(EntityTypeBuilder<FinApInvoice> b)
    {
        b.ToTable("ap_invoice", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.VendorId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.VendorInvoiceNo).HasMaxLength(80);
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.PaidAmount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinApPaymentRequestConfig : IEntityTypeConfiguration<FinApPaymentRequest>
{
    public void Configure(EntityTypeBuilder<FinApPaymentRequest> b)
    {
        b.ToTable("ap_payment_request", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.VendorId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.RequestAmount).HasPrecision(18, 2);
        b.Property(x => x.PayMethod).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.PaymentCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinApPaymentRequestLineConfig : IEntityTypeConfiguration<FinApPaymentRequestLine>
{
    public void Configure(EntityTypeBuilder<FinApPaymentRequestLine> b)
    {
        b.ToTable("ap_payment_request_line", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PaymentRequestId });
        b.Property(x => x.Amount).HasPrecision(18, 2);
    }
}

public sealed class FinApPaymentConfig : IEntityTypeConfiguration<FinApPayment>
{
    public void Configure(EntityTypeBuilder<FinApPayment> b)
    {
        b.ToTable("ap_payment", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.VendorId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.PayMethod).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinApPaymentAllocationConfig : IEntityTypeConfiguration<FinApPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<FinApPaymentAllocation> b)
    {
        b.ToTable("ap_payment_allocation", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PaymentId });
        b.Property(x => x.Amount).HasPrecision(18, 2);
    }
}

public sealed class FinArInvoiceConfig : IEntityTypeConfiguration<FinArInvoice>
{
    public void Configure(EntityTypeBuilder<FinArInvoice> b)
    {
        b.ToTable("ar_invoice", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.CustomerId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.CustomerInvoiceNo).HasMaxLength(80);
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.ReceivedAmount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinArCreditLimitConfig : IEntityTypeConfiguration<FinArCreditLimit>
{
    public void Configure(EntityTypeBuilder<FinArCreditLimit> b)
    {
        b.ToTable("ar_credit_limit", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CustomerId }).IsUnique();
        b.Property(x => x.CreditLimit).HasPrecision(18, 2);
        b.Property(x => x.WarningPercent).HasPrecision(5, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinArReceiptConfig : IEntityTypeConfiguration<FinArReceipt>
{
    public void Configure(EntityTypeBuilder<FinArReceipt> b)
    {
        b.ToTable("ar_receipt", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.CustomerId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.PayMethod).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FinArReceiptAllocationConfig : IEntityTypeConfiguration<FinArReceiptAllocation>
{
    public void Configure(EntityTypeBuilder<FinArReceiptAllocation> b)
    {
        b.ToTable("ar_receipt_allocation", "fin");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReceiptId });
        b.Property(x => x.Amount).HasPrecision(18, 2);
    }
}
