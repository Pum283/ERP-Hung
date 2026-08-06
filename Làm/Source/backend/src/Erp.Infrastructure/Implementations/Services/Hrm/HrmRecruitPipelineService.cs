using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmRecruitPipelineService : IHrmRecruitPipelineService
{
    private static readonly HashSet<string> Pipeline = new(StringComparer.OrdinalIgnoreCase)
        { "New", "Screening", "Evaluating", "Accepted", "Rejected" };

    private readonly AppDbContext _db;

    public HrmRecruitPipelineService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<JobPostingDto>> ListPostingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await (
            from p in _db.JobPostings.AsNoTracking()
            join r in _db.RecruitmentRequests.AsNoTracking() on p.RecruitmentRequestId equals r.Id
            join jt in _db.JobTitles.AsNoTracking() on r.JobTitleId equals jt.Id
            where p.TenantId == tenantId && !p.IsDeleted
            orderby p.CreatedAt descending
            select new JobPostingDto(
                p.Id, p.RecruitmentRequestId, r.DocNo, p.Title, p.Channel, p.Status,
                jt.Name, r.Headcount, p.CreatedAt)
        ).ToListAsync(ct);
    }

    public async Task<JobPostingDto> CreatePostingAsync(
        Guid tenantId, Guid userId, JobPostingCreateRequest req, CancellationToken ct = default)
    {
        var rr = await _db.RecruitmentRequests.FirstOrDefaultAsync(
            x => x.Id == req.RecruitmentRequestId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu nhu cầu không tồn tại.", 404);
        if (rr.Status != "Approved")
            throw new AppException("Chỉ tạo tin từ phiếu đã duyệt.");

        var title = (req.Title ?? "").Trim();
        if (title.Length == 0) throw new AppException("Tiêu đề tin trống.");
        var channel = string.IsNullOrWhiteSpace(req.Channel) ? "Internal" : req.Channel.Trim();
        if (channel.Length > 40) throw new AppException("Kênh tối đa 40 ký tự.");

        var entity = new JobPosting
        {
            TenantId = tenantId,
            RecruitmentRequestId = rr.Id,
            Title = title,
            Channel = channel,
            Status = "Open",
            CreatedByUserId = userId,
            CreatedBy = userId
        };
        _db.JobPostings.Add(entity);
        await _db.SaveChangesAsync(ct);

        var jtName = await _db.JobTitles.Where(x => x.Id == rr.JobTitleId).Select(x => x.Name).FirstAsync(ct);
        return new JobPostingDto(entity.Id, entity.RecruitmentRequestId, rr.DocNo, entity.Title, entity.Channel,
            entity.Status, jtName, rr.Headcount, entity.CreatedAt);
    }

    public async Task ClosePostingAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var p = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Tin tuyển không tồn tại.", 404);
        p.Status = "Closed";
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CandidateDto>> ListCandidatesAsync(
        Guid tenantId, Guid? jobPostingId, CancellationToken ct = default)
    {
        var q = from c in _db.Candidates.AsNoTracking()
                join p in _db.JobPostings.AsNoTracking() on c.JobPostingId equals p.Id
                where c.TenantId == tenantId && !c.IsDeleted
                select new { c, p.Title };
        if (jobPostingId is Guid pid) q = q.Where(x => x.c.JobPostingId == pid);

        var rows = await q.OrderByDescending(x => x.c.CreatedAt).ToListAsync(ct);
        var ouIds = rows.Where(x => x.c.EvalOrgUnitId is not null).Select(x => x.c.EvalOrgUnitId!.Value).Distinct().ToList();
        var orgs = ouIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows.Select(x => MapCandidate(x.c, x.Title, orgs)).ToList();
    }

    public async Task<CandidateDto> CreateCandidateAsync(
        Guid tenantId, Guid userId, CandidateCreateRequest req, CancellationToken ct = default)
    {
        var post = await _db.JobPostings.FirstOrDefaultAsync(
            x => x.Id == req.JobPostingId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Tin tuyển không tồn tại.", 404);
        if (post.Status != "Open") throw new AppException("Tin đã đóng, không nhận UV.");

        var name = (req.FullName ?? "").Trim();
        if (name.Length == 0) throw new AppException("Họ tên ứng viên trống.");

        var entity = new Candidate
        {
            TenantId = tenantId,
            JobPostingId = post.Id,
            FullName = name,
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim(),
            CvStorageKey = string.IsNullOrWhiteSpace(req.CvStorageKey) ? null : req.CvStorageKey.Trim(),
            PipelineStatus = "New",
            CreatedBy = userId
        };
        _db.Candidates.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapCandidate(entity, post.Title, new Dictionary<Guid, string>());
    }

    public async Task<CandidateDto> UpdatePipelineAsync(
        Guid tenantId, Guid id, CandidatePipelineUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        var status = (req.PipelineStatus ?? "").Trim();
        if (!Pipeline.Contains(status))
            throw new AppException("Trạng thái pipeline không hợp lệ (New|Screening|Evaluating|Accepted|Rejected).");
        entity.PipelineStatus = status;
        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    public async Task<CandidateDto> EvaluateAsync(Guid tenantId, Guid id, CandidateEvalRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        if (req.EvalOrgUnitId is Guid ou)
        {
            var ouOk = await _db.OrgUnits.AnyAsync(x => x.Id == ou && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ouOk) throw new AppException("Đơn vị đánh giá không hợp lệ.", 404);
            entity.EvalOrgUnitId = ou;
            if (entity.PipelineStatus is "New" or "Screening")
                entity.PipelineStatus = "Evaluating";
        }
        if (req.EvalScore is int s)
        {
            if (s is < 0 or > 100) throw new AppException("Điểm 0–100.");
            entity.EvalScore = s;
        }
        if (req.EvalComment is not null)
            entity.EvalComment = req.EvalComment.Trim();
        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    public async Task<CandidateDto> AddCareNoteAsync(Guid tenantId, Guid id, CandidateCareNoteRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        var note = (req.Note ?? "").Trim();
        if (note.Length == 0) throw new AppException("Ghi chú trống.");
        var line = $"{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} · {note}";
        entity.CareNotes = string.IsNullOrWhiteSpace(entity.CareNotes) ? line : entity.CareNotes + "\n" + line;
        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    public async Task<IReadOnlyList<RecruitChannelStatDto>> ChannelStatsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var posts = await _db.JobPostings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var cand = await (
            from c in _db.Candidates.AsNoTracking()
            join p in _db.JobPostings.AsNoTracking() on c.JobPostingId equals p.Id
            where c.TenantId == tenantId && !c.IsDeleted
            group c by p.Channel into g
            select new { Channel = g.Key, Count = g.Count() }
        ).ToListAsync(ct);

        var channels = posts.Select(x => x.Channel).Union(cand.Select(x => x.Channel)).Distinct();
        return channels.Select(ch => new RecruitChannelStatDto(
            ch,
            posts.FirstOrDefault(x => x.Channel == ch)?.Count ?? 0,
            cand.FirstOrDefault(x => x.Channel == ch)?.Count ?? 0
        )).OrderBy(x => x.Channel).ToList();
    }

    private async Task<Candidate> GetCandidateAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _db.Candidates.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException("Ứng viên không tồn tại.", 404);

    private async Task<CandidateDto> MapLiveAsync(Candidate c, CancellationToken ct)
    {
        var title = await _db.JobPostings.Where(x => x.Id == c.JobPostingId).Select(x => x.Title).FirstAsync(ct);
        var orgs = new Dictionary<Guid, string>();
        if (c.EvalOrgUnitId is Guid ou)
        {
            var name = await _db.OrgUnits.Where(x => x.Id == ou).Select(x => x.Name).FirstOrDefaultAsync(ct);
            if (name is not null) orgs[ou] = name;
        }
        return MapCandidate(c, title, orgs);
    }

    private static CandidateDto MapCandidate(Candidate c, string title, IReadOnlyDictionary<Guid, string> orgs)
        => new(c.Id, c.JobPostingId, title, c.FullName, c.Email, c.Phone, c.CvStorageKey, c.PipelineStatus,
            c.EvalOrgUnitId,
            c.EvalOrgUnitId is Guid ou ? orgs.GetValueOrDefault(ou) : null,
            c.EvalScore, c.EvalComment, c.CareNotes, c.ConvertedEmployeeId, c.CreatedAt);
}
