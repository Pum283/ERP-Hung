using System.Text.RegularExpressions;
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

    // Trạng thái cho phép sơ loại (UC_HRM_059)
    private static readonly HashSet<string> ScreenableStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "New", "Screening" };

    // Valid evaluation results (UC_HRM_061)
    private static readonly HashSet<string> ValidEvalResults = new(StringComparer.OrdinalIgnoreCase)
        { "Pass", "Fail", "Hold" };

    // Regex validate email & phone (UC_HRM_056)
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex =
        new(@"^[\d\+\-\(\)\s]{9,15}$", RegexOptions.Compiled);

    private readonly AppDbContext _db;

    public HrmRecruitPipelineService(AppDbContext db) => _db = db;

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_055 + UC_HRM_054 — JobPosting (tin tuyển & kênh đăng)
    // ──────────────────────────────────────────────────────────────────────────

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
        if (title.Length < 5) throw new AppException("Tiêu đề tin tuyển quá ngắn (tối thiểu 5 ký tự).");

        // UC_HRM_055: validate channel
        var channel = string.IsNullOrWhiteSpace(req.Channel) ? "Internal" : req.Channel.Trim();
        if (channel.Length > 40) throw new AppException("Kênh tối đa 40 ký tự.");
        var validChannels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Internal", "Website", "Facebook", "LinkedIn", "Other" };
        if (!validChannels.Contains(channel))
            throw new AppException($"Kênh không hợp lệ. Chấp nhận: {string.Join(", ", validChannels)}.");

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
        if (p.Status == "Closed") throw new AppException("Tin tuyển đã đóng.");
        p.Status = "Closed";
        await _db.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_056 + UC_HRM_057 — Nhập hồ sơ ứng viên & Upload CV
    // ──────────────────────────────────────────────────────────────────────────

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
        // Validate JobPosting
        var post = await _db.JobPostings.FirstOrDefaultAsync(
            x => x.Id == req.JobPostingId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Tin tuyển không tồn tại.", 404);
        if (post.Status != "Open") throw new AppException("Tin đã đóng, không nhận ứng viên.");

        // UC_HRM_056: validate FullName
        var name = (req.FullName ?? "").Trim();
        if (name.Length == 0) throw new AppException("Họ tên ứng viên trống.");
        if (name.Length > 200) throw new AppException("Họ tên ứng viên tối đa 200 ký tự.");

        // UC_HRM_056: validate Email format
        string? email = null;
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            email = req.Email.Trim();
            if (!EmailRegex.IsMatch(email))
                throw new AppException($"Địa chỉ email không hợp lệ: {email}.");
        }

        // UC_HRM_056: validate Phone format
        string? phone = null;
        if (!string.IsNullOrWhiteSpace(req.Phone))
        {
            phone = req.Phone.Trim();
            if (!PhoneRegex.IsMatch(phone))
                throw new AppException("Số điện thoại không hợp lệ (chỉ chấp nhận 9–15 ký tự số).");
        }

        // UC_HRM_056: chống trùng Email trong cùng JobPosting
        if (email is not null)
        {
            var dupEmail = await _db.Candidates.AnyAsync(
                x => x.JobPostingId == req.JobPostingId
                     && x.TenantId == tenantId
                     && !x.IsDeleted
                     && x.Email != null
                     && x.Email.ToLower() == email.ToLower(), ct);
            if (dupEmail)
                throw new AppException($"Email '{email}' đã được đăng ký cho tin tuyển này.");
        }

        // UC_HRM_056: chống trùng Phone trong cùng JobPosting
        if (phone is not null)
        {
            var dupPhone = await _db.Candidates.AnyAsync(
                x => x.JobPostingId == req.JobPostingId
                     && x.TenantId == tenantId
                     && !x.IsDeleted
                     && x.Phone != null
                     && x.Phone == phone, ct);
            if (dupPhone)
                throw new AppException($"Số điện thoại '{phone}' đã được đăng ký cho tin tuyển này.");
        }

        // UC_HRM_057: CvStorageKey
        string? cvKey = string.IsNullOrWhiteSpace(req.CvStorageKey) ? null : req.CvStorageKey.Trim();

        var entity = new Candidate
        {
            TenantId = tenantId,
            JobPostingId = post.Id,
            FullName = name,
            Email = email,
            Phone = phone,
            CvStorageKey = cvKey,
            PipelineStatus = "New",
            CreatedBy = userId
        };
        _db.Candidates.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapCandidate(entity, post.Title, new Dictionary<Guid, string>());
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_063 — Pipeline trạng thái ứng viên (Strict State Machine)
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CandidateDto> UpdatePipelineAsync(
        Guid tenantId, Guid id, CandidatePipelineUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        var newStatus = (req.PipelineStatus ?? "").Trim();
        if (!Pipeline.Contains(newStatus))
            throw new AppException("Trạng thái pipeline không hợp lệ (New|Screening|Evaluating|Accepted|Rejected).");

        // UC_HRM_063: Validate state transition
        ValidateStateTransition(entity.PipelineStatus, newStatus);

        // UC_HRM_063: Check posting is Open
        var posting = await _db.JobPostings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entity.JobPostingId && !x.IsDeleted, ct);
        if (posting is not null && posting.Status == "Closed")
            throw new AppException("Tin tuyển đã đóng, không thể thay đổi trạng thái ứng viên.");

        entity.PipelineStatus = newStatus;
        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    public async Task<CandidateDto> EvaluateAsync(Guid tenantId, Guid id, CandidateEvalRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        if (entity.PipelineStatus is "Accepted" or "Rejected")
            throw new AppException($"Ứng viên đã ở trạng thái '{entity.PipelineStatus}', không thể đánh giá.");

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
            if (s is < 0 or > 100) throw new AppException("Điểm đánh giá phải trong khoảng 0–100.");
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
        if (note.Length == 0) throw new AppException("Ghi chú chăm sóc trống.");
        if (note.Length > 1000) throw new AppException("Ghi chú chăm sóc tối đa 1000 ký tự.");
        var line = $"{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} · {note}";
        entity.CareNotes = string.IsNullOrWhiteSpace(entity.CareNotes) ? line : entity.CareNotes + "\n" + line;
        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_059 — Sơ loại ứng viên
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CandidateDto> ScreenCandidateAsync(
        Guid tenantId, Guid id, CandidateScreenRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);

        var action = (req.Action ?? "").Trim();
        var note = (req.ScreeningNote ?? "").Trim();

        if (action is not ("Screen" or "ScreenReject"))
            throw new AppException("Hành động sơ loại không hợp lệ. Chấp nhận: Screen | ScreenReject.");

        if (note.Length == 0)
            throw new AppException(action == "Screen"
                ? "Vui lòng nhập ghi chú sơ loại (lý do tiếp tục vòng tiếp theo)."
                : "Vui lòng nhập lý do từ chối sơ loại.");
        if (note.Length > 500)
            throw new AppException("Ghi chú sơ loại tối đa 500 ký tự.");

        if (!ScreenableStatuses.Contains(entity.PipelineStatus))
            throw new AppException(
                $"Không thể sơ loại ứng viên ở trạng thái '{entity.PipelineStatus}'. " +
                "Chỉ sơ loại được ứng viên ở trạng thái New hoặc Screening.");

        var posting = await _db.JobPostings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entity.JobPostingId && !x.IsDeleted, ct);
        if (posting is not null && posting.Status == "Closed")
            throw new AppException("Tin tuyển đã đóng, không thể sơ loại ứng viên.");

        if (action == "Screen")
        {
            entity.PipelineStatus = "Screening";
            entity.ScreeningNote = note;
        }
        else
        {
            entity.PipelineStatus = "Rejected";
            entity.ScreeningNote = note;
        }

        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_060 — Chuyển ứng viên cho đơn vị đánh giá
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CandidateDto> AssignEvalOrgUnitAsync(
        Guid tenantId, Guid id, CandidateAssignEvalOrgRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        if (entity.PipelineStatus is "Accepted" or "Rejected")
            throw new AppException($"Ứng viên đã ở trạng thái '{entity.PipelineStatus}', không thể chuyển đơn vị đánh giá.");

        var ou = await _db.OrgUnits.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EvalOrgUnitId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đơn vị đánh giá không tồn tại.", 404);

        entity.EvalOrgUnitId = ou.Id;
        if (entity.PipelineStatus is "New" or "Screening")
            entity.PipelineStatus = "Evaluating";

        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_061 — Form đánh giá ứng viên chi tiết
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CandidateDto> SubmitEvaluationAsync(
        Guid tenantId, Guid id, CandidateSubmitEvalRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        if (entity.PipelineStatus is "Accepted" or "Rejected")
            throw new AppException($"Ứng viên đã ở trạng thái '{entity.PipelineStatus}', không thể đánh giá.");

        if (req.EvalScore is < 0 or > 100)
            throw new AppException("Điểm đánh giá phải trong khoảng 0–100.");

        var result = (req.EvalResult ?? "").Trim();
        if (!ValidEvalResults.Contains(result))
            throw new AppException("Kết quả đề xuất đánh giá không hợp lệ (Pass|Fail|Hold).");

        var comment = (req.EvalComment ?? "").Trim();
        if (comment.Length > 1000)
            throw new AppException("Nhận xét đánh giá tối đa 1000 ký tự.");

        if (req.EvalOrgUnitId is Guid ouId)
        {
            var ouOk = await _db.OrgUnits.AnyAsync(x => x.Id == ouId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ouOk) throw new AppException("Đơn vị đánh giá không hợp lệ.", 404);
            entity.EvalOrgUnitId = ouId;
        }

        entity.EvalScore = req.EvalScore;
        entity.EvalResult = result;
        entity.EvalComment = comment;

        if (entity.PipelineStatus is "New" or "Screening")
            entity.PipelineStatus = "Evaluating";

        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_062 — Ra quyết định tuyển dụng (Accept / Reject)
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CandidateDto> DecideCandidateAsync(
        Guid tenantId, Guid id, CandidateDecideRequest req, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        var action = (req.Action ?? "").Trim();
        var note = (req.DecisionNote ?? "").Trim();

        if (action is not ("Accept" or "Reject"))
            throw new AppException("Hành động quyết định không hợp lệ. Chấp nhận: Accept | Reject.");

        if (note.Length == 0)
            throw new AppException(action == "Accept"
                ? "Vui lòng nhập ghi chú thư mời làm việc / nhận việc."
                : "Vui lòng nhập lý do từ chối ứng viên.");
        if (note.Length > 1000)
            throw new AppException("Ghi chú quyết định tối đa 1000 ký tự.");

        if (action == "Accept")
        {
            var posting = await _db.JobPostings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.JobPostingId && !x.IsDeleted, ct);
            if (posting is not null && posting.Status == "Closed")
                throw new AppException("Tin tuyển đã đóng, không thể chấp nhận ứng viên.");

            if (entity.PipelineStatus == "Accepted")
                throw new AppException("Ứng viên đã ở trạng thái Chấp nhận (Accepted).");

            entity.PipelineStatus = "Accepted";
            entity.DecisionNote = note;
        }
        else // Reject
        {
            if (entity.PipelineStatus == "Accepted")
                throw new AppException("Ứng viên đã ở trạng thái Chấp nhận (Accepted), không thể từ chối.");

            entity.PipelineStatus = "Rejected";
            entity.DecisionNote = note;
        }

        await _db.SaveChangesAsync(ct);
        return await MapLiveAsync(entity, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_055 — Thống kê kênh đăng tuyển
    // ──────────────────────────────────────────────────────────────────────────

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

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_064 — Lịch sử chăm sóc ứng viên
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CareNoteItemDto>> GetCareNotesAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await GetCandidateAsync(tenantId, id, ct);
        if (string.IsNullOrWhiteSpace(entity.CareNotes))
            return Array.Empty<CareNoteItemDto>();

        var result = new List<CareNoteItemDto>();
        var lines = entity.CareNotes.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            var sepIdx = line.IndexOf(" · ", StringComparison.Ordinal);
            if (sepIdx > 0)
            {
                var timePart = line[..sepIdx].Trim();
                var notePart = line[(sepIdx + 3)..].Trim();
                if (DateTimeOffset.TryParse(timePart, out var parsedAt))
                {
                    result.Add(new CareNoteItemDto(parsedAt, notePart));
                    continue;
                }
            }
            result.Add(new CareNoteItemDto(DateTimeOffset.UtcNow, line));
        }

        return result;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UC_HRM_065 — Báo cáo hiệu quả kênh tuyển
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<RecruitChannelReportDto>> GetChannelReportAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var postingCounts = await _db.JobPostings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var candFunnel = await (
            from c in _db.Candidates.AsNoTracking()
            join p in _db.JobPostings.AsNoTracking() on c.JobPostingId equals p.Id
            where c.TenantId == tenantId && !c.IsDeleted
            group c by p.Channel into g
            select new
            {
                Channel = g.Key,
                CandidateCount = g.Count(),
                ScreeningCount = g.Count(x => x.PipelineStatus == "Screening"),
                EvaluatingCount = g.Count(x => x.PipelineStatus == "Evaluating"),
                AcceptedCount = g.Count(x => x.PipelineStatus == "Accepted"),
                RejectedCount = g.Count(x => x.PipelineStatus == "Rejected"),
            }
        ).ToListAsync(ct);

        var allChannels = postingCounts.Select(x => x.Channel)
            .Union(candFunnel.Select(x => x.Channel))
            .Distinct();

        return allChannels.Select(ch =>
        {
            var pc = postingCounts.FirstOrDefault(x => x.Channel == ch)?.Count ?? 0;
            var f = candFunnel.FirstOrDefault(x => x.Channel == ch);
            var cc = f?.CandidateCount ?? 0;
            var sc = f?.ScreeningCount ?? 0;
            var ec = f?.EvaluatingCount ?? 0;
            var ac = f?.AcceptedCount ?? 0;
            var rc = f?.RejectedCount ?? 0;
            var rate = cc == 0 ? 0.0 : Math.Round((double)ac / cc * 100.0, 2);

            return new RecruitChannelReportDto(ch, pc, cc, sc, ec, ac, rc, rate);
        }).OrderBy(x => x.Channel).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static void ValidateStateTransition(string current, string next)
    {
        if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase)) return;

        // Terminal states cannot transition out easily
        if (current is "Accepted" or "Rejected")
            throw new AppException($"Ứng viên ở trạng thái kết thúc '{current}', không thể đổi sang '{next}'.");

        // Allowed transitions:
        // New -> Screening, Rejected
        // Screening -> Evaluating, Rejected
        // Evaluating -> Accepted, Rejected
        bool valid = (current, next) switch
        {
            ("New", "Screening" or "Rejected") => true,
            ("Screening", "Evaluating" or "Rejected") => true,
            ("Evaluating", "Accepted" or "Rejected") => true,
            _ => false
        };

        if (!valid)
            throw new AppException($"Không thể chuyển trạng thái trực tiếp từ '{current}' sang '{next}'.");
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
            c.EvalScore, c.EvalComment, c.CareNotes, c.ConvertedEmployeeId,
            c.ScreeningNote, c.EvalResult, c.DecisionNote, c.CreatedAt);
}
