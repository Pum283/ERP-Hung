using Erp.Application.DTOs.Lms;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Implementations.Services.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LmsEnrollmentPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsClassService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public LmsEnrollmentPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("lms-enrollment-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new LmsClassService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task EnrollAsync_CreatesEnrollmentAndPaymentCallLog()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP001", FullName = "Nguyễn Văn Học" };
        var cls = new LmsTrainingClass { TenantId = _tenant, Code = "LOP-01", Name = "Lớp React", CourseTitle = "Khóa Frontend", StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)), Status = "Open" };
        _db.Employees.Add(emp);
        _db.LmsTrainingClasses.Add(cls);
        await _db.SaveChangesAsync();

        var enroll = await _svc.EnrollAsync(_tenant, _user, cls.Id, new LmsClassEnrollmentRequest(emp.Id));

        Assert.NotNull(enroll);
        Assert.Equal("Enrolled", enroll.Status);
        Assert.Equal("EMP001", enroll.EmployeeCode);
        Assert.Equal("Nguyễn Văn Học", enroll.EmployeeName);

        // Verify IntegrationCallLog created for payment gateway
        var callLog = await _db.IntegrationCallLogs.FirstOrDefaultAsync(x => x.TenantId == _tenant && x.Kind == "PAYMENT_GATEWAY");
        Assert.NotNull(callLog);
        Assert.Equal("/api/gateway/lms-pay", callLog.Target);
        Assert.Equal(200, callLog.StatusCode);
    }
}
