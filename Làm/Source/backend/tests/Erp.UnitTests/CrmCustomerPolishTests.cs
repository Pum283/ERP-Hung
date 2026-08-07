using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmCustomerPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCustomerService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public CrmCustomerPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-customer-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new CrmCustomerService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task UpsertPersonCustomer_Succeeds()
    {
        var dto = await _svc.UpsertAsync(_tenant, _user, new CrmCustomerUpsertRequest(
            null, "KH-CN01", "Person", "Nguyễn Văn A", null, "0901234567", "a@test.com", null,
            "Customer", null, "123 Lê Lợi", "Khách thân thiết", null, "Active"));

        Assert.NotNull(dto);
        Assert.Equal("KH-CN01", dto.Code);
        Assert.Equal("Person", dto.CustomerType);
        Assert.Equal("Nguyễn Văn A", dto.DisplayName);
        Assert.Equal("0901234567", dto.Phone);
    }

    [Fact]
    public async Task UpsertOrganizationCustomer_RequiresCompanyNameOrDisplayName()
    {
        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertAsync(_tenant, _user, new CrmCustomerUpsertRequest(
            null, "KH-DN01", "Organization", "", "", "0909999888", "b@test.com", "0101234567",
            "Prospect", null, "HN", null, null, "Active")));
    }

    [Fact]
    public async Task UpsertCustomer_DuplicatePhone_Throws()
    {
        await _svc.UpsertAsync(_tenant, _user, new CrmCustomerUpsertRequest(
            null, "KH-01", "Person", "Khách 1", null, "0988777666", null, null,
            "Customer", null, null, null, null, "Active"));

        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertAsync(_tenant, _user, new CrmCustomerUpsertRequest(
            null, "KH-02", "Person", "Khách 2", null, "0988777666", null, null,
            "Customer", null, null, null, null, "Active")));
    }

    [Fact]
    public async Task SearchCustomers_FiltersByTypeAndQuery()
    {
        await _svc.UpsertAsync(_tenant, _user, new CrmCustomerUpsertRequest(
            null, "KH-P1", "Person", "Trần Văn CN", null, "0911111111", null, null,
            "Customer", null, null, null, null, "Active"));
        await _svc.UpsertAsync(_tenant, _user, new CrmCustomerUpsertRequest(
            null, "KH-O1", "Organization", "Công ty ABC", "Cty ABC", "0922222222", null, "010999888",
            "Partner", null, null, null, null, "Active"));

        var personOnly = await _svc.SearchAsync(_tenant, new CrmCustomerSearchRequest(null, "Person", null, null, null, null, null, false));
        Assert.Single(personOnly);
        Assert.Equal("KH-P1", personOnly[0].Code);

        var orgOnly = await _svc.SearchAsync(_tenant, new CrmCustomerSearchRequest("ABC", "Organization", null, null, null, null, null, false));
        Assert.Single(orgOnly);
        Assert.Equal("KH-O1", orgOnly[0].Code);
    }
}
