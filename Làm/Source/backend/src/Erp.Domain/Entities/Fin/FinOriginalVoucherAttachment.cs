using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Đính kèm chứng từ gốc (hóa đơn VAT, hợp đồng, phiếu chi) vào bút toán sổ cái (UC_FIN_017).</summary>
public class FinOriginalVoucherAttachment : TenantEntity
{
    public Guid JournalEntryId { get; set; }
    public string VoucherNumber { get; set; } = "PKT-2026-0814";
    public string AttachmentName { get; set; } = "Hóa đơn giá trị gia tăng số 0001288 (PDF gốc)";
    public string FileUrl { get; set; } = "/uploads/fin/vouchers/inv-0001288-signed.pdf";
    public string MimeType { get; set; } = "application/pdf";
    public long FileSizeBytes { get; set; } = 850000;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
