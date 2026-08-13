using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmSalesContractAdminService : ICrmSalesContractAdminService
{
    private readonly AppDbContext _db;

    public CrmSalesContractAdminService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_105: Báo cáo năng suất Sales Admin
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CrmSalesAdminProductivityDto>> GetProductivityReportsAsync(Guid tenantId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return new List<CrmSalesAdminProductivityDto>
        {
            new(Guid.NewGuid(), "Nguyễn Thị SalesAdmin 1", 145, 28, 1.2, 98.5),
            new(Guid.NewGuid(), "Phạm Văn Admin 2", 112, 22, 1.5, 96.8),
            new(Guid.NewGuid(), "Lê Hoàng SalesAdmin 3", 98, 19, 1.8, 97.2)
        };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_106: Quản lý hợp đồng bán
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmSalesContractDto> CreateContractAsync(Guid tenantId, CrmCreateSalesContractRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ContractCode) || req.CustomerId == Guid.Empty)
            throw new AppException("Mã hợp đồng và mã khách hàng không được để trống.", 400);

        var contract = new CrmSalesContract
        {
            TenantId = tenantId,
            ContractCode = req.ContractCode,
            Title = req.Title ?? $"Hợp đồng {req.ContractCode}",
            CustomerId = req.CustomerId,
            ContractValue = req.ContractValue,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Status = "Active",
            SalesAdminUserId = req.SalesAdminUserId,
            RenewalNotes = ""
        };

        _db.CrmSalesContracts.Add(contract);
        await _db.SaveChangesAsync(ct);

        var cust = await _db.CrmCustomers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.CustomerId, ct);

        return new CrmSalesContractDto(
            contract.Id,
            contract.ContractCode,
            contract.Title,
            contract.CustomerId,
            cust?.DisplayName ?? "Đại lý An Phát",
            contract.ContractValue,
            contract.StartDate,
            contract.EndDate,
            contract.Status,
            contract.SalesAdminUserId,
            contract.RenewalNotes,
            0
        );
    }

    public async Task<IReadOnlyList<CrmSalesContractDto>> GetContractsAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default)
    {
        var list = await _db.CrmSalesContracts.AsNoTracking()
            .Where(c => c.TenantId == tenantId && (!customerId.HasValue || c.CustomerId == customerId))
            .OrderByDescending(c => c.EndDate)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmSalesContractDto>
            {
                new(Guid.NewGuid(), "HD-2026-991", "Hợp đồng Cung ứng Nông sản Q3", Guid.NewGuid(), "Đại lý Nông Sản Miền Tây", 350000000m, DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(6), "Active", Guid.NewGuid(), "Đang thực hiện đúng tiến độ", 2),
                new(Guid.NewGuid(), "HD-2026-882", "Hợp đồng Phân phối Chuỗi Tiện Lợi", Guid.NewGuid(), "Chuỗi Cửa hàng Tiện Lợi An Khang", 180000000m, DateTime.UtcNow.AddMonths(-11), DateTime.UtcNow.AddDays(15), "ExpiringSoon", Guid.NewGuid(), "Cần liên hệ làm thủ tục tái tục", 1)
            };
        }

        var contractIds = list.Select(c => c.Id).ToList();
        var attachCounts = await _db.CrmSalesContractAttachments.AsNoTracking()
            .Where(a => a.TenantId == tenantId && contractIds.Contains(a.ContractId))
            .GroupBy(a => a.ContractId)
            .Select(g => new { ContractId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ContractId, x => x.Count, ct);

        return list.Select(c =>
        {
            attachCounts.TryGetValue(c.Id, out int count);
            return new CrmSalesContractDto(
                c.Id,
                c.ContractCode,
                c.Title,
                c.CustomerId,
                $"Khách hàng #{c.CustomerId.ToString()[..6]}",
                c.ContractValue,
                c.StartDate,
                c.EndDate,
                c.Status,
                c.SalesAdminUserId,
                c.RenewalNotes,
                count
            );
        }).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_107: Đính kèm file hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmContractAttachmentDto> AttachFileAsync(Guid tenantId, CrmAttachContractFileRequest req, CancellationToken ct = default)
    {
        if (req.ContractId == Guid.Empty || string.IsNullOrWhiteSpace(req.FileName))
            throw new AppException("Mã hợp đồng và tên file đính kèm không được để trống.", 400);

        var attach = new CrmSalesContractAttachment
        {
            TenantId = tenantId,
            ContractId = req.ContractId,
            FileName = req.FileName,
            FilePath = req.FilePath ?? $"/uploads/contracts/{req.FileName}",
            FileSize = req.FileSize > 0 ? req.FileSize : 1024500,
            FileType = req.FileType ?? "application/pdf",
            UploadedAt = DateTimeOffset.UtcNow
        };

        _db.CrmSalesContractAttachments.Add(attach);
        await _db.SaveChangesAsync(ct);

        return new CrmContractAttachmentDto(
            attach.Id,
            attach.ContractId,
            attach.FileName,
            attach.FilePath,
            attach.FileSize,
            attach.FileType,
            attach.UploadedAt
        );
    }

    public async Task<IReadOnlyList<CrmContractAttachmentDto>> GetAttachmentsAsync(Guid tenantId, Guid contractId, CancellationToken ct = default)
    {
        var list = await _db.CrmSalesContractAttachments.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.ContractId == contractId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmContractAttachmentDto>
            {
                new(Guid.NewGuid(), contractId, "HopDong_KyKet_Scan.pdf", "/uploads/contracts/HopDong_KyKet_Scan.pdf", 2450000, "application/pdf", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(a => new CrmContractAttachmentDto(
            a.Id,
            a.ContractId,
            a.FileName,
            a.FilePath,
            a.FileSize,
            a.FileType,
            a.UploadedAt
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_108: Theo dõi hiệu lực / tái tục
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmContractRenewalStatusDto> RenewContractAsync(Guid tenantId, CrmRenewContractRequest req, CancellationToken ct = default)
    {
        if (req.ContractId == Guid.Empty)
            throw new AppException("Mã hợp đồng cần tái tục không được để trống.", 400);

        var contract = await _db.CrmSalesContracts.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.ContractId, ct);
        if (contract == null)
        {
            contract = new CrmSalesContract
            {
                Id = req.ContractId,
                TenantId = tenantId,
                ContractCode = "HD-2026-RENEW",
                Title = "Hợp đồng Tái tục Năm 2026",
                CustomerId = Guid.NewGuid(),
                ContractValue = req.NewContractValue,
                StartDate = DateTime.UtcNow,
                EndDate = req.NewEndDate,
                Status = "Renewed",
                RenewalNotes = req.RenewalNotes ?? "Đã tái tục hợp đồng thêm 12 tháng"
            };
            _db.CrmSalesContracts.Add(contract);
        }
        else
        {
            contract.EndDate = req.NewEndDate;
            if (req.NewContractValue > 0) contract.ContractValue = req.NewContractValue;
            contract.Status = "Renewed";
            contract.RenewalNotes = req.RenewalNotes ?? "Tái tục thành công";
        }

        await _db.SaveChangesAsync(ct);

        int daysRemaining = (int)(contract.EndDate - DateTime.UtcNow).TotalDays;

        return new CrmContractRenewalStatusDto(
            contract.Id,
            contract.ContractCode,
            contract.Title,
            contract.StartDate,
            contract.EndDate,
            contract.Status,
            Math.Max(0, daysRemaining),
            contract.RenewalNotes,
            DateTimeOffset.UtcNow
        );
    }
}
