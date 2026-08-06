using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pjm;
using Erp.Application.Interfaces.Services.Pjm;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pjm;

public sealed class PjmCostCloseService : IPjmCostCloseService
{
    private readonly AppDbContext _db;
    private readonly IPjmProjectService _projects;
    public PjmCostCloseService(AppDbContext db, IPjmProjectService projects)
    {
        _db = db;
        _projects = projects;
    }

    public async Task<PjmExpenseDto> UpsertExpenseAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmExpenseUpsertRequest req, CancellationToken ct = default)
    {
        var project = await RequireOpenProject(tenantId, projectId, ct);
        if (req.Amount <= 0) throw new AppException("Số tiền chi phí phải > 0.");
        var category = string.IsNullOrWhiteSpace(req.Category) ? "Other" : req.Category.Trim();
        var desc = (req.Description ?? "").Trim();
        if (desc.Length is < 1 or > 300) throw new AppException("Mô tả chi phí 1–300 ký tự.");

        PjmExpense entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PjmExpenses.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy chi phí.", 404);
            if (entity.Status == "Posted") throw new AppException("Chi phí đã Posted — không sửa.");
        }
        else
        {
            entity = new PjmExpense
            {
                TenantId = tenantId,
                ProjectId = project.Id,
                Code = await NextCodeAsync(tenantId, "EX", ct),
                CreatedBy = userId,
            };
            _db.PjmExpenses.Add(entity);
        }

        entity.Category = category;
        entity.Description = desc;
        entity.Amount = decimal.Round(req.Amount, 2);
        entity.ExpenseDate = req.ExpenseDate ?? DateTimeOffset.UtcNow;
        entity.WbsItemId = req.WbsItemId;
        entity.Note = Null(req.Note, 1000);
        entity.UpdatedBy = userId;
        if (req.Post)
        {
            entity.Status = "Posted";
            entity.PostedAt = DateTimeOffset.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return new PjmExpenseDto(
            entity.Id, entity.ProjectId, entity.Code, entity.Category, entity.Description,
            entity.Amount, entity.ExpenseDate, entity.WbsItemId, entity.Status, entity.PostedAt, entity.Note);
    }

    public async Task<PjmMaterialIssueDto> CreateMaterialIssueAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmMaterialIssueCreateRequest req, CancellationToken ct = default)
    {
        var project = await RequireOpenProject(tenantId, projectId, ct);
        if (req.Lines is null || req.Lines.Count == 0)
            throw new AppException("Cần ít nhất một dòng NVL.");

        var doc = new PjmMaterialIssue
        {
            TenantId = tenantId,
            ProjectId = project.Id,
            Code = await NextCodeAsync(tenantId, "MI", ct),
            Status = "Draft",
            Note = Null(req.Note, 1000),
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.PjmMaterialIssues.Add(doc);

        foreach (var line in req.Lines)
        {
            if (line.Qty <= 0) throw new AppException("SL NVL phải > 0.");
            if (line.UnitCost < 0) throw new AppException("Đơn giá không được âm.");
            var code = (line.ProductCode ?? "").Trim().ToUpperInvariant();
            var name = (line.ProductName ?? "").Trim();
            if (code.Length is < 1 or > 40 || name.Length is < 1 or > 200)
                throw new AppException("Mã/tên NVL không hợp lệ.");
            _db.PjmMaterialIssueLines.Add(new PjmMaterialIssueLine
            {
                TenantId = tenantId,
                MaterialIssueId = doc.Id,
                ProductCode = code,
                ProductName = name,
                Unit = string.IsNullOrWhiteSpace(line.Unit) ? "CAI" : line.Unit.Trim(),
                Qty = line.Qty,
                UnitCost = decimal.Round(line.UnitCost, 2),
                CreatedBy = userId,
                UpdatedBy = userId,
            });
        }

        if (req.Post)
        {
            doc.Status = "Posted";
            doc.PostedAt = DateTimeOffset.UtcNow;
        }
        await _db.SaveChangesAsync(ct);

        var loaded = await _db.PjmMaterialIssues.AsNoTracking().Include(x => x.Lines)
            .FirstAsync(x => x.Id == doc.Id, ct);
        var lines = loaded.Lines.Where(l => !l.IsDeleted).Select(l => new PjmMaterialIssueLineDto(
            l.Id, l.ProductCode, l.ProductName, l.Unit, l.Qty, l.UnitCost,
            decimal.Round(l.Qty * l.UnitCost, 2))).ToList();
        return new PjmMaterialIssueDto(
            loaded.Id, loaded.ProjectId, loaded.Code, loaded.Status, loaded.Note, loaded.PostedAt,
            decimal.Round(lines.Sum(l => l.Amount), 2), lines);
    }

    public async Task<PjmAcceptanceDto> CreateAcceptanceAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmAcceptanceCreateRequest req, CancellationToken ct = default)
    {
        var project = await RequireOpenProject(tenantId, projectId, ct);
        var kind = string.IsNullOrWhiteSpace(req.Kind) ? "Phase" : req.Kind.Trim();
        if (kind is not ("Phase" or "Final")) throw new AppException("Kind: Phase | Final.");
        var title = (req.Title ?? "").Trim();
        if (title.Length is < 1 or > 200) throw new AppException("Tiêu đề 1–200 ký tự.");

        if (kind == "Final")
        {
            var exists = await _db.PjmAcceptances.AnyAsync(
                x => x.TenantId == tenantId && x.ProjectId == projectId && x.Kind == "Final" && !x.IsDeleted, ct);
            if (exists) throw new AppException("Đã có biên bản nghiệm thu cuối.");
        }

        var entity = new PjmAcceptance
        {
            TenantId = tenantId,
            ProjectId = project.Id,
            Code = await NextCodeAsync(tenantId, "AC", ct),
            Kind = kind,
            Title = title,
            Status = "Draft",
            Note = Null(req.Note, 1000),
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.PjmAcceptances.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new PjmAcceptanceDto(
            entity.Id, entity.ProjectId, entity.Code, entity.Kind, entity.Title,
            entity.Status, entity.SignerName, entity.SignedAt, entity.Note);
    }

    public async Task<PjmAcceptanceDto> SignAcceptanceAsync(
        Guid tenantId, Guid userId, Guid projectId, Guid acceptanceId, PjmAcceptanceSignRequest req, CancellationToken ct = default)
    {
        _ = await RequireOpenProject(tenantId, projectId, ct);
        var signer = (req.SignerName ?? "").Trim();
        if (signer.Length is < 1 or > 120) throw new AppException("Tên người ký 1–120 ký tự.");

        var entity = await _db.PjmAcceptances.FirstOrDefaultAsync(
            x => x.Id == acceptanceId && x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy biên bản NT.", 404);
        if (entity.Status == "Signed") throw new AppException("Biên bản đã ký.");

        entity.Status = "Signed";
        entity.SignerName = signer;
        entity.SignedAt = DateTimeOffset.UtcNow;
        entity.Note = Null(req.Note, 1000) ?? entity.Note;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PjmAcceptanceDto(
            entity.Id, entity.ProjectId, entity.Code, entity.Kind, entity.Title,
            entity.Status, entity.SignerName, entity.SignedAt, entity.Note);
    }

    public async Task<PjmProjectDto> RecognizeRevenueAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmRecognizeRevenueRequest req, CancellationToken ct = default)
    {
        var project = await RequireOpenProject(tenantId, projectId, ct);
        if (req.Amount < 0) throw new AppException("Doanh thu không được âm.");
        var hasFinal = await _db.PjmAcceptances.AnyAsync(
            x => x.TenantId == tenantId && x.ProjectId == projectId
                 && x.Kind == "Final" && x.Status == "Signed" && !x.IsDeleted, ct);
        if (!hasFinal) throw new AppException("Cần nghiệm thu cuối đã ký trước khi ghi doanh thu.");

        project.RecognizedRevenue = decimal.Round(req.Amount, 2);
        project.RevenueRecognizedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.Note))
            project.Note = string.IsNullOrWhiteSpace(project.Note)
                ? $"DT: {req.Note.Trim()}"
                : $"{project.Note}\nDT: {req.Note.Trim()}";
        project.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await _projects.ListProjectsAsync(tenantId, project.Code, ct))
            .First(x => x.Id == project.Id);
    }

    public async Task<PjmProjectDto> CloseProjectAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmCloseProjectRequest req, CancellationToken ct = default)
    {
        var project = await RequireOpenProject(tenantId, projectId, ct);
        var hasFinal = await _db.PjmAcceptances.AnyAsync(
            x => x.TenantId == tenantId && x.ProjectId == projectId
                 && x.Kind == "Final" && x.Status == "Signed" && !x.IsDeleted, ct);
        if (!hasFinal) throw new AppException("Cần nghiệm thu cuối đã ký trước khi đóng dự án.");

        project.StatusCode = "Completed";
        project.ClosedAt = DateTimeOffset.UtcNow;
        project.ClosedByUserId = userId;
        if (!string.IsNullOrWhiteSpace(req.Note))
            project.Note = string.IsNullOrWhiteSpace(project.Note)
                ? req.Note.Trim()
                : $"{project.Note}\nĐóng: {req.Note.Trim()}";
        project.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await _projects.ListProjectsAsync(tenantId, project.Code, ct))
            .First(x => x.Id == project.Id);
    }

    private async Task<PjmProject> RequireOpenProject(Guid tenantId, Guid projectId, CancellationToken ct)
    {
        var p = await _db.PjmProjects.FirstOrDefaultAsync(
            x => x.Id == projectId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy dự án.", 404);
        if (p.StatusCode is "Closed" or "Completed" or "Cancelled")
            throw new AppException("Dự án đã đóng/hủy — không thao tác.");
        return p;
    }

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyMMdd");
        var stem = $"{prefix}-{today}-";
        string? last = prefix switch
        {
            "EX" => await _db.PjmExpenses.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            "MI" => await _db.PjmMaterialIssues.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            _ => await _db.PjmAcceptances.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
        };
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private static string? Null(string? s, int max)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim();
        return t.Length <= max ? t : t[..max];
    }
}
