using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Sys;

/// <summary>
/// Bước 154 — UC_SYS_064 (notif prefs), 071 (file scan), 077 (bulk export), 082 (IP allow/deny).
/// </summary>
public sealed class SysNotifScanExportIpService : ISysNotifScanExportIpService
{
    public static readonly HashSet<string> LockedSecurityEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "security.login_failed",
        "security.password_changed",
        "security.account_locked",
        "sys.ip_blocked"
    };

    private static readonly HashSet<string> AllowedExportEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        "Users", "Files", "AuditLogs"
    };

    private static readonly string EicarSignature = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

    private readonly AppDbContext _db;

    public SysNotifScanExportIpService(AppDbContext db) => _db = db;

    // ── 064 ─────────────────────────────────────────────────────────────────

    public async Task<SysNotificationPreferenceDto> GetMyNotificationPreferencesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var e = await _db.SysUserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct);
        return e is null ? DefaultPrefs(userId) : MapPrefs(e);
    }

    public async Task<SysNotificationPreferenceDto> UpsertMyNotificationPreferencesAsync(
        Guid tenantId, Guid userId, SysNotificationPreferenceUpsertRequest req, CancellationToken ct = default)
    {
        ValidateQuietHours(req.QuietHoursStart, req.QuietHoursEnd);

        var e = await _db.SysUserNotificationPreferences
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct);
        if (e is null)
        {
            e = new SysUserNotificationPreference
            {
                TenantId = tenantId, UserId = userId, CreatedBy = userId
            };
            _db.SysUserNotificationPreferences.Add(e);
        }

        e.ChannelInApp = req.ChannelInApp;
        e.ChannelEmail = req.ChannelEmail;
        e.ChannelSms = req.ChannelSms;
        e.ChannelPush = req.ChannelPush;
        e.MuteAll = req.MuteAll;
        e.QuietHoursStart = NormalizeHhMm(req.QuietHoursStart);
        e.QuietHoursEnd = NormalizeHhMm(req.QuietHoursEnd);
        e.UpdatedAt = DateTimeOffset.UtcNow;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapPrefs(e);
    }

    public bool ShouldDeliverInApp(SysNotificationPreferenceDto prefs, string eventType, DateTimeOffset utcNow)
        => ShouldDeliverInAppStatic(prefs, eventType, utcNow);

    public static bool ShouldDeliverInAppStatic(SysNotificationPreferenceDto prefs, string eventType, DateTimeOffset utcNow)
    {
        if (LockedSecurityEvents.Contains(eventType ?? ""))
            return true;
        if (prefs.MuteAll) return false;
        if (!prefs.ChannelInApp) return false;
        if (IsInQuietHours(prefs.QuietHoursStart, prefs.QuietHoursEnd, utcNow))
            return false;
        return true;
    }

    // ── 071 ─────────────────────────────────────────────────────────────────

    public async Task<SysFileScanStatusDto> ScanFileAsync(
        Guid tenantId, Guid userId, Guid fileId, string? contentHint, CancellationToken ct = default)
    {
        var file = await _db.FileObjects
                       .FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId && !x.IsDeleted, ct)
                   ?? throw new AppException("File không tồn tại.", 404);

        file.ScanStatus = "Scanning";
        file.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var infected = IsEicarThreat(file.FileName, contentHint);
        var status = infected ? "Infected" : "Clean";
        var threat = infected ? "EICAR-Test-File" : null;
        var now = DateTimeOffset.UtcNow;

        file.ScanStatus = status;
        file.ScannedAt = now;
        file.ThreatName = threat;
        file.UpdatedAt = now;
        file.UpdatedBy = userId;

        _db.SysFileScanLogs.Add(new SysFileScanLog
        {
            TenantId = tenantId,
            FileObjectId = file.Id,
            ScanStatus = status,
            Engine = "Day1-StubScanner",
            ThreatName = threat,
            Detail = infected ? "Phát hiện chữ ký EICAR (stub)." : "Không phát hiện mã độc (stub).",
            ScannedAt = now,
            ScannedByUserId = userId,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync(ct);

        return new SysFileScanStatusDto(file.Id, file.FileName, file.ScanStatus, file.ScannedAt, file.ThreatName, "Day1-StubScanner");
    }

    public async Task<SysFileScanStatusDto> GetFileScanStatusAsync(Guid tenantId, Guid fileId, CancellationToken ct = default)
    {
        var file = await _db.FileObjects.AsNoTracking()
                       .FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId && !x.IsDeleted, ct)
                   ?? throw new AppException("File không tồn tại.", 404);
        return new SysFileScanStatusDto(file.Id, file.FileName, file.ScanStatus, file.ScannedAt, file.ThreatName, null);
    }

    public async Task<IReadOnlyList<SysFileScanLogDto>> ListFileScanLogsAsync(
        Guid tenantId, Guid fileId, CancellationToken ct = default)
        => await _db.SysFileScanLogs.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.FileObjectId == fileId && !x.IsDeleted)
            .OrderByDescending(x => x.ScannedAt)
            .Take(50)
            .Select(x => new SysFileScanLogDto(x.Id, x.FileObjectId, x.ScanStatus, x.Engine, x.ThreatName, x.Detail, x.ScannedAt))
            .ToListAsync(ct);

    public async Task EnsureFileDownloadAllowedAsync(Guid tenantId, Guid fileId, CancellationToken ct = default)
    {
        var file = await _db.FileObjects.AsNoTracking()
                       .FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId && !x.IsDeleted, ct)
                   ?? throw new AppException("File không tồn tại.", 404);
        if (string.Equals(file.ScanStatus, "Infected", StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException($"File bị chặn tải xuống (threat: {file.ThreatName ?? "malware"}).");
        if (string.Equals(file.ScanStatus, "Scanning", StringComparison.OrdinalIgnoreCase))
            throw new AppException("File đang được quét, thử lại sau.");
    }

    // ── 077 ─────────────────────────────────────────────────────────────────

    public async Task<SysBulkExportJobDto> StartBulkExportAsync(
        Guid tenantId, Guid userId, SysBulkExportRequest req, CancellationToken ct = default)
    {
        var types = (req.EntityTypes ?? Array.Empty<string>())
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (types.Count == 0)
            throw new AppException("Chọn ít nhất 1 loại dữ liệu để xuất.");
        if (types.Count > 10)
            throw new AppException("Tối đa 10 loại dữ liệu mỗi lần xuất hàng loạt.");
        foreach (var t in types)
            if (!AllowedExportEntities.Contains(t))
                throw new AppException($"Loại '{t}' chưa hỗ trợ xuất hàng loạt. Hỗ trợ: Users, Files, AuditLogs.");

        var format = string.IsNullOrWhiteSpace(req.Format) ? "Csv" : req.Format.Trim();
        if (format is not ("Csv" or "Pdf"))
            throw new AppException("Format chỉ hỗ trợ Csv|Pdf.");

        var job = new ImportExportJob
        {
            TenantId = tenantId,
            JobType = "BulkExport",
            EntityType = string.Join(",", types),
            Format = format,
            Status = "Running",
            StartedAt = DateTimeOffset.UtcNow,
            ActorId = userId,
            CreatedBy = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
        _db.ImportExportJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        try
        {
            var sb = new StringBuilder();
            var total = 0;
            foreach (var t in types)
            {
                var (csv, rows) = await BuildEntityCsvAsync(tenantId, t, ct);
                sb.AppendLine($"### {t} ({rows} rows)");
                sb.AppendLine(csv);
                sb.AppendLine();
                total += rows;
            }

            byte[] data;
            string fileName;
            string contentType;
            if (format == "Pdf")
            {
                var pdf = new StringBuilder();
                pdf.AppendLine("%PDF-1.4");
                pdf.AppendLine("% BULK EXPORT");
                pdf.AppendLine(sb.ToString());
                pdf.AppendLine("%%EOF");
                data = Encoding.UTF8.GetBytes(pdf.ToString());
                fileName = $"bulk_export_{DateTimeOffset.UtcNow:yyyyMMdd_HHmm}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                data = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
                fileName = $"bulk_export_{DateTimeOffset.UtcNow:yyyyMMdd_HHmm}.csv";
                contentType = "text/csv; charset=utf-8";
            }

            job.Status = "Completed";
            job.RowCount = total;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ResultFileName = fileName;
            job.ResultContentType = contentType;
            job.ResultContent = Convert.ToBase64String(data);
            await _db.SaveChangesAsync(ct);
            return MapJob(job);
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorDetails = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<SysBulkExportJobDto>> ListExportJobsAsync(
        Guid tenantId, int take, CancellationToken ct = default)
    {
        if (take <= 0) take = 20;
        if (take > 200) take = 200;
        var jobs = await _db.ImportExportJobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && (j.JobType == "BulkExport" || j.JobType == "Export") && !j.IsDeleted)
            .OrderByDescending(j => j.StartedAt)
            .Take(take)
            .ToListAsync(ct);
        return jobs.Select(MapJob).ToList();
    }

    public async Task<SysBulkExportDownloadDto> DownloadExportJobAsync(
        Guid tenantId, Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.ImportExportJobs.AsNoTracking()
                      .FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenantId && !j.IsDeleted, ct)
                  ?? throw new AppException("Job xuất không tồn tại.", 404);
        if (!string.Equals(job.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Job chưa hoàn thành, không thể tải.");
        if (job.ExpiresAt is { } exp && exp < DateTimeOffset.UtcNow)
            throw new AppException("File xuất đã hết hạn.");
        if (string.IsNullOrWhiteSpace(job.ResultContent))
            throw new AppException("Job không có nội dung tải xuống (export sync cũ).");

        return new SysBulkExportDownloadDto(
            job.ResultFileName ?? "export.bin",
            job.ResultContentType ?? "application/octet-stream",
            Convert.FromBase64String(job.ResultContent));
    }

    // ── 082 ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SysIpRuleDto>> ListIpRulesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.SysIpRules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.RuleType).ThenBy(x => x.IpAddressOrCidr)
            .Select(x => new SysIpRuleDto(x.Id, x.IpAddressOrCidr, x.RuleType, x.Description, x.IsActive))
            .ToListAsync(ct);

    public async Task<SysIpRuleDto> UpsertIpRuleAsync(
        Guid tenantId, Guid userId, SysIpRuleUpsertRequest req, CancellationToken ct = default)
    {
        var cidr = (req.IpAddressOrCidr ?? "").Trim();
        if (string.IsNullOrWhiteSpace(cidr) || cidr.Length > 64)
            throw new AppException("IpAddressOrCidr bắt buộc, tối đa 64 ký tự.");
        if (!IsValidIpOrCidr(cidr))
            throw new AppException("IP/CIDR không hợp lệ (vd. 192.168.1.10 hoặc 10.0.0.0/8).");

        var ruleType = (req.RuleType ?? "").Trim();
        if (!ruleType.Equals("Allow", StringComparison.OrdinalIgnoreCase) &&
            !ruleType.Equals("Deny", StringComparison.OrdinalIgnoreCase))
            throw new AppException("RuleType phải là Allow|Deny.");
        ruleType = ruleType.Equals("Deny", StringComparison.OrdinalIgnoreCase) ? "Deny" : "Allow";

        SysIpRule entity;
        if (req.Id is Guid id)
        {
            entity = await _db.SysIpRules.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("IP rule không tồn tại.", 404);
        }
        else
        {
            entity = new SysIpRule { TenantId = tenantId, CreatedBy = userId };
            _db.SysIpRules.Add(entity);
        }

        entity.IpAddressOrCidr = cidr;
        entity.RuleType = ruleType;
        entity.Description = (req.Description ?? "").Trim();
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new SysIpRuleDto(entity.Id, entity.IpAddressOrCidr, entity.RuleType, entity.Description, entity.IsActive);
    }

    public async Task DeleteIpRuleAsync(Guid tenantId, Guid ruleId, CancellationToken ct = default)
    {
        var entity = await _db.SysIpRules.FirstOrDefaultAsync(x => x.Id == ruleId && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("IP rule không tồn tại.", 404);
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<SysIpCheckResult> EvaluateIpAsync(Guid tenantId, string? ip, CancellationToken ct = default)
    {
        var rules = await _db.SysIpRules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted)
            .ToListAsync(ct);
        if (rules.Count == 0)
            return new SysIpCheckResult(true, "no_rules");

        var clientIp = (ip ?? "").Trim();
        if (string.IsNullOrWhiteSpace(clientIp))
            return new SysIpCheckResult(false, "missing_ip");

        // Deny luôn thắng nếu khớp
        foreach (var d in rules.Where(r => r.RuleType.Equals("Deny", StringComparison.OrdinalIgnoreCase)))
        {
            if (IpMatches(clientIp, d.IpAddressOrCidr))
                return new SysIpCheckResult(false, $"deny:{d.IpAddressOrCidr}");
        }

        var allows = rules.Where(r => r.RuleType.Equals("Allow", StringComparison.OrdinalIgnoreCase)).ToList();
        if (allows.Count == 0)
            return new SysIpCheckResult(true, "denylist_only_pass");

        // Có Allow → allowlist: phải khớp ít nhất 1 Allow
        foreach (var a in allows)
        {
            if (IpMatches(clientIp, a.IpAddressOrCidr))
                return new SysIpCheckResult(true, $"allow:{a.IpAddressOrCidr}");
        }
        return new SysIpCheckResult(false, "not_in_allowlist");
    }

    public async Task EnsureIpAllowedAsync(Guid tenantId, string? ip, CancellationToken ct = default)
    {
        var result = await EvaluateIpAsync(tenantId, ip, ct);
        if (!result.Allowed)
            throw new ForbiddenException($"IP bị từ chối ({result.Reason}).");
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static SysNotificationPreferenceDto DefaultPrefs(Guid userId) => new(
        userId, true, true, false, true, false, null, null);

    private static SysNotificationPreferenceDto MapPrefs(SysUserNotificationPreference e) => new(
        e.UserId, e.ChannelInApp, e.ChannelEmail, e.ChannelSms, e.ChannelPush,
        e.MuteAll, e.QuietHoursStart, e.QuietHoursEnd);

    private static SysBulkExportJobDto MapJob(ImportExportJob j) => new(
        j.Id, j.JobType, j.EntityType, j.Format, j.Status, j.RowCount, j.ErrorCount,
        j.ErrorDetails, j.StartedAt, j.CompletedAt, j.ActorId, j.ResultFileName, j.ExpiresAt);

    private static void ValidateQuietHours(string? start, string? end)
    {
        var s = NormalizeHhMm(start);
        var e = NormalizeHhMm(end);
        if (s is null && e is null) return;
        if (s is null || e is null)
            throw new AppException("QuietHoursStart và QuietHoursEnd phải cùng có hoặc cùng trống.");
    }

    private static string? NormalizeHhMm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var v = value.Trim();
        if (!Regex.IsMatch(v, @"^([01]\d|2[0-3]):[0-5]\d$"))
            throw new AppException("Quiet hours phải dạng HH:mm (00:00–23:59).");
        return v;
    }

    public static bool IsInQuietHours(string? start, string? end, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end)) return false;
        if (!TimeSpan.TryParse(start, out var s) || !TimeSpan.TryParse(end, out var e)) return false;
        var t = utcNow.TimeOfDay;
        if (s <= e) return t >= s && t < e;
        // qua nửa đêm
        return t >= s || t < e;
    }

    private static bool IsEicarThreat(string fileName, string? contentHint)
    {
        if (!string.IsNullOrWhiteSpace(fileName) &&
            fileName.Contains("eicar", StringComparison.OrdinalIgnoreCase))
            return true;
        if (!string.IsNullOrWhiteSpace(contentHint) &&
            (contentHint.Contains(EicarSignature, StringComparison.Ordinal) ||
             contentHint.Contains("EICAR-STANDARD-ANTIVIRUS-TEST-FILE", StringComparison.OrdinalIgnoreCase)))
            return true;
        return false;
    }

    private async Task<(string Csv, int RowCount)> BuildEntityCsvAsync(Guid tenantId, string entityType, CancellationToken ct)
    {
        var sb = new StringBuilder();
        int rowCount;
        switch (entityType)
        {
            case "Users":
                sb.AppendLine("Username,DisplayName,Email,Status");
                var users = await _db.Users.AsNoTracking()
                    .Where(u => u.TenantId == tenantId && !u.IsDeleted).OrderBy(u => u.Username).ToListAsync(ct);
                foreach (var u in users)
                    sb.AppendLine($"{Esc(u.Username)},{Esc(u.DisplayName)},{Esc(u.Email)},{u.Status}");
                rowCount = users.Count;
                break;
            case "Files":
                sb.AppendLine("FileName,SizeBytes,ScanStatus,StorageKey");
                var files = await _db.FileObjects.AsNoTracking()
                    .Where(f => f.TenantId == tenantId && !f.IsDeleted).OrderByDescending(f => f.CreatedAt).Take(5000).ToListAsync(ct);
                foreach (var f in files)
                    sb.AppendLine($"{Esc(f.FileName)},{f.SizeBytes},{Esc(f.ScanStatus)},{Esc(f.StorageKey)}");
                rowCount = files.Count;
                break;
            case "AuditLogs":
                sb.AppendLine("Action,EntityType,EntityId,CreatedAt");
                var logs = await _db.AuditLogs.AsNoTracking()
                    .Where(a => a.TenantId == tenantId).OrderByDescending(a => a.CreatedAt).Take(5000).ToListAsync(ct);
                foreach (var a in logs)
                    sb.AppendLine($"{Esc(a.Action)},{Esc(a.EntityType)},{a.EntityId},{a.CreatedAt:yyyy-MM-dd HH:mm}");
                rowCount = logs.Count;
                break;
            default:
                throw new AppException($"Loại '{entityType}' chưa hỗ trợ.");
        }
        return (sb.ToString(), rowCount);
    }

    private static string Esc(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }

    private static bool IsValidIpOrCidr(string value)
    {
        var parts = value.Split('/', 2);
        if (!IPAddress.TryParse(parts[0], out var addr)) return false;
        if (parts.Length == 1) return true;
        if (!int.TryParse(parts[1], out var prefix)) return false;
        var max = addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
        return prefix >= 0 && prefix <= max;
    }

    public static bool IpMatches(string clientIp, string rule)
    {
        if (!IPAddress.TryParse(clientIp, out var client)) return false;
        var parts = rule.Split('/', 2);
        if (!IPAddress.TryParse(parts[0], out var network)) return false;
        if (parts.Length == 1)
            return client.Equals(network);

        if (!int.TryParse(parts[1], out var prefix)) return false;
        var clientBytes = client.GetAddressBytes();
        var netBytes = network.GetAddressBytes();
        if (clientBytes.Length != netBytes.Length) return false;
        var fullBytes = prefix / 8;
        var remBits = prefix % 8;
        for (var i = 0; i < fullBytes; i++)
            if (clientBytes[i] != netBytes[i]) return false;
        if (remBits == 0) return true;
        var mask = (byte)(0xFF << (8 - remBits));
        return (clientBytes[fullBytes] & mask) == (netBytes[fullBytes] & mask);
    }
}
