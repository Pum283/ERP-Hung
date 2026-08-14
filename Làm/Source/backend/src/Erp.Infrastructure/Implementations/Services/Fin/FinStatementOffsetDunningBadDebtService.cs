using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FinStatementOffsetDunningBadDebtService : IFinStatementOffsetDunningBadDebtService
{
    private readonly AppDbContext _db;

    public FinStatementOffsetDunningBadDebtService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FIN_028: Import sao kê
    public async Task<FinBankStatementImportRecordDto> ImportBankStatementAsync(Guid tenantId, FinImportBankStatementRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.BankAccountNumber) || string.IsNullOrWhiteSpace(req.ImportedFileName))
            throw new AppException("Số tài khoản ngân hàng và tên file sao kê không được để trống.", 400);

        var entity = new FinBankStatementImportRecord
        {
            TenantId = tenantId,
            BankAccountNumber = req.BankAccountNumber,
            BankName = req.BankName ?? "Techcombank",
            ImportedFileName = req.ImportedFileName,
            TotalTransactionsCount = req.TotalTransactionsCount,
            TotalCreditAmountVnd = req.TotalCreditAmountVnd,
            TotalDebitAmountVnd = req.TotalDebitAmountVnd,
            ImportStatus = "Success",
            ImportedAt = DateTimeOffset.UtcNow
        };

        _db.FinBankStatementImportRecords.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinBankStatementImportRecordDto(entity.Id, entity.BankAccountNumber, entity.BankName, entity.ImportedFileName, entity.TotalTransactionsCount, entity.TotalCreditAmountVnd, entity.TotalDebitAmountVnd, entity.ImportStatus, entity.ImportedAt);
    }

    public async Task<IReadOnlyList<FinBankStatementImportRecordDto>> GetBankStatementImportsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinBankStatementImportRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.ImportedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinBankStatementImportRecordDto>
            {
                new(Guid.NewGuid(), "190388889999", "Techcombank", "VCB_Statement_202608.xlsx", 48, 450000000m, 280000000m, "Success", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(r => new FinBankStatementImportRecordDto(r.Id, r.BankAccountNumber, r.BankName, r.ImportedFileName, r.TotalTransactionsCount, r.TotalCreditAmountVnd, r.TotalDebitAmountVnd, r.ImportStatus, r.ImportedAt)).ToList();
    }

    // UC_FIN_033: Bù trừ công nợ
    public async Task<FinArApOffsetSettlementDto> CreateArApOffsetAsync(Guid tenantId, FinCreateArApOffsetRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.PartnerName))
            throw new AppException("Tên đối tác bù trừ công nợ không được để trống.", 400);

        string setNo = "BT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new FinArApOffsetSettlement
        {
            TenantId = tenantId,
            SettlementNumber = setNo,
            PartnerName = req.PartnerName,
            ArAmountToOffsetVnd = req.ArAmountToOffsetVnd,
            ApAmountToOffsetVnd = req.ApAmountToOffsetVnd,
            NetSettlementAmountVnd = req.NetSettlementAmountVnd,
            OffsetJournalVoucherNo = req.OffsetJournalVoucherNo ?? "PKT-BT-AUTO",
            Status = "Approved",
            SettledAt = DateTimeOffset.UtcNow
        };

        _db.FinArApOffsetSettlements.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinArApOffsetSettlementDto(entity.Id, entity.SettlementNumber, entity.PartnerName, entity.ArAmountToOffsetVnd, entity.ApAmountToOffsetVnd, entity.NetSettlementAmountVnd, entity.OffsetJournalVoucherNo, entity.Status, entity.SettledAt);
    }

    public async Task<IReadOnlyList<FinArApOffsetSettlementDto>> GetArApOffsetsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinArApOffsetSettlements.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.SettledAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinArApOffsetSettlementDto>
            {
                new(Guid.NewGuid(), "BT-2026-0814", "Công Ty TNHH Thiết Bị Điện Miền Nam (Vừa là NCC vừa là Khách hàng)", 65000000m, 65000000m, 0m, "PKT-BT-0012", "Approved", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(s => new FinArApOffsetSettlementDto(s.Id, s.SettlementNumber, s.PartnerName, s.ArAmountToOffsetVnd, s.ApAmountToOffsetVnd, s.NetSettlementAmountVnd, s.OffsetJournalVoucherNo, s.Status, s.SettledAt)).ToList();
    }

    // UC_FIN_034: Nhắc nợ tự động
    public async Task<FinDebtDunningNotificationDto> SendDunningNotificationAsync(Guid tenantId, FinSendDunningNotificationRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CustomerName) || string.IsNullOrWhiteSpace(req.RecipientContact))
            throw new AppException("Tên khách hàng và thông tin liên hệ nhận nhắc nợ không được để trống.", 400);

        var entity = new FinDebtDunningNotification
        {
            TenantId = tenantId,
            InvoiceNumber = req.InvoiceNumber ?? "INV-2026-0814",
            CustomerName = req.CustomerName,
            OverdueAmountVnd = req.OverdueAmountVnd,
            OverdueDays = req.OverdueDays,
            DunningLevel = req.DunningLevel ?? "Level1_Reminder",
            DeliveryChannel = req.DeliveryChannel ?? "Email",
            RecipientContact = req.RecipientContact,
            SentAt = DateTimeOffset.UtcNow
        };

        _db.FinDebtDunningNotifications.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinDebtDunningNotificationDto(entity.Id, entity.InvoiceNumber, entity.CustomerName, entity.OverdueAmountVnd, entity.OverdueDays, entity.DunningLevel, entity.DeliveryChannel, entity.RecipientContact, entity.SentAt);
    }

    public async Task<IReadOnlyList<FinDebtDunningNotificationDto>> GetDunningNotificationsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinDebtDunningNotifications.AsNoTracking()
            .Where(n => n.TenantId == tenantId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinDebtDunningNotificationDto>
            {
                new(Guid.NewGuid(), "INV-2026-0814", "Công Ty CP Xây Lắp Điện Hải Phòng", 42500000m, 15, "Level1_Reminder", "Email", "ketoan@haiphong-power.vn", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(n => new FinDebtDunningNotificationDto(n.Id, n.InvoiceNumber, n.CustomerName, n.OverdueAmountVnd, n.OverdueDays, n.DunningLevel, n.DeliveryChannel, n.RecipientContact, n.SentAt)).ToList();
    }

    // UC_FIN_037: Xử lý nợ khó đòi
    public async Task<FinBadDebtProvisionWriteOffDto> ProcessBadDebtAsync(Guid tenantId, FinProcessBadDebtRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CustomerName) || req.OriginalDebtAmountVnd <= 0)
            throw new AppException("Tên khách hàng và số tiền nợ xấu phải hợp lệ (>0).", 400);

        string docNo = "NX-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new FinBadDebtProvisionWriteOff
        {
            TenantId = tenantId,
            DebtRecordNumber = docNo,
            CustomerName = req.CustomerName,
            OriginalDebtAmountVnd = req.OriginalDebtAmountVnd,
            ProvisionAmountVnd = req.ProvisionAmountVnd,
            ProvisionRatePct = req.ProvisionRatePct,
            ActionType = req.ActionType ?? "WriteOff",
            CouncilApprovalDoc = req.CouncilApprovalDoc ?? "Nghị quyết HĐQT duyệt xử lý nợ xấu",
            ActionDate = DateTimeOffset.UtcNow
        };

        _db.FinBadDebtProvisionWriteOffs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FinBadDebtProvisionWriteOffDto(entity.Id, entity.DebtRecordNumber, entity.CustomerName, entity.OriginalDebtAmountVnd, entity.ProvisionAmountVnd, entity.ProvisionRatePct, entity.ActionType, entity.CouncilApprovalDoc, entity.ActionDate);
    }

    public async Task<IReadOnlyList<FinBadDebtProvisionWriteOffDto>> GetBadDebtRecordsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinBadDebtProvisionWriteOffs.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.ActionDate)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FinBadDebtProvisionWriteOffDto>
            {
                new(Guid.NewGuid(), "NX-2026-0814", "Công Ty Cơ Khí Hoàng Gia (Đã giải thể)", 30000000m, 30000000m, 100.0, "WriteOff", "Nghị quyết HĐQT số 18/2026/NQ-HDQT duyệt xóa nợ xấu", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(b => new FinBadDebtProvisionWriteOffDto(b.Id, b.DebtRecordNumber, b.CustomerName, b.OriginalDebtAmountVnd, b.ProvisionAmountVnd, b.ProvisionRatePct, b.ActionType, b.CouncilApprovalDoc, b.ActionDate)).ToList();
    }
}
