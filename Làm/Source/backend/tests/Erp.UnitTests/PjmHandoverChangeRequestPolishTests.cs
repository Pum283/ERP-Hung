using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PjmHandoverChangeRequestPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PjmHandoverChangeRequestService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public PjmHandoverChangeRequestPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pjm-handover-ecr-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new PjmHandoverChangeRequestService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateHandoverChecklist_SavesRepresentativeName()
    {
        var req = new PjmCreateHandoverChecklistRequest(Guid.NewGuid(), "PRJ-088", "Bàn giao chìa khóa phòng điện", true, "Đại diện Chủ đầu tư");
        var res = await _svc.CreateHandoverChecklistAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsSatisfied);
        Assert.Equal("Đại diện Chủ đầu tư", res.CustomerRepresentativeName);
    }

    [Fact]
    public async Task UploadProtocolAttachment_SavesAttachmentFileUrl()
    {
        var req = new PjmUploadProtocolAttachmentRequest(Guid.NewGuid(), "PRJ-088", "Biên bản nghiệm thu", "ProtocolPdf", "/uploads/prj-088.pdf", 500000);
        var res = await _svc.UploadProtocolAttachmentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("/uploads/prj-088.pdf", res.FileUrl);
        Assert.Equal("ProtocolPdf", res.AttachmentType);
    }

    [Fact]
    public async Task CreateEcr_GeneratesEcrNumber()
    {
        var req = new PjmCreateEcrRequest(Guid.NewGuid(), "PRJ-088", "Thay đổi vị trí tủ điện", "Vướng cột bê tông", 20000000m, 3);
        var res = await _svc.CreateEcrAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("ECR-", res.EcrNumber);
        Assert.Equal("Submitted", res.Status);
    }

    [Fact]
    public async Task ApproveEcr_UpdatesApprovalStatus()
    {
        var createReq = new PjmCreateEcrRequest(Guid.NewGuid(), "PRJ-088", "Thêm cáp dự phòng", "Khách hàng đề nghị", 15000000m, 2);
        var created = await _svc.CreateEcrAsync(_tenant, createReq);

        var approveReq = new PjmApproveEcrRequest(created.Id, true, 15000000m, 2, "GĐ Ban Dự Án", "Duyệt bổ sung");
        var res = await _svc.ApproveEcrAsync(_tenant, approveReq);

        Assert.NotNull(res);
        Assert.True(res.IsApproved);
        Assert.Equal("GĐ Ban Dự Án", res.ApproverName);
    }
}
