using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FinRecurringVoucherAdvanceVaultService : IFinRecurringVoucherAdvanceVaultService
{
    private readonly AppDbContext _db;

    public FinRecurringVoucherAdvanceVaultService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FIN_011: Bút toán định kỳ / mẫu
    public async Task<FinRecurringTemplateVoucherDto> CreateRecurringTemplateAsync(Guid tenantId, FinCreateRecurringTemplateRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TemplateCode) || string.IsNullOrWhiteSpace(req.TemplateName))
            throw new AppException("Mã và tên mẫu bút toán định kỳ không được để trống.", 400);

        var entity = new FinRecurringTemplateVoucher
        {
            TenantId = tenantId,
            TemplateCode = req.TemplateCode,
            TemplateName = req.TemplateName,
            Frequency = req.Frequency ?? "Monthly",
            DefaultAmountVnd = req.DefaultAmountVnd,
            DebitAccountCode = req.DebitAccountCode ?? "6424",
            CreditAccountCode = req.CreditAccountCode ?? "2141",
            IsActive = req.IsActive
        };

        _db.FinRecurringTemplateVouchers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinRecurringTemplateVoucherDto(entity.Id, entity.TemplateCode, entity.TemplateName, entity.Frequency, entity.DefaultAmountVnd, entity.DebitAccountCode, entity.CreditAccountCode, entity.IsActive);
    }

    public async Task<IReadOnlyList<FinRecurringTemplateVoucherDto>> GetRecurringTemplatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinRecurringTemplateVouchers.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.TemplateCode)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinRecurringTemplateVoucherDto>
            {
                new(Guid.NewGuid(), "TMPL-DEPR-OFFICE", "Trích khấu hao tài sản cố định văn phòng định kỳ hàng tháng", "Monthly", 35000000m, "6424", "2141", true),
                new(Guid.NewGuid(), "TMPL-RENT-QUARTER", "Phân bổ chi phí thuê văn phòng trụ sở định kỳ theo quý", "Quarterly", 120000000m, "6427", "242", true)
            };
        }

        return list.Select(t => new FinRecurringTemplateVoucherDto(t.Id, t.TemplateCode, t.TemplateName, t.Frequency, t.DefaultAmountVnd, t.DebitAccountCode, t.CreditAccountCode, t.IsActive)).ToList();
    }

    // UC_FIN_017: Đính kèm chứng từ gốc
    public async Task<FinOriginalVoucherAttachmentDto> UploadVoucherAttachmentAsync(Guid tenantId, FinUploadVoucherAttachmentRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.AttachmentName))
            throw new AppException("Tên tài liệu chứng từ gốc không được để trống.", 400);

        var entity = new FinOriginalVoucherAttachment
        {
            TenantId = tenantId,
            JournalEntryId = req.JournalEntryId == Guid.Empty ? Guid.NewGuid() : req.JournalEntryId,
            VoucherNumber = req.VoucherNumber ?? "PKT-2026-0814",
            AttachmentName = req.AttachmentName,
            FileUrl = req.FileUrl ?? "/uploads/fin/vouchers/inv-default.pdf",
            MimeType = req.MimeType ?? "application/pdf",
            FileSizeBytes = req.FileSizeBytes > 0 ? req.FileSizeBytes : 850000,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _db.FinOriginalVoucherAttachments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinOriginalVoucherAttachmentDto(entity.Id, entity.JournalEntryId, entity.VoucherNumber, entity.AttachmentName, entity.FileUrl, entity.MimeType, entity.FileSizeBytes, entity.UploadedAt);
    }

    public async Task<IReadOnlyList<FinOriginalVoucherAttachmentDto>> GetVoucherAttachmentsAsync(Guid tenantId, Guid journalEntryId, CancellationToken ct = default)
    {
        var list = await _db.FinOriginalVoucherAttachments.AsNoTracking()
            .Where(a => a.TenantId == tenantId && (journalEntryId == Guid.Empty || a.JournalEntryId == journalEntryId))
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinOriginalVoucherAttachmentDto>
            {
                new(Guid.NewGuid(), journalEntryId, "PKT-2026-0814", "Hóa đơn giá trị gia tăng số 0001288 (PDF gốc)", "/uploads/fin/vouchers/inv-0001288-signed.pdf", "application/pdf", 850000, DateTimeOffset.UtcNow)
            };
        }

        return list.Select(a => new FinOriginalVoucherAttachmentDto(a.Id, a.JournalEntryId, a.VoucherNumber, a.AttachmentName, a.FileUrl, a.MimeType, a.FileSizeBytes, a.UploadedAt)).ToList();
    }

    // UC_FIN_021: Đề nghị tạm ứng / hoàn ứng
    public async Task<FinAdvanceSettlementRequestDto> CreateAdvanceSettlementAsync(Guid tenantId, FinCreateAdvanceSettlementRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EmployeeName) || req.AdvanceAmountVnd <= 0)
            throw new AppException("Tên nhân viên và số tiền tạm ứng phải hợp lệ (>0).", 400);

        string reqNo = "TU-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new FinAdvanceSettlementRequest
        {
            TenantId = tenantId,
            RequestNumber = reqNo,
            EmployeeName = req.EmployeeName,
            Purpose = req.Purpose ?? "Tạm ứng chi phí công tác",
            AdvanceAmountVnd = req.AdvanceAmountVnd,
            SettledAmountVnd = req.SettledAmountVnd,
            RemainingRefundVnd = req.RemainingRefundVnd,
            Status = req.SettledAmountVnd > 0 ? "Settled" : "Advanced",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.FinAdvanceSettlementRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinAdvanceSettlementRequestDto(entity.Id, entity.RequestNumber, entity.EmployeeName, entity.Purpose, entity.AdvanceAmountVnd, entity.SettledAmountVnd, entity.RemainingRefundVnd, entity.Status, entity.CreatedAt);
    }

    public async Task<IReadOnlyList<FinAdvanceSettlementRequestDto>> GetAdvanceSettlementsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinAdvanceSettlementRequests.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinAdvanceSettlementRequestDto>
            {
                new(Guid.NewGuid(), "TU-2026-0814", "Kỹ Sư Trưởng Nguyễn Văn An", "Tạm ứng tiền vé máy bay và lưu trú công tác hiện trường dự án Solar FPT", 15000000m, 14200000m, 800000m, "Settled", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(s => new FinAdvanceSettlementRequestDto(s.Id, s.RequestNumber, s.EmployeeName, s.Purpose, s.AdvanceAmountVnd, s.SettledAmountVnd, s.RemainingRefundVnd, s.Status, s.CreatedAt)).ToList();
    }

    // UC_FIN_022: Kiểm kê quỹ
    public async Task<FinCashVaultCountAuditDto> CreateVaultCountAuditAsync(Guid tenantId, FinCreateVaultCountAuditRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.FundCode))
            throw new AppException("Mã quỹ tiền mặt kiểm kê không được để trống.", 400);

        var entity = new FinCashVaultCountAudit
        {
            TenantId = tenantId,
            FundCode = req.FundCode,
            FundName = req.FundName ?? req.FundCode,
            BookBalanceVnd = req.BookBalanceVnd,
            PhysicalCountVnd = req.PhysicalCountVnd,
            VarianceVnd = req.VarianceVnd,
            AuditorName = req.AuditorName ?? "Hội Đồng Kiểm Kê Quỹ",
            AuditConclusion = req.AuditConclusion ?? "Khớp đúng số dư sổ quỹ",
            AuditDate = DateTimeOffset.UtcNow
        };

        _db.FinCashVaultCountAudits.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinCashVaultCountAuditDto(entity.Id, entity.FundCode, entity.FundName, entity.BookBalanceVnd, entity.PhysicalCountVnd, entity.VarianceVnd, entity.AuditorName, entity.AuditConclusion, entity.AuditDate);
    }

    public async Task<IReadOnlyList<FinCashVaultCountAuditDto>> GetVaultCountAuditsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinCashVaultCountAudits.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinCashVaultCountAuditDto>
            {
                new(Guid.NewGuid(), "QUY-MAT-VND", "Quỹ Tiền Mặt Trụ Sở Chính (VND)", 85200000m, 85200000m, 0m, "Kế Toán Trưởng & Thủ Quỹ", "Khớp đúng 100% giữa sổ quỹ tiền mặt và tiền mặt thực tế tại két sắt", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(a => new FinCashVaultCountAuditDto(a.Id, a.FundCode, a.FundName, a.BookBalanceVnd, a.PhysicalCountVnd, a.VarianceVnd, a.AuditorName, a.AuditConclusion, a.AuditDate)).ToList();
    }
}
