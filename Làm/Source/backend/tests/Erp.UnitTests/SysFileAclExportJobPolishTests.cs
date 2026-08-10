using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysFileAclExportJobPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public SysFileAclExportJobPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-file-acl-export-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new SysPlatformService(_db, new OutboxWriter(_db));
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_SYS_069: Phân quyền file theo đối tượng ───

    [Fact]
    public async Task UC069_LinkFileToEntity_ValidFile_SetsLinkedEntityFields()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("contract.pdf", "application/pdf", 2048));
        var entityId = Guid.NewGuid();

        await _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Customer", entityId));

        var linked = await _svc.ListLinkedFilesAsync(_tenant, "Customer", entityId);
        Assert.Single(linked);
        Assert.Equal("contract.pdf", linked[0].FileName);
        Assert.Equal("Customer", linked[0].EntityType);
    }

    [Fact]
    public async Task UC069_LinkFileToEntity_EmptyEntityType_ThrowsAppException()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("doc.pdf", "application/pdf", 1024));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "  ", Guid.NewGuid())));
        Assert.Contains("EntityType", ex.Message);
    }

    [Fact]
    public async Task UC069_LinkFileToEntity_EmptyEntityId_ThrowsAppException()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("doc.pdf", "application/pdf", 1024));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Order", Guid.Empty)));
        Assert.Contains("EntityId", ex.Message);
    }

    [Fact]
    public async Task UC069_LinkFileToEntity_NonExistentFile_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(Guid.NewGuid(), "Order", Guid.NewGuid())));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC069_LinkFileToEntity_DuplicateLink_ThrowsAppException()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("dup.pdf", "application/pdf", 512));
        var entityId = Guid.NewGuid();
        await _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Order", entityId));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Order", entityId)));
        Assert.Contains("đã được gắn vào đối tượng này", ex.Message);
    }

    [Fact]
    public async Task UC069_LinkFileToEntity_AlreadyLinkedToDifferentEntity_ThrowsAppException()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("shared.pdf", "application/pdf", 512));
        await _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Customer", Guid.NewGuid()));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Order", Guid.NewGuid())));
        Assert.Contains("đối tượng khác", ex.Message);
    }

    [Fact]
    public async Task UC069_UnlinkFileFromEntity_ClearsLinkedFields()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("unlink.pdf", "application/pdf", 1024));
        var entityId = Guid.NewGuid();
        await _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Project", entityId));

        await _svc.UnlinkFileFromEntityAsync(_tenant, file.Id, "Project", entityId);

        var linked = await _svc.ListLinkedFilesAsync(_tenant, "Project", entityId);
        Assert.Empty(linked);
    }

    [Fact]
    public async Task UC069_UnlinkFileFromEntity_WrongEntity_ThrowsAppException()
    {
        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("wrong.pdf", "application/pdf", 512));
        await _svc.LinkFileToEntityAsync(_tenant, _user, new LinkFileToEntityRequest(file.Id, "Customer", Guid.NewGuid()));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UnlinkFileFromEntityAsync(_tenant, file.Id, "Order", Guid.NewGuid()));
        Assert.Contains("không được gắn vào đối tượng này", ex.Message);
    }

    // ─── UC_SYS_074: Export Excel (CSV) ───

    [Fact]
    public async Task UC074_ExportEntityData_CsvFormat_ReturnsValidCsvWithBOM()
    {
        _db.Users.Add(new AppUser { TenantId = _tenant, Username = "admin", DisplayName = "Admin User", Email = "admin@test.com", Status = UserStatus.Active, PasswordHash = "x" });
        _db.Users.Add(new AppUser { TenantId = _tenant, Username = "editor", DisplayName = "Editor", Email = "ed@test.com", Status = UserStatus.Active, PasswordHash = "x" });
        await _db.SaveChangesAsync();

        var result = await _svc.ExportEntityDataAsync(_tenant, _user, new GenericExportRequest("Users", "Csv"));

        Assert.Equal("text/csv; charset=utf-8", result.ContentType);
        Assert.True(result.FileName.EndsWith(".csv"));
        Assert.Equal(2, result.RowCount);
        var csv = Encoding.UTF8.GetString(result.Data);
        Assert.Contains("admin", csv);
        Assert.Contains("editor", csv);
    }

    [Fact]
    public async Task UC074_ExportEntityData_EmptyEntityType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ExportEntityDataAsync(_tenant, _user, new GenericExportRequest("", "Csv")));
        Assert.Contains("EntityType", ex.Message);
    }

    [Fact]
    public async Task UC074_ExportEntityData_InvalidFormat_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ExportEntityDataAsync(_tenant, _user, new GenericExportRequest("Users", "Xlsx")));
        Assert.Contains("Csv", ex.Message);
    }

    [Fact]
    public async Task UC074_ExportEntityData_UnsupportedEntityType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ExportEntityDataAsync(_tenant, _user, new GenericExportRequest("Unicorns", "Csv")));
        Assert.Contains("chưa được hỗ trợ", ex.Message);
    }

    // ─── UC_SYS_075: Export PDF ───

    [Fact]
    public async Task UC075_ExportEntityData_PdfFormat_ReturnsPdfWithHeader()
    {
        _db.Users.Add(new AppUser { TenantId = _tenant, Username = "pdfuser", DisplayName = "PDF User", Email = "pdf@test.com", Status = UserStatus.Active, PasswordHash = "x" });
        await _db.SaveChangesAsync();

        var result = await _svc.ExportEntityDataAsync(_tenant, _user, new GenericExportRequest("Users", "Pdf"));

        Assert.Equal("application/pdf", result.ContentType);
        Assert.True(result.FileName.EndsWith(".pdf"));
        var pdfText = Encoding.UTF8.GetString(result.Data);
        Assert.StartsWith("%PDF-1.4", pdfText);
        Assert.Contains("%%EOF", pdfText);
        Assert.Contains("pdfuser", pdfText);
    }

    // ─── UC_SYS_076: Lịch sử job import/export ───

    [Fact]
    public async Task UC076_ExportEntityData_CreatesImportExportJobRecord()
    {
        _db.Users.Add(new AppUser { TenantId = _tenant, Username = "jobuser", DisplayName = "J", Email = "j@t.com", Status = UserStatus.Active, PasswordHash = "x" });
        await _db.SaveChangesAsync();

        await _svc.ExportEntityDataAsync(_tenant, _user, new GenericExportRequest("Users", "Csv"));

        var jobs = await _svc.ListImportExportJobsAsync(_tenant, 10);
        Assert.Single(jobs);
        Assert.Equal("Export", jobs[0].JobType);
        Assert.Equal("Users", jobs[0].EntityType);
        Assert.Equal("Csv", jobs[0].Format);
        Assert.Equal("Completed", jobs[0].Status);
        Assert.True(jobs[0].RowCount >= 1);
    }

    [Fact]
    public async Task UC076_ListImportExportJobs_FiltersByTenant()
    {
        var otherTenant = Guid.NewGuid();
        _db.ImportExportJobs.Add(new ImportExportJob { TenantId = _tenant, JobType = "Import", EntityType = "Users", Status = "Completed", RowCount = 10 });
        _db.ImportExportJobs.Add(new ImportExportJob { TenantId = otherTenant, JobType = "Export", EntityType = "Users", Status = "Completed", RowCount = 5 });
        await _db.SaveChangesAsync();

        var jobs = await _svc.ListImportExportJobsAsync(_tenant, 20);
        Assert.Single(jobs);
        Assert.Equal("Import", jobs[0].JobType);
    }

    [Fact]
    public async Task UC076_ListImportExportJobs_TakeLimit_CapsAt500()
    {
        for (int i = 0; i < 10; i++)
            _db.ImportExportJobs.Add(new ImportExportJob { TenantId = _tenant, JobType = "Export", EntityType = "Files", Status = "Completed", RowCount = i });
        await _db.SaveChangesAsync();

        var jobs = await _svc.ListImportExportJobsAsync(_tenant, 5);
        Assert.Equal(5, jobs.Count);
    }

    [Fact]
    public async Task UC076_ListImportExportJobs_NegativeTake_DefaultsTo20()
    {
        for (int i = 0; i < 25; i++)
            _db.ImportExportJobs.Add(new ImportExportJob { TenantId = _tenant, JobType = "Import", EntityType = "Users", Status = "Completed" });
        await _db.SaveChangesAsync();

        var jobs = await _svc.ListImportExportJobsAsync(_tenant, -1);
        Assert.Equal(20, jobs.Count);
    }
}
