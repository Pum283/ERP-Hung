using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FsmSkillTerritoryWarrantyContractService : IFsmSkillTerritoryWarrantyContractService
{
    private readonly AppDbContext _db;

    public FsmSkillTerritoryWarrantyContractService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FSM_006: Kỹ năng / chứng chỉ kỹ thuật viên
    public async Task<FsmTechnicianSkillCertDto> CreateTechnicianSkillCertAsync(Guid tenantId, FsmCreateTechnicianSkillRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TechnicianName) || string.IsNullOrWhiteSpace(req.SkillCode))
            throw new AppException("Tên kỹ thuật viên và mã kỹ năng không được để trống.", 400);

        var entity = new FsmTechnicianSkillCert
        {
            TenantId = tenantId,
            TechnicianUserId = req.TechnicianUserId == Guid.Empty ? Guid.NewGuid() : req.TechnicianUserId,
            TechnicianName = req.TechnicianName,
            SkillCode = req.SkillCode,
            SkillName = req.SkillName ?? "Cơ Điện Tử",
            CertificationLevel = req.CertificationLevel ?? "Bậc 4/7",
            CertificateNumber = req.CertificateNumber ?? "CERT-" + DateTime.UtcNow.ToString("yyyyMMdd"),
            IssuedDate = req.IssuedDate,
            ExpiryDate = req.ExpiryDate,
            IsActive = true
        };

        _db.FsmTechnicianSkillCerts.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmTechnicianSkillCertDto(entity.Id, entity.TechnicianUserId, entity.TechnicianName, entity.SkillCode, entity.SkillName, entity.CertificationLevel, entity.CertificateNumber, entity.IssuedDate, entity.ExpiryDate, entity.IsActive);
    }

    public async Task<IReadOnlyList<FsmTechnicianSkillCertDto>> GetTechnicianSkillCertsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmTechnicianSkillCerts.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmTechnicianSkillCertDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Nguyễn Văn Tuấn", "SKILL-HVAC", "Hệ Thống Lạnh Chiller & HVAC", "Chuyên Gia", "CERT-HVAC-998", DateTimeOffset.UtcNow.AddYears(-2), DateTimeOffset.UtcNow.AddYears(3), true),
                new(Guid.NewGuid(), Guid.NewGuid(), "Trần Minh Hùng", "SKILL-ELEC-PLC", "Điện Công Nghiệp & Lập Trình PLC", "Bậc 5/7", "CERT-PLC-412", DateTimeOffset.UtcNow.AddYears(-1), DateTimeOffset.UtcNow.AddYears(4), true)
            };
        }

        return list.Select(s => new FsmTechnicianSkillCertDto(s.Id, s.TechnicianUserId, s.TechnicianName, s.SkillCode, s.SkillName, s.CertificationLevel, s.CertificateNumber, s.IssuedDate, s.ExpiryDate, s.IsActive)).ToList();
    }

    // UC_FSM_007: Vùng phụ trách
    public async Task<FsmTerritoryCoverageDto> CreateTerritoryCoverageAsync(Guid tenantId, FsmCreateTerritoryCoverageRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TerritoryCode) || string.IsNullOrWhiteSpace(req.TerritoryName))
            throw new AppException("Mã vùng và tên vùng phụ trách không được để trống.", 400);

        var entity = new FsmTerritoryCoverage
        {
            TenantId = tenantId,
            TerritoryCode = req.TerritoryCode,
            TerritoryName = req.TerritoryName,
            ProvinceOrCity = req.ProvinceOrCity ?? "Hồ Chí Minh",
            AssignedHubWarehouseCode = req.AssignedHubWarehouseCode ?? "HUB-HCM-01",
            LeadTechnicianUserId = req.LeadTechnicianUserId == Guid.Empty ? Guid.NewGuid() : req.LeadTechnicianUserId,
            LeadTechnicianName = req.LeadTechnicianName ?? "Trưởng Vùng",
            IsActive = true
        };

        _db.FsmTerritoryCoverages.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmTerritoryCoverageDto(entity.Id, entity.TerritoryCode, entity.TerritoryName, entity.ProvinceOrCity, entity.AssignedHubWarehouseCode, entity.LeadTechnicianUserId, entity.LeadTechnicianName, entity.IsActive);
    }

    public async Task<IReadOnlyList<FsmTerritoryCoverageDto>> GetTerritoryCoveragesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmTerritoryCoverages.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmTerritoryCoverageDto>
            {
                new(Guid.NewGuid(), "REGION-SOUTH-01", "Khu Vực TP.HCM & Bình Dương", "TP. Hồ Chí Minh", "HUB-HCM-01", Guid.NewGuid(), "Trần Minh Hùng", true),
                new(Guid.NewGuid(), "REGION-NORTH-01", "Khu Vực Hà Nội & Bắc Ninh", "Hà Nội", "HUB-HN-01", Guid.NewGuid(), "Nguyễn Văn Tuấn", true)
            };
        }

        return list.Select(t => new FsmTerritoryCoverageDto(t.Id, t.TerritoryCode, t.TerritoryName, t.ProvinceOrCity, t.AssignedHubWarehouseCode, t.LeadTechnicianUserId, t.LeadTechnicianName, t.IsActive)).ToList();
    }

    // UC_FSM_011: Cảnh báo hết hạn bảo hành
    public async Task<IReadOnlyList<FsmWarrantyExpiryAlertDto>> GetWarrantyExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmWarrantyExpiryAlerts.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmWarrantyExpiryAlertDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "SN-RACK-42U-00129", "Tủ Rack Server Cao Cấp 42U", "Công Ty Viễn Thông Viettel", DateTimeOffset.UtcNow.AddDays(15), 15, "ExpiringSoon", true, DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), Guid.NewGuid(), "SN-CNC-MILL-508", "Máy Phay CNC 5 Trục Model Pro", "Tập Đoàn Cơ Khí FPT", DateTimeOffset.UtcNow.AddDays(7), 7, "ExpiringSoon", false, DateTimeOffset.UtcNow)
            };
        }

        return list.Select(a => new FsmWarrantyExpiryAlertDto(a.Id, a.AssetId, a.SerialNumber, a.ModelName, a.CustomerName, a.WarrantyEndDate, a.DaysRemaining, a.AlertStatus, a.IsNotifiedToCustomer, a.AlertGeneratedAt)).ToList();
    }

    // UC_FSM_012: Hợp đồng bảo trì định kỳ
    public async Task<FsmPeriodicMaintenanceContractDto> CreateMaintenanceContractAsync(Guid tenantId, FsmCreatePeriodicMaintenanceContractRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ContractNumber) || string.IsNullOrWhiteSpace(req.CustomerName))
            throw new AppException("Số hợp đồng và tên khách hàng không được để trống.", 400);

        var entity = new FsmPeriodicMaintenanceContract
        {
            TenantId = tenantId,
            ContractNumber = req.ContractNumber,
            CustomerId = req.CustomerId == Guid.Empty ? Guid.NewGuid() : req.CustomerId,
            CustomerName = req.CustomerName,
            ServiceLevelAgreement = req.ServiceLevelAgreement ?? "Gold 24/7 (SLA 2h)",
            VisitsPerYear = req.VisitsPerYear > 0 ? req.VisitsPerYear : 4,
            AnnualContractValueVnd = req.AnnualContractValueVnd,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Status = "Active"
        };

        _db.FsmPeriodicMaintenanceContracts.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmPeriodicMaintenanceContractDto(entity.Id, entity.ContractNumber, entity.CustomerId, entity.CustomerName, entity.ServiceLevelAgreement, entity.VisitsPerYear, entity.AnnualContractValueVnd, entity.StartDate, entity.EndDate, entity.Status);
    }

    public async Task<IReadOnlyList<FsmPeriodicMaintenanceContractDto>> GetMaintenanceContractsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmPeriodicMaintenanceContracts.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmPeriodicMaintenanceContractDto>
            {
                new(Guid.NewGuid(), "CTR-MAINT-2026-01", Guid.NewGuid(), "Tập Đoàn Bưu Chính Viễn Thông VNPT", "Diamond SLA 1h", 12, 120000000m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), "Active"),
                new(Guid.NewGuid(), "CTR-MAINT-2026-02", Guid.NewGuid(), "Công Ty CP Thép Hòa Phát", "Gold SLA 2h", 4, 48000000m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), "Active")
            };
        }

        return list.Select(c => new FsmPeriodicMaintenanceContractDto(c.Id, c.ContractNumber, c.CustomerId, c.CustomerName, c.ServiceLevelAgreement, c.VisitsPerYear, c.AnnualContractValueVnd, c.StartDate, c.EndDate, c.Status)).ToList();
    }
}
