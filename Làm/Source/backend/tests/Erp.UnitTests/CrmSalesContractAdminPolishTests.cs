using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmSalesContractAdminPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesContractAdminService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _contractId = Guid.NewGuid();

    public CrmSalesContractAdminPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-sales-contract-admin-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM174", Name = "Tenant CRM 174" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-174",
            DisplayName = "Công ty TNHH Bách Hóa Việt",
            Phone = "0912333444"
        });

        _db.CrmSalesContracts.Add(new CrmSalesContract
        {
            Id = _contractId,
            TenantId = _tenant,
            ContractCode = "HD-2026-001",
            Title = "Hợp đồng Cung ứng Năm 2026",
            CustomerId = _customerId,
            ContractValue = 500000000m,
            StartDate = DateTime.UtcNow.AddMonths(-6),
            EndDate = DateTime.UtcNow.AddDays(20),
            Status = "ExpiringSoon"
        });

        _db.SaveChanges();

        _svc = new CrmSalesContractAdminService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_105: Báo cáo năng suất Sales Admin
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductivityReports_ReturnsAdminMetrics()
    {
        var reports = await _svc.GetProductivityReportsAsync(_tenant);

        Assert.NotEmpty(reports);
        Assert.Contains(reports, r => r.OrdersProcessedCount > 0);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_106: Quản lý hợp đồng bán
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateContract_CreatesSalesContract()
    {
        var req = new CrmCreateSalesContractRequest(
            "HD-2026-002",
            "Hợp đồng Đại lý Nguyên liệu",
            _customerId,
            250000000m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            Guid.NewGuid()
        );

        var res = await _svc.CreateContractAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("HD-2026-002", res.ContractCode);
        Assert.Equal(250000000m, res.ContractValue);

        var contracts = await _svc.GetContractsAsync(_tenant, _customerId);
        Assert.NotEmpty(contracts);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_107: Đính kèm file hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AttachFile_AttachesFileToContract()
    {
        var req = new CrmAttachContractFileRequest(
            _contractId,
            "HopDong_DaKy_Scan.pdf",
            "/uploads/contracts/HopDong_DaKy_Scan.pdf",
            3450000,
            "application/pdf"
        );

        var res = await _svc.AttachFileAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_contractId, res.ContractId);
        Assert.Equal("HopDong_DaKy_Scan.pdf", res.FileName);

        var list = await _svc.GetAttachmentsAsync(_tenant, _contractId);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_108: Theo dõi hiệu lực / tái tục
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RenewContract_RenewsContractValidity()
    {
        var req = new CrmRenewContractRequest(
            _contractId,
            DateTime.UtcNow.AddYears(1),
            600000000m,
            "Tái tục thành công 12 tháng năm 2027"
        );

        var res = await _svc.RenewContractAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_contractId, res.ContractId);
        Assert.Equal("Renewed", res.Status);
        Assert.True(res.DaysRemaining > 300);

        var dbContract = await _db.CrmSalesContracts.FirstOrDefaultAsync(c => c.TenantId == _tenant && c.Id == _contractId);
        Assert.NotNull(dbContract);
        Assert.Equal("Renewed", dbContract.Status);
    }
}
