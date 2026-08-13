using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Realtime;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Base;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Sys;

public sealed class SysPlatformService : ISysPlatformService
{
    private static readonly ModuleCatalogItemDto[] Modules =
    [
        new("SYS", "Hệ thống", true),
        new("HRM", "Nhân sự", false),
        new("WF", "Workflow", false),
        new("LMS", "Đào tạo", false),
        new("AST", "Tài sản", false),
        new("FIN", "Tài chính", false),
        new("CRM", "CRM", false),
        new("INV", "Kho", false),
    ];

    private readonly AppDbContext _db;
    private readonly IOutboxWriter _outbox;

    public SysPlatformService(AppDbContext db, IOutboxWriter outbox)
    {
        _db = db;
        _outbox = outbox;
    }

    public async Task<PasswordPolicyDto> GetPasswordPolicyAsync(Guid tenantId, CancellationToken ct = default)
    {
        var json = await GetSettingRawAsync(tenantId, "password.policy", ct);
        if (json is null) return DefaultPolicy();
        return JsonSerializer.Deserialize<PasswordPolicyDto>(json) ?? DefaultPolicy();
    }

    public async Task<PasswordPolicyDto> SetPasswordPolicyAsync(Guid tenantId, PasswordPolicyDto policy, CancellationToken ct = default)
    {
        await UpsertSettingAsync(tenantId, "password.policy", JsonSerializer.Serialize(policy), ct);
        return policy;
    }

    public static PasswordPolicyDto DefaultPolicy() => new(8, true, true, true, 5, 15, 120);

    public async Task ValidatePasswordAsync(Guid tenantId, string password, CancellationToken ct = default)
    {
        var p = await GetPasswordPolicyAsync(tenantId, ct);
        if (password.Length < p.MinLength) throw new AppException($"Mật khẩu tối thiểu {p.MinLength} ký tự.");
        if (p.RequireDigit && !password.Any(char.IsDigit)) throw new AppException("Mật khẩu cần ít nhất 1 chữ số.");
        if (p.RequireUpper && !password.Any(char.IsUpper)) throw new AppException("Mật khẩu cần ít nhất 1 chữ hoa.");
        if (p.RequireLower && !password.Any(char.IsLower)) throw new AppException("Mật khẩu cần ít nhất 1 chữ thường.");
    }

    public async Task<TenantDto> GetTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var t = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Tenant không tồn tại.", 404);
        return MapTenant(t);
    }

    public async Task<TenantDto> UpdateTenantAsync(Guid tenantId, TenantUpdateRequest req, CancellationToken ct = default)
    {
        var t = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Tenant không tồn tại.", 404);
        t.Name = req.Name.Trim();
        t.Status = req.Status;
        t.Timezone = req.Timezone;
        t.DefaultLocale = req.DefaultLocale;
        t.DefaultCurrency = req.DefaultCurrency;
        t.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapTenant(t);
    }

    public async Task<TenantDto> SetTenantLogoAsync(Guid tenantId, string? logoUrl, string? storageKey, CancellationToken ct = default)
    {
        var t = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Tenant không tồn tại.", 404);
        t.LogoUrl = logoUrl;
        t.LogoStorageKey = storageKey;
        t.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapTenant(t);
    }

    public Task<IReadOnlyList<ModuleCatalogItemDto>> ListModulesAsync()
        => Task.FromResult<IReadOnlyList<ModuleCatalogItemDto>>(Modules);

    public async Task<IReadOnlyList<LicenseDto>> ListLicensesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Licenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.ValidTo)
            .Select(x => new LicenseDto(x.Id, x.PlanCode, x.ValidFrom, x.ValidTo, x.MaxUsers, x.MaxOrgUnits, x.Status))
            .ToListAsync(ct);

    public async Task<LicenseDto> UpsertLicenseAsync(Guid tenantId, Guid? actorId, LicenseUpsertRequest req, CancellationToken ct = default)
    {
        License e;
        if (req.Id is Guid id)
        {
            e = await _db.Licenses.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("License không tồn tại.", 404);
        }
        else
        {
            e = new License { TenantId = tenantId, CreatedBy = actorId };
            _db.Licenses.Add(e);
        }
        e.PlanCode = req.PlanCode.Trim();
        e.ValidFrom = req.ValidFrom;
        e.ValidTo = req.ValidTo;
        e.MaxUsers = req.MaxUsers;
        e.MaxOrgUnits = req.MaxOrgUnits;
        e.Status = req.Status;
        e.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new LicenseDto(e.Id, e.PlanCode, e.ValidFrom, e.ValidTo, e.MaxUsers, e.MaxOrgUnits, e.Status);
    }

    public async Task<LicenseStatusDto> GetLicenseStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var lic = await _db.Licenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "Active" && !x.IsDeleted)
            .OrderByDescending(x => x.ValidTo)
            .FirstOrDefaultAsync(ct) ?? throw new AppException("Chưa có license Active.", 404);
        var users = await _db.Users.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        var orgs = await _db.OrgUnits.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        var days = lic.ValidTo.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
        return new LicenseStatusDto(lic.PlanCode, lic.ValidTo, days, lic.MaxUsers, users, lic.MaxOrgUnits, orgs,
            days <= 14, users > lic.MaxUsers, orgs > lic.MaxOrgUnits);
    }

    public async Task EnsureMaxUsersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var st = await GetLicenseStatusAsync(tenantId, ct);
        if (st.CurrentUsers >= st.MaxUsers)
            throw new AppException($"Đã đạt giới hạn {st.MaxUsers} user theo gói license.");
    }

    public async Task EnsureMaxOrgUnitsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var st = await GetLicenseStatusAsync(tenantId, ct);
        if (st.CurrentOrgUnits >= st.MaxOrgUnits)
            throw new AppException($"Đã đạt giới hạn {st.MaxOrgUnits} chi nhánh theo gói license.");
    }

    public async Task<IReadOnlyList<SystemSettingDto>> ListSettingsAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.SystemSettings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new SystemSettingDto(x.Key, x.ValueJson))
            .ToListAsync(ct);

    public async Task UpsertSettingAsync(Guid tenantId, string key, string valueJson, CancellationToken ct = default)
    {
        var e = await _db.SystemSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key && !x.IsDeleted, ct);
        if (e is null)
        {
            e = new SystemSetting { TenantId = tenantId, Key = key };
            _db.SystemSettings.Add(e);
        }
        e.ValueJson = valueJson;
        e.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<string?> GetSettingValueAsync(Guid tenantId, string key, Guid? orgUnitId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        if (orgUnitId is Guid orgId && orgId != Guid.Empty)
        {
            var branchKey = $"OrgUnit:{orgId}:{key.Trim()}";
            var branchSetting = await _db.SystemSettings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == branchKey && !x.IsDeleted, ct);
            if (branchSetting is not null) return branchSetting.ValueJson;
        }

        var tenantSetting = await _db.SystemSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key.Trim() && !x.IsDeleted, ct);
        return tenantSetting?.ValueJson;
    }

    public async Task UpsertOrgUnitSettingAsync(Guid tenantId, Guid orgUnitId, string key, string valueJson, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new AppException("Mã cấu hình không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(valueJson))
            throw new AppException("Giá trị cấu hình không được để trống.", 400);

        var orgExists = await _db.OrgUnits.AnyAsync(x => x.Id == orgUnitId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!orgExists)
            throw new AppException("Chi nhánh không tồn tại trong hệ thống.", 400);

        var branchKey = $"OrgUnit:{orgUnitId}:{key.Trim()}";
        var e = await _db.SystemSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == branchKey && !x.IsDeleted, ct);
        if (e is null)
        {
            e = new SystemSetting { TenantId = tenantId, Key = branchKey };
            _db.SystemSettings.Add(e);
        }
        e.ValueJson = valueJson;
        e.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<LookupCategoryDto>> ListLookupCategoriesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.LookupCategories.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new LookupCategoryDto(x.Id, x.Code, x.Name, x.IsActive)).ToListAsync(ct);

    public async Task<LookupCategoryDto> UpsertLookupCategoryAsync(Guid tenantId, Guid? actorId, LookupCategoryDto req, CancellationToken ct = default)
    {
        LookupCategory e;
        if (req.Id != Guid.Empty && await _db.LookupCategories.AnyAsync(x => x.Id == req.Id, ct))
        {
            e = await _db.LookupCategories.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        }
        else
        {
            e = new LookupCategory { TenantId = tenantId, CreatedBy = actorId };
            _db.LookupCategories.Add(e);
        }
        e.Code = req.Code.Trim();
        e.Name = req.Name.Trim();
        e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new LookupCategoryDto(e.Id, e.Code, e.Name, e.IsActive);
    }

    public async Task<IReadOnlyList<LookupItemDto>> ListLookupItemsAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default)
        => await _db.LookupItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CategoryId == categoryId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LookupItemDto(x.Id, x.CategoryId, x.Code, x.Name, x.SortOrder, x.IsActive))
            .ToListAsync(ct);

    public async Task<LookupItemDto> UpsertLookupItemAsync(Guid tenantId, Guid? actorId, LookupItemDto req, CancellationToken ct = default)
    {
        LookupItem e;
        if (req.Id != Guid.Empty && await _db.LookupItems.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.LookupItems.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else
        {
            e = new LookupItem { TenantId = tenantId, CreatedBy = actorId };
            _db.LookupItems.Add(e);
        }
        e.CategoryId = req.CategoryId;
        e.Code = req.Code.Trim();
        e.Name = req.Name.Trim();
        e.SortOrder = req.SortOrder;
        e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new LookupItemDto(e.Id, e.CategoryId, e.Code, e.Name, e.SortOrder, e.IsActive);
    }

    public async Task<IReadOnlyList<NumberSequenceDto>> ListNumberSequencesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.NumberSequences.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new NumberSequenceDto(x.Id, x.DocType, x.Pattern, x.NextValue)).ToListAsync(ct);

    public async Task<NumberSequenceDto> UpsertNumberSequenceAsync(Guid tenantId, Guid? actorId, NumberSequenceDto req, CancellationToken ct = default)
    {
        NumberSequence e;
        if (req.Id != Guid.Empty && await _db.NumberSequences.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.NumberSequences.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else
        {
            e = new NumberSequence { TenantId = tenantId, CreatedBy = actorId, ResetYear = DateTime.UtcNow.Year };
            _db.NumberSequences.Add(e);
        }
        e.DocType = req.DocType.Trim();
        e.Pattern = string.IsNullOrWhiteSpace(req.Pattern) ? "{yyyy}-{seq:5}" : req.Pattern;
        if (req.NextValue > 0) e.NextValue = req.NextValue;
        await _db.SaveChangesAsync(ct);
        return new NumberSequenceDto(e.Id, e.DocType, e.Pattern, e.NextValue);
    }

    public async Task<string> NextNumberAsync(Guid tenantId, string docType, CancellationToken ct = default)
    {
        var e = await _db.NumberSequences.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.DocType == docType && !x.IsDeleted, ct);
        if (e is null)
        {
            e = new NumberSequence { TenantId = tenantId, DocType = docType, Pattern = "{yyyy}-{seq:5}", NextValue = 1, ResetYear = DateTime.UtcNow.Year };
            _db.NumberSequences.Add(e);
        }
        var year = DateTime.UtcNow.Year;
        if (e.ResetYear != year) { e.ResetYear = year; e.NextValue = 1; }
        var seq = e.NextValue++;
        await _db.SaveChangesAsync(ct);
        return e.Pattern
            .Replace("{yyyy}", year.ToString())
            .Replace("{seq:5}", seq.ToString("D5"))
            .Replace("{seq}", seq.ToString());
    }

    public async Task<IReadOnlyList<LoginAuditDto>> ListLoginAuditsAsync(Guid tenantId, int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 200);
        return await _db.LoginAudits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.AttemptedAt)
            .Take(take)
            .Select(x => new LoginAuditDto(x.Id, x.UserId, x.Username, x.Success, x.IpAddress, x.FailureReason, x.AttemptedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AppNotificationDto>> ListNotificationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
        => await _db.AppNotifications.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(100)
            .Select(x => new AppNotificationDto(x.Id, x.Title, x.Body, x.Link, x.EventType, x.IsRead, x.CreatedAt))
            .ToListAsync(ct);

    public async Task MarkNotificationReadAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var n = await _db.AppNotifications.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.UserId == userId, ct)
                ?? throw new AppException("Thông báo không tồn tại.", 404);
        n.IsRead = true;
        n.ReadAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllNotificationsReadAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var rows = await _db.AppNotifications
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsRead && !x.IsDeleted)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var n in rows)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }
        await _db.SaveChangesAsync(ct);
    }

    public Task<int> UnreadNotificationCountAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
        => _db.AppNotifications.CountAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsRead && !x.IsDeleted, ct);

    public async Task<IReadOnlyList<NotificationRuleDto>> ListNotificationRulesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.NotificationRules.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new NotificationRuleDto(x.Id, x.EventType, x.TitleTemplate, x.BodyTemplate, x.IsEnabled))
            .ToListAsync(ct);

    public async Task<NotificationRuleDto> UpsertNotificationRuleAsync(Guid tenantId, Guid? actorId, NotificationRuleDto req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EventType))
            throw new AppException("Mã sự kiện không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.TitleTemplate))
            throw new AppException("Tiêu đề thông báo mẫu không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.BodyTemplate))
            throw new AppException("Nội dung thông báo mẫu không được để trống.", 400);

        var eventTypeTrim = req.EventType.Trim();
        var isDup = await _db.NotificationRules.AnyAsync(x => x.TenantId == tenantId && x.EventType == eventTypeTrim && x.Id != req.Id && !x.IsDeleted, ct);
        if (isDup)
            throw new AppException($"Quy tắc thông báo cho sự kiện '{eventTypeTrim}' đã tồn tại.", 400);

        NotificationRule e;
        if (req.Id != Guid.Empty && await _db.NotificationRules.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.NotificationRules.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else
        {
            e = new NotificationRule { TenantId = tenantId, CreatedBy = actorId };
            _db.NotificationRules.Add(e);
        }
        e.EventType = eventTypeTrim;
        e.TitleTemplate = req.TitleTemplate;
        e.BodyTemplate = req.BodyTemplate;
        e.IsEnabled = req.IsEnabled;
        await _db.SaveChangesAsync(ct);
        return new NotificationRuleDto(e.Id, e.EventType, e.TitleTemplate, e.BodyTemplate, e.IsEnabled);
    }

    public async Task NotifyEventAsync(Guid tenantId, Guid targetUserId, string eventType, string? link, IDictionary<string, string>? vars, CancellationToken ct = default)
    {
        // UC_SYS_064 — tôn trọng tùy chọn cá nhân (trừ sự kiện bảo mật bắt buộc)
        var prefsEntity = await _db.SysUserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == targetUserId && !x.IsDeleted, ct);
        var prefs = prefsEntity is null
            ? new SysNotificationPreferenceDto(targetUserId, true, true, false, true, false, null, null)
            : new SysNotificationPreferenceDto(
                prefsEntity.UserId, prefsEntity.ChannelInApp, prefsEntity.ChannelEmail,
                prefsEntity.ChannelSms, prefsEntity.ChannelPush, prefsEntity.MuteAll,
                prefsEntity.QuietHoursStart, prefsEntity.QuietHoursEnd);
        if (!SysNotifScanExportIpService.ShouldDeliverInAppStatic(prefs, eventType, DateTimeOffset.UtcNow))
            return;

        var rule = await _db.NotificationRules.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EventType == eventType && x.IsEnabled && !x.IsDeleted, ct);
        var title = rule?.TitleTemplate ?? eventType;
        var body = rule?.BodyTemplate ?? eventType;
        if (vars is not null)
        {
            foreach (var kv in vars)
            {
                title = title.Replace("{" + kv.Key + "}", kv.Value);
                body = body.Replace("{" + kv.Key + "}", kv.Value);
            }
        }
        _db.AppNotifications.Add(new AppNotification
        {
            TenantId = tenantId, UserId = targetUserId, Title = title, Body = body, Link = link, EventType = eventType
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<byte[]> ExportUsersCsvAsync(Guid tenantId, Guid currentUserId, CancellationToken ct = default)
    {
        var users = await _db.Users.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Username).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("username,displayName,email,phone,status,primaryOrgUnitId");
        foreach (var u in users)
            sb.AppendLine($"{Escape(u.Username)},{Escape(u.DisplayName)},{Escape(u.Email)},{Escape(u.Phone)},{u.Status},{u.PrimaryOrgUnitId}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public Task<string> GetUserImportTemplateAsync()
        => Task.FromResult("username,displayName,email,phone,password,status,primaryOrgUnitId\r\nuser1,User One,u1@example.com,,!Abc123,Active,\r\n");

    public async Task<(int Ok, int Fail, IReadOnlyList<string> Errors)> ImportUsersCsvAsync(Guid tenantId, Guid actorId, Stream csv, CancellationToken ct = default)
    {
        using var reader = new StreamReader(csv, Encoding.UTF8);
        var header = await reader.ReadLineAsync(ct);
        var errors = new List<string>();
        var ok = 0;
        var fail = 0;
        var lineNo = 1;
        while (!reader.EndOfStream)
        {
            lineNo++;
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = SplitCsv(line);
            try
            {
                await EnsureMaxUsersAsync(tenantId, ct);
                var username = parts.ElementAtOrDefault(0)?.Trim() ?? "";
                if (string.IsNullOrEmpty(username)) throw new AppException("Thiếu username");
                var existing = await _db.Users.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Username == username && !x.IsDeleted, ct);
                if (existing is null)
                {
                    existing = new AppUser { TenantId = tenantId, Username = username, CreatedBy = actorId };
                    _db.Users.Add(existing);
                }
                existing.DisplayName = parts.ElementAtOrDefault(1);
                existing.Email = parts.ElementAtOrDefault(2);
                existing.Phone = parts.ElementAtOrDefault(3);
                var pwd = parts.ElementAtOrDefault(4);
                if (!string.IsNullOrWhiteSpace(pwd))
                {
                    await ValidatePasswordAsync(tenantId, pwd, ct);
                    existing.PasswordHash = PasswordHasher.Hash(pwd);
                }
                if (Enum.TryParse<UserStatus>(parts.ElementAtOrDefault(5) ?? "Active", true, out var st))
                    existing.Status = st;
                if (Guid.TryParse(parts.ElementAtOrDefault(6), out var orgId))
                    existing.PrimaryOrgUnitId = orgId;
                await _db.SaveChangesAsync(ct);
                ok++;
            }
            catch (Exception ex)
            {
                fail++;
                errors.Add($"Dòng {lineNo}: {ex.Message}");
            }
        }
        return (ok, fail, errors);
    }

    public async Task<IReadOnlyList<LegalEntityDto>> ListLegalEntitiesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.LegalEntities.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new LegalEntityDto(x.Id, x.Code, x.Name, x.TaxCode, x.IsActive)).ToListAsync(ct);

    public async Task<LegalEntityDto> UpsertLegalEntityAsync(Guid tenantId, Guid? actorId, LegalEntityDto req, CancellationToken ct = default)
    {
        var e = await UpsertNamedAsync(_db.LegalEntities, tenantId, actorId, req.Id, ct);
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.TaxCode = req.TaxCode; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new LegalEntityDto(e.Id, e.Code, e.Name, e.TaxCode, e.IsActive);
    }

    public async Task<IReadOnlyList<SalesPointDto>> ListSalesPointsAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.SalesPoints.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new SalesPointDto(x.Id, x.Code, x.Name, x.OrgUnitId, x.Address, x.IsActive)).ToListAsync(ct);

    public async Task<SalesPointDto> UpsertSalesPointAsync(Guid tenantId, Guid? actorId, SalesPointDto req, CancellationToken ct = default)
    {
        SalesPoint e;
        if (req.Id != Guid.Empty && await _db.SalesPoints.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.SalesPoints.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else
        {
            e = new SalesPoint { TenantId = tenantId, CreatedBy = actorId };
            _db.SalesPoints.Add(e);
        }
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.OrgUnitId = req.OrgUnitId; e.Address = req.Address; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new SalesPointDto(e.Id, e.Code, e.Name, e.OrgUnitId, e.Address, e.IsActive);
    }

    public async Task<IReadOnlyList<ProvinceDto>> ListProvincesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Provinces.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new ProvinceDto(x.Id, x.Code, x.Name, x.IsActive)).ToListAsync(ct);

    public async Task<ProvinceDto> UpsertProvinceAsync(Guid tenantId, Guid? actorId, ProvinceDto req, CancellationToken ct = default)
    {
        var e = await UpsertNamedAsync(_db.Provinces, tenantId, actorId, req.Id, ct);
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new ProvinceDto(e.Id, e.Code, e.Name, e.IsActive);
    }

    public async Task<IReadOnlyList<OrgNodeDto>> GetOrgChartAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.OrgUnits.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new OrgNodeDto(x.Id, x.Code, x.Name, x.ParentId, x.UnitType)).ToListAsync(ct);

    public async Task<IReadOnlyList<WorkCalendarDto>> ListWorkCalendarsAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.WorkCalendars.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new WorkCalendarDto(x.Id, x.Code, x.Name, x.WeekMask, x.HolidaysJson, x.IsActive)).ToListAsync(ct);

    public async Task<WorkCalendarDto> UpsertWorkCalendarAsync(Guid tenantId, Guid? actorId, WorkCalendarDto req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            throw new AppException("Mã lịch làm việc (Code) không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.Name))
            throw new AppException("Tên lịch làm việc (Name) không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.WeekMask) || req.WeekMask.Length != 7 || !req.WeekMask.All(c => c == '0' || c == '1'))
            throw new AppException("WeekMask phải đúng 7 ký tự gồm '0' và '1' (Ví dụ '1111100' cho Thứ 2 - Thứ 6).", 400);

        if (!string.IsNullOrWhiteSpace(req.HolidaysJson))
        {
            try { JsonDocument.Parse(req.HolidaysJson); }
            catch { throw new AppException("HolidaysJson không phải là chuỗi JSON hợp lệ.", 400); }
        }

        WorkCalendar e;
        if (req.Id != Guid.Empty && await _db.WorkCalendars.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.WorkCalendars.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new WorkCalendar { TenantId = tenantId, CreatedBy = actorId }; _db.WorkCalendars.Add(e); }
        e.Code = req.Code.Trim().ToUpperInvariant(); e.Name = req.Name.Trim(); e.WeekMask = req.WeekMask; e.HolidaysJson = req.HolidaysJson; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new WorkCalendarDto(e.Id, e.Code, e.Name, e.WeekMask, e.HolidaysJson, e.IsActive);
    }

    public async Task<IReadOnlyList<MessageTemplateDto>> ListMessageTemplatesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.MessageTemplates.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new MessageTemplateDto(x.Id, x.Code, x.Channel, x.Subject, x.Body, x.IsActive)).ToListAsync(ct);

    public async Task<MessageTemplateDto> UpsertMessageTemplateAsync(Guid tenantId, Guid? actorId, MessageTemplateDto req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            throw new AppException("Mã template không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.Body))
            throw new AppException("Nội dung template không được để trống.", 400);

        if (string.Equals(req.Channel, "Email", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(req.Subject))
            throw new AppException("Tiêu đề email không được để trống.", 400);

        var codeTrim = req.Code.Trim();
        var isDup = await _db.MessageTemplates.AnyAsync(x => x.TenantId == tenantId && x.Code == codeTrim && x.Channel == req.Channel && x.Id != req.Id && !x.IsDeleted, ct);
        if (isDup)
            throw new AppException($"Mẫu tin nhắn với mã '{codeTrim}' và kênh '{req.Channel}' đã tồn tại.", 400);

        MessageTemplate e;
        if (req.Id != Guid.Empty && await _db.MessageTemplates.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.MessageTemplates.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new MessageTemplate { TenantId = tenantId, CreatedBy = actorId }; _db.MessageTemplates.Add(e); }
        e.Code = codeTrim; e.Channel = req.Channel; e.Subject = req.Subject ?? ""; e.Body = req.Body; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new MessageTemplateDto(e.Id, e.Code, e.Channel, e.Subject, e.Body, e.IsActive);
    }

    public async Task<ChannelSendResultDto> SendChannelMessageAsync(
        Guid tenantId, Guid? actorId, ChannelSendRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Channel))
            throw new AppException("Kênh gửi không được để trống.", 400);

        var channel = req.Channel.Trim();
        if (!channel.Equals("Email", StringComparison.OrdinalIgnoreCase) &&
            !channel.Equals("Sms", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("Kênh gửi không hợp lệ. Hệ thống chỉ hỗ trợ 'Email' hoặc 'Sms'.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.Target))
            throw new AppException("Người nhận (Target) không được để trống.", 400);

        var target = req.Target.Trim();
        if (channel.Equals("Email", StringComparison.OrdinalIgnoreCase))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(target, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new AppException("Địa chỉ Email người nhận không hợp lệ.", 400);
        }
        else if (channel.Equals("Sms", StringComparison.OrdinalIgnoreCase))
        {
            var cleanedPhone = System.Text.RegularExpressions.Regex.Replace(target, @"[^\d+]", "");
            if (cleanedPhone.Length < 8 || cleanedPhone.Length > 15)
                throw new AppException("Số điện thoại người nhận không hợp lệ.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.TemplateCode))
            throw new AppException("Mã template không được để trống.", 400);

        var tpl = await _db.MessageTemplates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == req.TemplateCode.Trim() && x.IsActive && !x.IsDeleted, ct);

        if (tpl is null)
            throw new AppException($"Mẫu tin nhắn '{req.TemplateCode}' không tồn tại hoặc đã bị khóa.", 404);

        var subject = tpl.Subject ?? "";
        var body = tpl.Body ?? "";

        if (req.Vars is not null)
        {
            foreach (var kv in req.Vars)
            {
                subject = subject.Replace("{" + kv.Key + "}", kv.Value);
                body = body.Replace("{" + kv.Key + "}", kv.Value);
            }
        }

        var log = new IntegrationCallLog
        {
            TenantId = tenantId,
            Kind = channel,
            Target = target,
            StatusCode = 200,
            RequestSummary = $"Template: {tpl.Code} | Event: {req.EventType ?? "Direct"}",
            CalledAt = DateTimeOffset.UtcNow
        };
        _db.IntegrationCallLogs.Add(log);

        await _outbox.EnqueueAsync(tenantId, req.EventType ?? "ChannelMessageSent", "SYS", new
        {
            Channel = channel,
            Target = target,
            TemplateCode = tpl.Code,
            Subject = subject,
            Body = body,
            SentAt = DateTimeOffset.UtcNow
        }, ct: ct);

        await _db.SaveChangesAsync(ct);

        return new ChannelSendResultDto(
            log.Id, channel, target, tpl.Code, subject, body, "Success", null, null);
    }

    public async Task<IReadOnlyList<FileFolderDto>> ListFoldersAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.FileFolders.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new FileFolderDto(x.Id, x.Name, x.ParentId)).ToListAsync(ct);

    public async Task<FileFolderDto> UpsertFolderAsync(Guid tenantId, Guid? actorId, FileFolderDto req, CancellationToken ct = default)
    {
        FileFolder e;
        if (req.Id != Guid.Empty && await _db.FileFolders.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.FileFolders.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new FileFolder { TenantId = tenantId, CreatedBy = actorId }; _db.FileFolders.Add(e); }
        e.Name = req.Name.Trim(); e.ParentId = req.ParentId;
        await _db.SaveChangesAsync(ct);
        return new FileFolderDto(e.Id, e.Name, e.ParentId);
    }

    public async Task SoftDeleteFileAsync(Guid tenantId, Guid fileId, CancellationToken ct = default)
    {
        var f = await _db.FileObjects.FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId, ct)
                ?? throw new AppException("File không tồn tại.", 404);
        f.IsDeleted = true; f.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RestoreFileAsync(Guid tenantId, Guid fileId, CancellationToken ct = default)
    {
        var f = await _db.FileObjects.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId, ct)
                ?? throw new AppException("File không tồn tại.", 404);
        f.IsDeleted = false; f.DeletedAt = null;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FileObjectDto>> ListFilesAsync(Guid tenantId, Guid? folderId, CancellationToken ct = default)
    {
        var q = _db.FileObjects.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId);
        if (folderId is Guid fid) q = q.Where(x => x.FolderId == fid);
        return await q.OrderByDescending(x => x.CreatedAt)
            .Select(x => new FileObjectDto(x.Id, x.StorageKey, x.FileName, x.ContentType, x.SizeBytes, x.FolderId, x.IsDeleted))
            .Take(200).ToListAsync(ct);
    }

    public async Task<FileObjectDto> UploadFileMetadataAsync(Guid tenantId, Guid? actorId, FileUploadRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.FileName))
            throw new AppException("Tên file không được để trống.", 400);

        if (req.SizeBytes <= 0)
            throw new AppException("Kích thước file phải lớn hơn 0 byte.", 400);

        if (req.SizeBytes > 50 * 1024 * 1024)
            throw new AppException("Kích thước file không được vượt quá 50MB.", 400);

        if (req.FolderId is Guid fid && fid != Guid.Empty)
        {
            var folderExists = await _db.FileFolders.AnyAsync(x => x.Id == fid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!folderExists)
                throw new AppException("Thư mục lưu trữ không tồn tại.", 400);
        }

        var cleanName = req.FileName.Trim();
        var storageKey = $"{tenantId:N}/{Guid.NewGuid():N}_{cleanName}";

        var f = new FileObject
        {
            TenantId = tenantId,
            FileName = cleanName,
            ContentType = req.ContentType ?? "application/octet-stream",
            SizeBytes = req.SizeBytes,
            StorageKey = storageKey,
            FolderId = req.FolderId,
            CreatedBy = actorId
        };

        _db.FileObjects.Add(f);
        await _db.SaveChangesAsync(ct);

        return new FileObjectDto(f.Id, f.StorageKey, f.FileName, f.ContentType, f.SizeBytes, f.FolderId, f.IsDeleted);
    }

    public async Task<FileObjectDto> GetFileObjectAsync(Guid tenantId, Guid fileId, CancellationToken ct = default)
    {
        var f = await _db.FileObjects.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (f is null)
            throw new AppException("File không tồn tại hoặc đã bị xóa.", 404);

        return new FileObjectDto(f.Id, f.StorageKey, f.FileName, f.ContentType, f.SizeBytes, f.FolderId, f.IsDeleted);
    }

    public async Task<IReadOnlyList<ApiKeyDto>> ListApiKeysAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.ApiKeys.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new ApiKeyDto(x.Id, x.Name, x.KeyPrefix, x.ExpiresAt, x.IsActive, x.LastUsedAt)).ToListAsync(ct);

    public async Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid tenantId, Guid? actorId, string name, CancellationToken ct = default)
    {
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var prefix = plain[..8];
        var e = new ApiKey
        {
            TenantId = tenantId, Name = name.Trim(), KeyPrefix = prefix,
            KeyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plain))),
            IsActive = true, CreatedBy = actorId
        };
        _db.ApiKeys.Add(e);
        await _db.SaveChangesAsync(ct);
        return new ApiKeyCreatedDto(e.Id, e.Name, e.KeyPrefix, plain);
    }

    public async Task RevokeApiKeyAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.ApiKeys.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct)
                ?? throw new AppException("API key không tồn tại.", 404);
        e.IsActive = false; e.IsDeleted = true; e.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WebhookDto>> ListWebhooksAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.WebhookSubscriptions.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new WebhookDto(x.Id, x.Name, x.TargetUrl, x.EventTypes, x.IsActive)).ToListAsync(ct);

    public async Task<WebhookDto> UpsertWebhookAsync(Guid tenantId, Guid? actorId, WebhookDto req, CancellationToken ct = default)
    {
        WebhookSubscription e;
        if (req.Id != Guid.Empty && await _db.WebhookSubscriptions.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.WebhookSubscriptions.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new WebhookSubscription { TenantId = tenantId, CreatedBy = actorId }; _db.WebhookSubscriptions.Add(e); }
        e.Name = req.Name.Trim(); e.TargetUrl = req.TargetUrl.Trim(); e.EventTypes = req.EventTypes; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new WebhookDto(e.Id, e.Name, e.TargetUrl, e.EventTypes, e.IsActive);
    }

    public async Task<IReadOnlyList<IntegrationLogDto>> ListIntegrationLogsAsync(Guid tenantId, int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 200);
        return await _db.IntegrationCallLogs.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CalledAt).Take(take)
            .Select(x => new IntegrationLogDto(x.Id, x.Kind, x.Target, x.StatusCode, x.RequestSummary, x.CalledAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExternalIntegrationDto>> ListIntegrationsAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.ExternalIntegrations.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new ExternalIntegrationDto(x.Id, x.Code, x.Name, x.Kind, x.ConfigJson, x.IsActive)).ToListAsync(ct);

    public async Task<ExternalIntegrationDto> UpsertIntegrationAsync(Guid tenantId, Guid? actorId, ExternalIntegrationDto req, CancellationToken ct = default)
    {
        ExternalIntegration e;
        if (req.Id != Guid.Empty && await _db.ExternalIntegrations.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.ExternalIntegrations.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new ExternalIntegration { TenantId = tenantId, CreatedBy = actorId }; _db.ExternalIntegrations.Add(e); }
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.Kind = req.Kind; e.ConfigJson = req.ConfigJson; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new ExternalIntegrationDto(e.Id, e.Code, e.Name, e.Kind, e.ConfigJson, e.IsActive);
    }

    private async Task<MessageTemplate> EnsureMessageTemplateAsync(
        Guid tenantId, Guid? actorId, string code, string channel, CancellationToken ct)
    {
        var existing = await _db.MessageTemplates
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId && !x.IsDeleted && x.IsActive
                && x.Code == code && x.Channel == channel, ct);
        if (existing is not null) return existing;

        var (subject, body) = DefaultTemplateContent(code, channel);
        var created = new MessageTemplate
        {
            TenantId = tenantId, Code = code, Channel = channel,
            Subject = subject, Body = body, IsActive = true, CreatedBy = actorId,
        };
        _db.MessageTemplates.Add(created);
        await _db.SaveChangesAsync(ct);
        return created;
    }

    private async Task<ExternalIntegration> EnsureChannelIntegrationAsync(
        Guid tenantId, Guid? actorId, string channel, CancellationToken ct)
    {
        var existing = await _db.ExternalIntegrations
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId && !x.IsDeleted && x.IsActive && x.Kind == channel, ct);
        if (existing is not null) return existing;

        var code = channel == "SMS" ? "SMS_STUB" : "EMAIL_STUB";
        var created = new ExternalIntegration
        {
            TenantId = tenantId, Code = code,
            Name = channel == "SMS" ? "SMS stub (log)" : "Email stub (log)",
            Kind = channel,
            ConfigJson = """{"mode":"stub","provider":"log"}""",
            IsActive = true, CreatedBy = actorId,
        };
        _db.ExternalIntegrations.Add(created);
        await _db.SaveChangesAsync(ct);
        return created;
    }

    private static (string Subject, string Body) DefaultTemplateContent(string code, string channel)
    {
        if (code.Equals("FORGOT_PASSWORD", StringComparison.OrdinalIgnoreCase))
        {
            return channel == "SMS"
                ? ("", "Ma OTP dat lai MK Pum's ERP: {otp}. Het han {expiresMinutes} phut.")
                : ("[Pum's ERP] Ma OTP dat lai mat khau",
                    "Xin chao {displayName},\n\nMa OTP dat lai mat khau: {otp}\nHet han sau {expiresMinutes} phut.\n\nNeu ban khong yeu cau, hay bo qua email nay.");
        }
        if (code.Equals("USER_INVITE", StringComparison.OrdinalIgnoreCase))
        {
            return channel == "SMS"
                ? ("", "Moi vao Pum's ERP. User: {username}. OTP kich hoat: {otp} (het han {expiresMinutes}p).")
                : ("[Pum's ERP] Loi moi tai khoan",
                    "Xin chao {displayName},\n\nBan duoc moi vao he thong Pum's ERP.\nUsername: {username}\nOTP kich hoat / dat MK: {otp}\nHet han: {expiresMinutes} phut.\n\nDang nhap roi doi mat khau ngay.");
        }
        return channel == "SMS"
            ? ("", "Thong bao Pum's ERP: {message}")
            : ("[Pum's ERP] Thong bao", "Xin chao {displayName},\n\n{message}");
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string>? vars)
    {
        var s = template ?? "";
        if (vars is null) return s;
        foreach (var kv in vars)
            s = s.Replace("{" + kv.Key + "}", kv.Value ?? "", StringComparison.OrdinalIgnoreCase);
        return s;
    }

    public async Task<IReadOnlyList<LocalePackDto>> ListLocalePacksAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LocalePacks.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new LocalePackDto(x.Id, x.Code, x.Name, x.IsDefault, x.IsActive)).ToListAsync(ct);
        if (list.Count == 0)
        {
            _db.LocalePacks.Add(new LocalePack { TenantId = tenantId, Code = "vi", Name = "Tiếng Việt", IsDefault = true });
            _db.LocalePacks.Add(new LocalePack { TenantId = tenantId, Code = "en", Name = "English", IsDefault = false });
            await _db.SaveChangesAsync(ct);
            return await ListLocalePacksAsync(tenantId, ct);
        }
        return list;
    }

    public async Task SetUserLocaleAsync(Guid tenantId, Guid userId, string localeCode, CancellationToken ct = default)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("User không tồn tại.", 404);
        u.PreferredLocale = localeCode;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<RoleMatrixRowDto>> GetPermissionMatrixAsync(Guid tenantId, CancellationToken ct = default)
    {
        var roles = await _db.Roles.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var rps = await _db.RolePermissions.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var perms = await _db.Permissions.AsNoTracking().Where(x => !x.IsDeleted).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        return roles.Select(r => new RoleMatrixRowDto(
            r.Id, r.Code, r.Name,
            rps.Where(x => x.RoleId == r.Id && perms.ContainsKey(x.PermissionId)).Select(x => perms[x.PermissionId]).OrderBy(x => x).ToList()
        )).ToList();
    }

    public async Task<IReadOnlyList<PermissionChangeLogDto>> ListPermissionChangeLogsAsync(Guid tenantId, int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 200);
        return await _db.PermissionChangeLogs.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(take)
            .Select(x => new PermissionChangeLogDto(x.Id, x.ActorUserId, x.ChangeType, x.RoleId, x.TargetUserId, x.DetailJson, x.CreatedAt))
            .ToListAsync(ct);
    }

    private async Task<string?> GetSettingRawAsync(Guid tenantId, string key, CancellationToken ct)
        => await _db.SystemSettings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Key == key && !x.IsDeleted)
            .Select(x => x.ValueJson).FirstOrDefaultAsync(ct);

    private static TenantDto MapTenant(Tenant t)
        => new(t.Id, t.Code, t.Name, t.Status, t.Timezone, t.DefaultLocale, t.DefaultCurrency, t.LogoUrl);

    private static async Task<T> UpsertNamedAsync<T>(DbSet<T> set, Guid tenantId, Guid? actorId, Guid id, CancellationToken ct)
        where T : TenantEntity, new()
    {
        if (id != Guid.Empty)
        {
            var existing = await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
            if (existing is not null) return existing;
        }
        var e = new T { TenantId = tenantId, CreatedBy = actorId };
        set.Add(e);
        return e;
    }

    private static string Escape(string? s)
    {
        s ??= "";
        if (s.Contains(',') || s.Contains('"')) return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        var cur = new StringBuilder();
        var inQ = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"') { inQ = !inQ; continue; }
            if (c == ',' && !inQ) { result.Add(cur.ToString()); cur.Clear(); continue; }
            cur.Append(c);
        }
        result.Add(cur.ToString());
        return result;
    }

    // ─────────── UC_SYS_069 — Phân quyền file theo đối tượng (LinkedEntity ACL) ───────────

    public async Task LinkFileToEntityAsync(Guid tenantId, Guid? actorId, LinkFileToEntityRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EntityType))
            throw new AppException("Loại đối tượng (EntityType) không được để trống.", 400);

        if (req.EntityId == Guid.Empty)
            throw new AppException("ID đối tượng (EntityId) không được để trống.", 400);

        var file = await _db.FileObjects.FirstOrDefaultAsync(x => x.Id == req.FileId && x.TenantId == tenantId && !x.IsDeleted, ct)
                   ?? throw new AppException("File không tồn tại hoặc đã bị xóa.", 404);

        if (file.LinkedEntityType == req.EntityType.Trim() && file.LinkedEntityId == req.EntityId)
            throw new AppException("File đã được gắn vào đối tượng này rồi.", 400);

        if (file.LinkedEntityType is not null && file.LinkedEntityId is not null)
            throw new AppException("File đã được gắn vào một đối tượng khác. Hãy gỡ liên kết cũ trước.", 400);

        file.LinkedEntityType = req.EntityType.Trim();
        file.LinkedEntityId = req.EntityId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<LinkedFileDto>> ListLinkedFilesAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new AppException("Loại đối tượng (EntityType) không được để trống.", 400);

        return await _db.FileObjects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.LinkedEntityType == entityType.Trim() && x.LinkedEntityId == entityId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new LinkedFileDto(x.Id, x.FileName, x.ContentType, x.SizeBytes, x.LinkedEntityType!, x.LinkedEntityId!.Value, x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task UnlinkFileFromEntityAsync(Guid tenantId, Guid fileId, string entityType, Guid entityId, CancellationToken ct = default)
    {
        var file = await _db.FileObjects.FirstOrDefaultAsync(x => x.Id == fileId && x.TenantId == tenantId, ct)
                   ?? throw new AppException("File không tồn tại.", 404);

        if (file.LinkedEntityType != entityType.Trim() || file.LinkedEntityId != entityId)
            throw new AppException("File không được gắn vào đối tượng này.", 400);

        file.LinkedEntityType = null;
        file.LinkedEntityId = null;
        await _db.SaveChangesAsync(ct);
    }

    // ─────────── UC_SYS_074/075 — Export Excel (CSV) / PDF ───────────

    public async Task<GenericExportResult> ExportEntityDataAsync(Guid tenantId, Guid? actorId, GenericExportRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EntityType))
            throw new AppException("Loại đối tượng xuất dữ liệu (EntityType) không được để trống.", 400);

        var format = req.Format?.Trim() ?? "Csv";
        if (format != "Csv" && format != "Pdf")
            throw new AppException("Định dạng xuất chỉ hỗ trợ 'Csv' hoặc 'Pdf'.", 400);

        var job = new ImportExportJob
        {
            TenantId = tenantId,
            JobType = "Export",
            EntityType = req.EntityType.Trim(),
            Format = format,
            Status = "Running",
            StartedAt = DateTimeOffset.UtcNow,
            ActorId = actorId
        };
        _db.ImportExportJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        try
        {
            var (csvContent, rowCount) = await BuildExportCsvAsync(tenantId, req.EntityType.Trim(), ct);

            byte[] data;
            string fileName;
            string contentType;

            if (format == "Pdf")
            {
                var pdf = new StringBuilder();
                pdf.AppendLine("%PDF-1.4");
                pdf.AppendLine($"% XUẤT DỮ LIỆU — {req.EntityType}");
                pdf.AppendLine($"% Tenant: {tenantId} | Ngày xuất: {DateTimeOffset.UtcNow.ToLocalTime():dd/MM/yyyy HH:mm}");
                pdf.AppendLine(new string('=', 60));
                pdf.AppendLine(csvContent);
                pdf.AppendLine(new string('=', 60));
                pdf.AppendLine($"% Tổng số bản ghi: {rowCount}");
                pdf.AppendLine("%%EOF");

                data = Encoding.UTF8.GetBytes(pdf.ToString());
                fileName = $"{req.EntityType}_{DateTimeOffset.UtcNow:yyyyMMdd_HHmm}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                data = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvContent)).ToArray();
                fileName = $"{req.EntityType}_{DateTimeOffset.UtcNow:yyyyMMdd_HHmm}.csv";
                contentType = "text/csv; charset=utf-8";
            }

            job.Status = "Completed";
            job.RowCount = rowCount;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            return new GenericExportResult(fileName, contentType, data, rowCount);
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

    private async Task<(string Csv, int RowCount)> BuildExportCsvAsync(Guid tenantId, string entityType, CancellationToken ct)
    {
        var sb = new StringBuilder();
        int rowCount;

        switch (entityType)
        {
            case "Users":
                sb.AppendLine("Username,DisplayName,Email,Phone,Status,CreatedAt");
                var users = await _db.Users.AsNoTracking().Where(u => u.TenantId == tenantId && !u.IsDeleted)
                    .OrderBy(u => u.Username).ToListAsync(ct);
                foreach (var u in users)
                    sb.AppendLine($"{Escape(u.Username)},{Escape(u.DisplayName)},{Escape(u.Email)},{Escape(u.Phone)},{u.Status},{u.CreatedAt:yyyy-MM-dd}");
                rowCount = users.Count;
                break;

            case "Files":
                sb.AppendLine("FileName,ContentType,SizeBytes,FolderId,StorageKey,IsDeleted");
                var files = await _db.FileObjects.IgnoreQueryFilters().AsNoTracking()
                    .Where(f => f.TenantId == tenantId).OrderByDescending(f => f.CreatedAt).Take(5000).ToListAsync(ct);
                foreach (var f in files)
                    sb.AppendLine($"{Escape(f.FileName)},{Escape(f.ContentType)},{f.SizeBytes},{f.FolderId},{Escape(f.StorageKey)},{f.IsDeleted}");
                rowCount = files.Count;
                break;

            case "AuditLogs":
                sb.AppendLine("Action,EntityType,EntityId,ActorUserId,BeforeJson,CreatedAt");
                var logs = await _db.AuditLogs.AsNoTracking().Where(a => a.TenantId == tenantId)
                    .OrderByDescending(a => a.CreatedAt).Take(5000).ToListAsync(ct);
                foreach (var a in logs)
                    sb.AppendLine($"{Escape(a.Action)},{Escape(a.EntityType)},{a.EntityId},{a.ActorUserId},{Escape(a.BeforeJson)},{a.CreatedAt:yyyy-MM-dd HH:mm}");
                rowCount = logs.Count;
                break;

            default:
                throw new AppException($"Loại đối tượng '{entityType}' chưa được hỗ trợ xuất dữ liệu.", 400);
        }

        return (sb.ToString(), rowCount);
    }

    // ─────────── UC_SYS_076 — Lịch sử job import/export ───────────

    public async Task<IReadOnlyList<ImportExportJobDto>> ListImportExportJobsAsync(Guid tenantId, int take, CancellationToken ct = default)
    {
        if (take <= 0) take = 20;
        if (take > 500) take = 500;

        return await _db.ImportExportJobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId)
            .OrderByDescending(j => j.StartedAt)
            .Take(take)
            .Select(j => new ImportExportJobDto(j.Id, j.JobType, j.EntityType, j.Format, j.Status, j.RowCount, j.ErrorCount, j.ErrorDetails, j.StartedAt, j.CompletedAt, j.ActorId))
            .ToListAsync(ct);
    }

    // ─────────── UC_SYS_078 — Nhật ký thao tác người dùng ───────────

    public async Task<(IReadOnlyList<AuditLogDto> Items, int Total)> ListAuditLogsAsync(
        Guid tenantId, AuditLogQueryRequest query, CancellationToken ct = default)
    {
        // Validate page/pageSize
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 50 : query.PageSize > 500 ? 500 : query.PageSize;

        // Validate dateRange: From phải <= To nếu cả hai đều có
        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
            throw new AppException("Ngày bắt đầu không được lớn hơn ngày kết thúc.", 400);

        var q = _db.AuditLogs.AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query.EntityType))
            q = q.Where(a => a.EntityType == query.EntityType.Trim());

        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(a => a.Action == query.Action.Trim());

        if (query.ActorUserId.HasValue)
            q = q.Where(a => a.ActorUserId == query.ActorUserId.Value);

        if (query.From.HasValue)
            q = q.Where(a => a.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(a => a.CreatedAt <= query.To.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(a.Id, a.EntityType, a.EntityId, a.Action,
                a.BeforeJson, a.AfterJson, a.ActorUserId, a.IpAddress, a.CreatedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    // ─────────── UC_SYS_080 — Xem chi tiết thay đổi field ───────────

    public async Task<AuditLogDetailDto> GetAuditLogDetailAsync(
        Guid tenantId, Guid auditLogId, CancellationToken ct = default)
    {
        var log = await _db.AuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auditLogId && a.TenantId == tenantId, ct)
            ?? throw new AppException("Không tìm thấy bản ghi audit log.", 404);

        var diffs = ComputeFieldDiffs(log.BeforeJson, log.AfterJson, log.Action);

        return new AuditLogDetailDto(
            log.Id, log.EntityType, log.EntityId, log.Action,
            log.ActorUserId, log.IpAddress, log.CreatedAt,
            log.BeforeJson, log.AfterJson, diffs);
    }

    /// <summary>
    /// So sánh hai JSON object (Before / After) và trả về danh sách field đã thay đổi.
    /// Xử lý an toàn khi JSON malformed — trả raw diff thay vì crash.
    /// </summary>
    private static IReadOnlyList<FieldDiffDto> ComputeFieldDiffs(string? beforeJson, string? afterJson, string action)
    {
        var diffs = new List<FieldDiffDto>();

        // CREATE: chỉ có After
        if (string.IsNullOrWhiteSpace(beforeJson) && !string.IsNullOrWhiteSpace(afterJson))
        {
            var after = TryParseDict(afterJson);
            if (after is null)
            {
                diffs.Add(new FieldDiffDto("(raw)", null, afterJson, "Created"));
                return diffs;
            }
            foreach (var kv in after)
                diffs.Add(new FieldDiffDto(kv.Key, null, kv.Value, "Added"));
            return diffs;
        }

        // DELETE: chỉ có Before
        if (!string.IsNullOrWhiteSpace(beforeJson) && string.IsNullOrWhiteSpace(afterJson))
        {
            var before = TryParseDict(beforeJson);
            if (before is null)
            {
                diffs.Add(new FieldDiffDto("(raw)", beforeJson, null, "Deleted"));
                return diffs;
            }
            foreach (var kv in before)
                diffs.Add(new FieldDiffDto(kv.Key, kv.Value, null, "Removed"));
            return diffs;
        }

        // UPDATE: cả Before lẫn After
        if (!string.IsNullOrWhiteSpace(beforeJson) && !string.IsNullOrWhiteSpace(afterJson))
        {
            var before = TryParseDict(beforeJson);
            var after  = TryParseDict(afterJson);

            if (before is null || after is null)
            {
                // Malformed JSON — trả raw so sánh
                diffs.Add(new FieldDiffDto("(raw)", beforeJson, afterJson, "Modified"));
                return diffs;
            }

            var allKeys = before.Keys.Union(after.Keys).Distinct();
            foreach (var key in allKeys)
            {
                var oldVal = before.GetValueOrDefault(key);
                var newVal = after.GetValueOrDefault(key);
                if (oldVal == newVal) continue;

                var kind = oldVal is null ? "Added" : newVal is null ? "Removed" : "Modified";
                diffs.Add(new FieldDiffDto(key, oldVal, newVal, kind));
            }
            return diffs;
        }

        return diffs;
    }

    private static Dictionary<string, string?>? TryParseDict(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
            return doc.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.ValueKind == JsonValueKind.Null
                    ? (string?)null
                    : p.Value.ToString());
        }
        catch
        {
            return null;
        }
    }

    // ─────────── UC_SYS_081 — Xuất audit log CSV ───────────

    public async Task<GenericExportResult> ExportAuditLogCsvAsync(
        Guid tenantId, AuditLogExportRequest req, CancellationToken ct = default)
    {
        // Validate: khoảng thời gian bắt buộc
        if (req.From >= req.To)
            throw new AppException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.", 400);

        // Giới hạn tối đa khoảng xuất: 366 ngày
        if ((req.To - req.From).TotalDays > 366)
            throw new AppException("Khoảng thời gian xuất tối đa 366 ngày.", 400);

        const int MaxRows = 10_000;

        var q = _db.AuditLogs.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.CreatedAt >= req.From && a.CreatedAt <= req.To);

        if (!string.IsNullOrWhiteSpace(req.EntityType))
            q = q.Where(a => a.EntityType == req.EntityType.Trim());

        if (!string.IsNullOrWhiteSpace(req.Action))
            q = q.Where(a => a.Action == req.Action.Trim());

        if (req.ActorUserId.HasValue)
            q = q.Where(a => a.ActorUserId == req.ActorUserId.Value);

        var rows = await q
            .OrderByDescending(a => a.CreatedAt)
            .Take(MaxRows)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,EntityType,EntityId,Action,ActorUserId,IpAddress,CreatedAt,HasBefore,HasAfter");
        foreach (var a in rows)
            sb.AppendLine($"{a.Id},{Escape(a.EntityType)},{a.EntityId},{Escape(a.Action)},{a.ActorUserId},{Escape(a.IpAddress)},{a.CreatedAt:yyyy-MM-dd HH:mm:ss},{!string.IsNullOrWhiteSpace(a.BeforeJson)},{!string.IsNullOrWhiteSpace(a.AfterJson)}");

        // UTF-8 BOM để Excel mở được đúng encoding
        var bom  = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var data = bom.Concat(body).ToArray();

        var fileName = $"AuditLog_{req.From:yyyyMMdd}_{req.To:yyyyMMdd}.csv";
        return new GenericExportResult(fileName, "text/csv; charset=utf-8", data, rows.Count);
    }

    // ─────────── UC_SYS_083 — Chính sách hết hạn phiên ───────────

    private const string SessionPolicyKey  = "session.policy";
    private static SessionPolicyDto DefaultSessionPolicy() => new(120, 30, 5, true);

    public async Task<SessionPolicyDto> GetSessionPolicyAsync(Guid tenantId, CancellationToken ct = default)
    {
        var json = await GetSettingRawAsync(tenantId, SessionPolicyKey, ct);
        if (json is null) return DefaultSessionPolicy();
        return JsonSerializer.Deserialize<SessionPolicyDto>(json) ?? DefaultSessionPolicy();
    }

    public async Task<SessionPolicyDto> SetSessionPolicyAsync(
        Guid tenantId, SessionPolicyUpdateRequest req, CancellationToken ct = default)
    {
        // Validate SessionMinutes: 1 phút ≤ x ≤ 10.080 phút (7 ngày)
        if (req.SessionMinutes < 1)
            throw new AppException("Thời gian phiên phải >= 1 phút.", 400);
        if (req.SessionMinutes > 10_080)
            throw new AppException("Thời gian phiên tối đa 10.080 phút (7 ngày).", 400);

        // Validate IdleTimeoutMinutes: 0 = tắt; tối đa bằng SessionMinutes
        if (req.IdleTimeoutMinutes < 0)
            throw new AppException("Thời gian idle timeout không được âm.", 400);
        if (req.IdleTimeoutMinutes > 0 && req.IdleTimeoutMinutes > req.SessionMinutes)
            throw new AppException("Thời gian idle timeout không được lớn hơn thời gian phiên.", 400);

        // Validate MaxConcurrentSessions: 1 – 20
        if (req.MaxConcurrentSessions < 1 || req.MaxConcurrentSessions > 20)
            throw new AppException("Số phiên đồng thời tối đa phải trong khoảng 1 – 20.", 400);

        var policy = new SessionPolicyDto(
            req.SessionMinutes, req.IdleTimeoutMinutes, req.MaxConcurrentSessions, req.ForceLogoutOnPasswordChange);

        await UpsertSettingAsync(tenantId, SessionPolicyKey, JsonSerializer.Serialize(policy), ct);

        // Revoke ngay lập tức các phiên đã vượt quá thời gian hết hạn theo policy mới
        var expiryCutoff = DateTimeOffset.UtcNow.AddMinutes(-req.SessionMinutes);
        var staleSessions = await _db.UserSessions
            .Where(s => s.TenantId == tenantId && !s.IsRevoked && s.LastSeenAt < expiryCutoff)
            .ToListAsync(ct);
        foreach (var s in staleSessions)
            s.IsRevoked = true;
        if (staleSessions.Count > 0)
            await _db.SaveChangesAsync(ct);

        return policy;
    }

    public async Task<int> PurgeExpiredSessionsAsync(Guid tenantId, CancellationToken ct = default)
    {
        // Xóa cứng các session đã revoke hoặc đã hết hạn ExpiresAt
        var now = DateTimeOffset.UtcNow;
        var toDelete = await _db.UserSessions
            .Where(s => s.TenantId == tenantId && (s.IsRevoked || s.ExpiresAt < now))
            .ToListAsync(ct);
        _db.UserSessions.RemoveRange(toDelete);
        if (toDelete.Count > 0)
            await _db.SaveChangesAsync(ct);
        return toDelete.Count;
    }

    // ─────────── UC_SYS_087 — Hàng đợi sự kiện liên module (Outbox Queue) ───────────

    public async Task<OutboxMessageDto> EnqueueOutboxAsync(Guid tenantId, EnqueueOutboxRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.EventType))
            throw new AppException("EventType không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.SourceModule))
            throw new AppException("SourceModule không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.PayloadJson))
            throw new AppException("PayloadJson không được để trống.", 400);

        // Validate JSON format
        try { JsonDocument.Parse(req.PayloadJson); }
        catch { throw new AppException("PayloadJson không phải là JSON hợp lệ.", 400); }

        var msg = new OutboxMessage
        {
            TenantId = tenantId,
            EventType = req.EventType.Trim(),
            SourceModule = req.SourceModule.Trim(),
            CorrelationId = req.CorrelationId ?? Guid.NewGuid(),
            PayloadJson = req.PayloadJson,
            Status = "Pending",
            AttemptCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.OutboxMessages.Add(msg);
        await _db.SaveChangesAsync(ct);

        return new OutboxMessageDto(
            msg.Id, msg.EventType, msg.SourceModule, msg.CorrelationId,
            msg.PayloadJson, msg.Status, msg.AttemptCount,
            msg.NextAttemptAt, msg.PublishedAt, msg.LastError, msg.CreatedAt);
    }

    public async Task<(IReadOnlyList<OutboxMessageDto> Items, int Total)> ListOutboxMessagesAsync(
        Guid tenantId, OutboxQueryRequest query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 50 : query.PageSize > 500 ? 500 : query.PageSize;

        var q = _db.OutboxMessages.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status.Trim());

        if (!string.IsNullOrWhiteSpace(query.SourceModule))
            q = q.Where(x => x.SourceModule == query.SourceModule.Trim());

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(msg => new OutboxMessageDto(
                msg.Id, msg.EventType, msg.SourceModule, msg.CorrelationId,
                msg.PayloadJson, msg.Status, msg.AttemptCount,
                msg.NextAttemptAt, msg.PublishedAt, msg.LastError, msg.CreatedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<OutboxBatchProcessResultDto> ProcessOutboxQueueAsync(
        Guid tenantId, int maxBatch = 50, CancellationToken ct = default)
    {
        if (maxBatch <= 0) maxBatch = 50;
        if (maxBatch > 200) maxBatch = 200;

        var now = DateTimeOffset.UtcNow;
        var pending = await _db.OutboxMessages
            .Where(x => x.TenantId == tenantId && (x.Status == "Pending" || (x.Status == "Failed" && x.NextAttemptAt <= now && x.AttemptCount < 5)))
            .OrderBy(x => x.CreatedAt)
            .Take(maxBatch)
            .ToListAsync(ct);

        int processed = 0, success = 0, failed = 0;

        foreach (var msg in pending)
        {
            processed++;
            msg.AttemptCount++;

            // Mô phỏng xử lý event dispatching
            if (msg.PayloadJson.Contains("simulate_error", StringComparison.OrdinalIgnoreCase))
            {
                msg.Status = msg.AttemptCount >= 5 ? "Dead" : "Failed";
                msg.LastError = $"Simulated dispatch error (Attempt {msg.AttemptCount})";
                msg.NextAttemptAt = now.AddMinutes(Math.Pow(2, msg.AttemptCount));
                failed++;
            }
            else
            {
                msg.Status = "Published";
                msg.PublishedAt = now;
                msg.LastError = null;
                success++;
            }
        }

        if (pending.Count > 0)
            await _db.SaveChangesAsync(ct);

        return new OutboxBatchProcessResultDto(processed, success, failed);
    }

    public async Task RetryOutboxMessageAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var msg = await _db.OutboxMessages.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct)
                  ?? throw new AppException("Thông điệp Outbox không tồn tại.", 404);

        if (msg.Status == "Published")
            throw new AppException("Thông điệp đã được xuất bản (Published) thành công, không thể retry.", 400);

        msg.Status = "Pending";
        msg.AttemptCount = 0;
        msg.NextAttemptAt = DateTimeOffset.UtcNow;
        msg.LastError = null;

        await _db.SaveChangesAsync(ct);
    }

    // ─────────── UC_SYS_088 — Kết nối Email Gateway ───────────

    public async Task<ExternalIntegrationDto> UpsertEmailGatewayAsync(
        Guid tenantId, Guid? actorId, UpsertEmailGatewayRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) throw new AppException("Mã kết nối (Code) không được để trống.", 400);
        if (string.IsNullOrWhiteSpace(req.Name)) throw new AppException("Tên kết nối (Name) không được để trống.", 400);

        var validProviders = new[] { "Smtp", "SendGrid", "AmazonSES" };
        if (!validProviders.Contains(req.Config.ProviderType))
            throw new AppException("Loại nhà cung cấp Email chỉ hỗ trợ Smtp, SendGrid, AmazonSES.", 400);

        if (req.Config.ProviderType == "Smtp")
        {
            if (string.IsNullOrWhiteSpace(req.Config.SmtpHost)) throw new AppException("SmtpHost không được để trống.", 400);
            if (req.Config.SmtpPort <= 0 || req.Config.SmtpPort > 65535) throw new AppException("SmtpPort không hợp lệ.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.Config.SenderEmail))
            throw new AppException("Email người gửi (SenderEmail) không được để trống.", 400);

        var code = req.Code.Trim().ToUpperInvariant();
        var existing = await _db.ExternalIntegrations.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code, ct);

        var configJson = JsonSerializer.Serialize(req.Config);

        if (existing is null)
        {
            existing = new ExternalIntegration
            {
                TenantId = tenantId,
                Code = code,
                Name = req.Name.Trim(),
                Kind = "EmailGateway",
                ConfigJson = configJson,
                IsActive = req.IsActive,
                CreatedBy = actorId
            };
            _db.ExternalIntegrations.Add(existing);
        }
        else
        {
            existing.Name = req.Name.Trim();
            existing.Kind = "EmailGateway";
            existing.ConfigJson = configJson;
            existing.IsActive = req.IsActive;
            existing.UpdatedBy = actorId;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return new ExternalIntegrationDto(existing.Id, existing.Code, existing.Name, existing.Kind, existing.ConfigJson, existing.IsActive);
    }

    public async Task<TestGatewayResultDto> TestEmailGatewayAsync(Guid tenantId, Guid gatewayId, CancellationToken ct = default)
    {
        var gw = await _db.ExternalIntegrations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == gatewayId && x.TenantId == tenantId, ct)
                 ?? throw new AppException("Email Gateway không tồn tại.", 404);

        if (gw.Kind != "EmailGateway")
            throw new AppException("Cấu hình này không phải là Email Gateway.", 400);

        if (!gw.IsActive)
            return new TestGatewayResultDto(false, "Cấu hình Email Gateway đang bị vô hiệu hóa.", 0, DateTimeOffset.UtcNow);

        EmailGatewayConfigDto? cfg;
        try { cfg = JsonSerializer.Deserialize<EmailGatewayConfigDto>(gw.ConfigJson); }
        catch { return new TestGatewayResultDto(false, "Cấu hình JSON không hợp lệ.", 0, DateTimeOffset.UtcNow); }

        if (cfg is null)
            return new TestGatewayResultDto(false, "Cấu hình rỗng.", 0, DateTimeOffset.UtcNow);

        // Simulated connection handshake
        return new TestGatewayResultDto(true, $"Kết nối thành công tới Email Gateway ({cfg.ProviderType} - {cfg.SenderEmail}).", 42, DateTimeOffset.UtcNow);
    }

    public async Task<ChannelSendResultDto> SendTestEmailAsync(Guid tenantId, Guid? actorId, SendTestEmailRequest req, CancellationToken ct = default)
    {
        var gw = await _db.ExternalIntegrations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.GatewayId && x.TenantId == tenantId, ct)
                 ?? throw new AppException("Email Gateway không tồn tại.", 404);

        if (string.IsNullOrWhiteSpace(req.TargetEmail) || !req.TargetEmail.Contains('@'))
            throw new AppException("Email nhận không hợp lệ.", 400);

        var log = new IntegrationCallLog
        {
            TenantId = tenantId,
            Kind = "EmailGatewayTest",
            Target = req.TargetEmail.Trim(),
            StatusCode = 200,
            RequestSummary = $"Test Email via {gw.Code}: {req.Subject}",
            CalledAt = DateTimeOffset.UtcNow
        };
        _db.IntegrationCallLogs.Add(log);

        _db.OutboxMessages.Add(new OutboxMessage
        {
            TenantId = tenantId,
            EventType = "SYS.EMAIL.TEST_SENT",
            SourceModule = "SYS",
            PayloadJson = JsonSerializer.Serialize(new { req.TargetEmail, req.Subject, GatewayCode = gw.Code }),
            Status = "Published",
            PublishedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return new ChannelSendResultDto(
            log.Id, "Email", req.TargetEmail.Trim(), "TEST_EMAIL",
            req.Subject, req.Body, "Success", gw.Id, gw.Code);
    }

    // ─────────── UC_SYS_089 — Kết nối SMS Gateway ───────────

    public async Task<ExternalIntegrationDto> UpsertSmsGatewayAsync(
        Guid tenantId, Guid? actorId, UpsertSmsGatewayRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) throw new AppException("Mã kết nối (Code) không được để trống.", 400);
        if (string.IsNullOrWhiteSpace(req.Name)) throw new AppException("Tên kết nối (Name) không được để trống.", 400);

        var validProviders = new[] { "Twilio", "VietGuys", "eSMS", "SpeedSMS" };
        if (!validProviders.Contains(req.Config.ProviderType))
            throw new AppException("Loại nhà cung cấp SMS chỉ hỗ trợ Twilio, VietGuys, eSMS, SpeedSMS.", 400);

        if (string.IsNullOrWhiteSpace(req.Config.SenderId))
            throw new AppException("SenderId (Brandname) không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.Config.ApiKeyOrSecret))
            throw new AppException("ApiKeyOrSecret không được để trống.", 400);

        var code = req.Code.Trim().ToUpperInvariant();
        var existing = await _db.ExternalIntegrations.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code, ct);

        var configJson = JsonSerializer.Serialize(req.Config);

        if (existing is null)
        {
            existing = new ExternalIntegration
            {
                TenantId = tenantId,
                Code = code,
                Name = req.Name.Trim(),
                Kind = "SmsGateway",
                ConfigJson = configJson,
                IsActive = req.IsActive,
                CreatedBy = actorId
            };
            _db.ExternalIntegrations.Add(existing);
        }
        else
        {
            existing.Name = req.Name.Trim();
            existing.Kind = "SmsGateway";
            existing.ConfigJson = configJson;
            existing.IsActive = req.IsActive;
            existing.UpdatedBy = actorId;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return new ExternalIntegrationDto(existing.Id, existing.Code, existing.Name, existing.Kind, existing.ConfigJson, existing.IsActive);
    }

    public async Task<TestGatewayResultDto> TestSmsGatewayAsync(Guid tenantId, Guid gatewayId, CancellationToken ct = default)
    {
        var gw = await _db.ExternalIntegrations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == gatewayId && x.TenantId == tenantId, ct)
                 ?? throw new AppException("SMS Gateway không tồn tại.", 404);

        if (gw.Kind != "SmsGateway")
            throw new AppException("Cấu hình này không phải là SMS Gateway.", 400);

        if (!gw.IsActive)
            return new TestGatewayResultDto(false, "Cấu hình SMS Gateway đang bị vô hiệu hóa.", 0, DateTimeOffset.UtcNow);

        SmsGatewayConfigDto? cfg;
        try { cfg = JsonSerializer.Deserialize<SmsGatewayConfigDto>(gw.ConfigJson); }
        catch { return new TestGatewayResultDto(false, "Cấu hình JSON không hợp lệ.", 0, DateTimeOffset.UtcNow); }

        if (cfg is null)
            return new TestGatewayResultDto(false, "Cấu hình rỗng.", 0, DateTimeOffset.UtcNow);

        return new TestGatewayResultDto(true, $"Kết nối thành công tới SMS Gateway ({cfg.ProviderType} - Brandname: {cfg.SenderId}).", 35, DateTimeOffset.UtcNow);
    }

    public async Task<ChannelSendResultDto> SendTestSmsAsync(Guid tenantId, Guid? actorId, SendTestSmsRequest req, CancellationToken ct = default)
    {
        var gw = await _db.ExternalIntegrations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.GatewayId && x.TenantId == tenantId, ct)
                 ?? throw new AppException("SMS Gateway không tồn tại.", 404);

        if (string.IsNullOrWhiteSpace(req.TargetPhone) || req.TargetPhone.Length < 9)
            throw new AppException("Số điện thoại nhận không hợp lệ.", 400);

        var log = new IntegrationCallLog
        {
            TenantId = tenantId,
            Kind = "SmsGatewayTest",
            Target = req.TargetPhone.Trim(),
            StatusCode = 200,
            RequestSummary = $"Test SMS via {gw.Code}: {req.Message}",
            CalledAt = DateTimeOffset.UtcNow
        };
        _db.IntegrationCallLogs.Add(log);

        _db.OutboxMessages.Add(new OutboxMessage
        {
            TenantId = tenantId,
            EventType = "SYS.SMS.TEST_SENT",
            SourceModule = "SYS",
            PayloadJson = JsonSerializer.Serialize(new { req.TargetPhone, req.Message, GatewayCode = gw.Code }),
            Status = "Published",
            PublishedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return new ChannelSendResultDto(
            log.Id, "Sms", req.TargetPhone.Trim(), "TEST_SMS",
            "Test SMS", req.Message, "Success", gw.Id, gw.Code);
    }

    // ─────────── UC_SYS_101 — Đính kèm file trong tin nhắn ───────────

    private static readonly string[] ForbiddenExtensions = [".exe", ".bat", ".cmd", ".sh", ".vbs", ".msi"];

    public async Task<ChatMessageAttachmentDto> SendChatMessageAsync(
        Guid tenantId, Guid senderUserId, SendChatMessageRequest req, CancellationToken ct = default)
    {
        if (req.ConversationId == Guid.Empty)
            throw new AppException("ConversationId không được để trống.", 400);

        var conversation = await _db.Conversations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.TenantId == tenantId, ct)
            ?? throw new AppException("Cuộc trò chuyện không tồn tại.", 404);

        FileObject? fileObj = null;

        if (req.AttachmentFileId.HasValue)
        {
            fileObj = await _db.FileObjects.FirstOrDefaultAsync(f => f.Id == req.AttachmentFileId.Value && f.TenantId == tenantId && !f.IsDeleted, ct)
                      ?? throw new AppException("File đính kèm không tồn tại hoặc đã bị xóa.", 400);

            // Edge case 1: Security extension check
            var ext = Path.GetExtension(fileObj.FileName).ToLowerInvariant();
            if (ForbiddenExtensions.Contains(ext))
                throw new AppException($"Loại file đính kèm '{ext}' bị cấm vì lý do bảo mật.", 400);

            // Edge case 2: File size limit (25MB)
            if (fileObj.SizeBytes > 25 * 1024 * 1024)
                throw new AppException("Dung lượng file đính kèm cho tin nhắn vượt quá giới hạn 25MB.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.Body) && fileObj is null)
            throw new AppException("Tin nhắn phải có nội dung văn bản hoặc file đính kèm.", 400);

        var msg = new ChatMessage
        {
            TenantId = tenantId,
            ConversationId = req.ConversationId,
            SenderUserId = senderUserId,
            Body = req.Body?.Trim() ?? "",
            AttachmentFileId = fileObj?.Id,
            AttachmentStorageKey = fileObj?.StorageKey,
            ParentMessageId = req.ParentMessageId,
            SentAt = DateTimeOffset.UtcNow
        };

        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync(ct);

        // Auto link file to ChatMessage entity if attachment exists
        if (fileObj is not null)
        {
            fileObj.LinkedEntityType = "ChatMessage";
            fileObj.LinkedEntityId = msg.Id;
            await _db.SaveChangesAsync(ct);
        }

        return new ChatMessageAttachmentDto(
            msg.Id, msg.ConversationId, msg.SenderUserId, msg.Body,
            fileObj?.Id, fileObj?.FileName, fileObj?.StorageKey, fileObj?.SizeBytes, fileObj?.ContentType,
            msg.ParentMessageId, msg.SentAt, msg.IsEdited, msg.RecalledAt);
    }

    public async Task<IReadOnlyList<ChatMessageAttachmentDto>> ListChatMessagesAsync(
        Guid tenantId, Guid conversationId, int take = 50, CancellationToken ct = default)
    {
        if (take <= 0) take = 50;
        if (take > 200) take = 200;

        var query = from m in _db.ChatMessages.AsNoTracking()
                    where m.TenantId == tenantId && m.ConversationId == conversationId
                    join f in _db.FileObjects.AsNoTracking() on m.AttachmentFileId equals f.Id into fj
                    from f in fj.DefaultIfEmpty()
                    orderby m.SentAt descending
                    select new { Message = m, File = f };

        var list = await query.Take(take).ToListAsync(ct);

        return list.Select(x =>
        {
            var isRecalled = x.Message.RecalledAt.HasValue;
            return new ChatMessageAttachmentDto(
                x.Message.Id, x.Message.ConversationId, x.Message.SenderUserId,
                isRecalled ? "Tin nhắn đã bị thu hồi" : x.Message.Body,
                isRecalled ? null : x.File?.Id,
                isRecalled ? null : x.File?.FileName,
                isRecalled ? null : x.File?.StorageKey,
                isRecalled ? null : x.File?.SizeBytes,
                isRecalled ? null : x.File?.ContentType,
                x.Message.ParentMessageId, x.Message.SentAt, x.Message.IsEdited, x.Message.RecalledAt);
        }).ToList();
    }

    public async Task RecallChatMessageAsync(Guid tenantId, Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var msg = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId && m.TenantId == tenantId, ct)
                  ?? throw new AppException("Tin nhắn không tồn tại.", 404);

        if (msg.SenderUserId != userId)
            throw new AppException("Bạn chỉ có thể thu hồi tin nhắn do chính mình gửi.", 403);

        if (msg.RecalledAt.HasValue)
            throw new AppException("Tin nhắn đã được thu hồi trước đó.", 400);

        msg.RecalledAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ─────────── UC_HRM_001 / 002 / 003 / 004 — Cơ cấu tổ chức & Điểm bán ───────────

    public async Task<OrgUnitDetailDto> UpsertOrgUnitAsync(
        Guid tenantId, Guid? actorId, UpsertOrgUnitRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            throw new AppException("Mã đơn vị tổ chức (Code) không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.Name))
            throw new AppException("Tên đơn vị tổ chức (Name) không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.UnitType))
            throw new AppException("Loại đơn vị tổ chức (UnitType) không được để trống.", 400);

        var code = req.Code.Trim().ToUpperInvariant();

        // Check code uniqueness
        var dupCode = await _db.OrgUnits.AnyAsync(x => x.TenantId == tenantId && x.Code == code && x.Id != (req.Id ?? Guid.Empty) && !x.IsDeleted, ct);
        if (dupCode)
            throw new AppException($"Mã đơn vị tổ chức '{code}' đã tồn tại trong hệ thống.", 400);

        OrgUnit? parent = null;
        if (req.ParentId.HasValue && req.ParentId.Value != Guid.Empty)
        {
            if (req.Id.HasValue && req.ParentId.Value == req.Id.Value)
                throw new AppException("Đơn vị tổ chức không thể chọn chính mình làm đơn vị cha.", 400);

            parent = await _db.OrgUnits.FirstOrDefaultAsync(x => x.Id == req.ParentId.Value && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Đơn vị cha không tồn tại.", 400);

            // Circular reference check
            if (req.Id.HasValue && req.Id.Value != Guid.Empty)
            {
                var cur = parent;
                while (cur is not null && cur.ParentId.HasValue)
                {
                    if (cur.ParentId.Value == req.Id.Value)
                        throw new AppException("Không thể chọn đơn vị con/cháu làm đơn vị cha (Vòng lặp cơ cấu).", 400);

                    cur = await _db.OrgUnits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cur.ParentId.Value && x.TenantId == tenantId && !x.IsDeleted, ct);
                }
            }
        }

        OrgUnit e;
        if (req.Id.HasValue && req.Id.Value != Guid.Empty)
        {
            e = await _db.OrgUnits.FirstOrDefaultAsync(x => x.Id == req.Id.Value && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Đơn vị tổ chức không tồn tại.", 404);
            e.UpdatedBy = actorId;
            e.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            await EnsureMaxOrgUnitsAsync(tenantId, ct);
            e = new OrgUnit { TenantId = tenantId, CreatedBy = actorId };
            _db.OrgUnits.Add(e);
        }

        e.Code = code;
        e.Name = req.Name.Trim();
        e.ParentId = parent?.Id;
        e.UnitType = req.UnitType.Trim();
        e.ManagerUserId = req.ManagerUserId;
        e.SortOrder = req.SortOrder;
        e.IsActive = req.IsActive;

        await _db.SaveChangesAsync(ct);

        // Update Path
        e.Path = parent is null ? $"/{e.Id}" : $"{parent.Path}/{e.Id}";
        await _db.SaveChangesAsync(ct);

        var childCount = await _db.OrgUnits.CountAsync(x => x.TenantId == tenantId && x.ParentId == e.Id && !x.IsDeleted, ct);

        return new OrgUnitDetailDto(
            e.Id, e.Code, e.Name, e.ParentId, parent?.Name, e.UnitType,
            e.Path, e.ManagerUserId, e.SortOrder, e.IsActive, childCount);
    }

    public async Task<IReadOnlyList<OrgUnitDetailDto>> ListOrgUnitsAsync(
        Guid tenantId, string? unitType = null, CancellationToken ct = default)
    {
        var query = _db.OrgUnits.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(unitType))
            query = query.Where(x => x.UnitType == unitType.Trim());

        var list = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code).ToListAsync(ct);

        var allParents = list.Where(x => x.ParentId.HasValue).Select(x => x.ParentId!.Value).Distinct().ToList();
        var parentMap = await _db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && allParents.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var childCounts = await _db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId, x => x.Count, ct);

        return list.Select(e => new OrgUnitDetailDto(
            e.Id, e.Code, e.Name, e.ParentId,
            e.ParentId.HasValue ? parentMap.GetValueOrDefault(e.ParentId.Value) : null,
            e.UnitType, e.Path, e.ManagerUserId, e.SortOrder, e.IsActive,
            childCounts.GetValueOrDefault(e.Id, 0))).ToList();
    }

    public async Task<OrgUnitDetailDto> GetOrgUnitDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.OrgUnits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Đơn vị tổ chức không tồn tại.", 404);

        string? parentName = null;
        if (e.ParentId.HasValue)
        {
            parentName = await _db.OrgUnits.AsNoTracking()
                .Where(x => x.Id == e.ParentId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(ct);
        }

        var childCount = await _db.OrgUnits.CountAsync(x => x.TenantId == tenantId && x.ParentId == e.Id && !x.IsDeleted, ct);

        return new OrgUnitDetailDto(
            e.Id, e.Code, e.Name, e.ParentId, parentName, e.UnitType,
            e.Path, e.ManagerUserId, e.SortOrder, e.IsActive, childCount);
    }

    public async Task DeleteOrgUnitAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.OrgUnits.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Đơn vị tổ chức không tồn tại.", 404);

        var hasChildren = await _db.OrgUnits.AnyAsync(x => x.TenantId == tenantId && x.ParentId == id && !x.IsDeleted, ct);
        if (hasChildren)
            throw new AppException("Không thể xóa đơn vị tổ chức đang có đơn vị con trực thuộc.", 400);

        var hasEmployees = await _db.Employees.AnyAsync(x => x.TenantId == tenantId && x.OrgUnitId == id && !x.IsDeleted, ct);
        if (hasEmployees)
            throw new AppException("Không thể xóa đơn vị tổ chức đang có nhân sự thuộc về.", 400);

        e.IsDeleted = true;
        e.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteSalesPointAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.SalesPoints.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Điểm bán không tồn tại.", 404);

        e.IsDeleted = true;
        e.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ─────────── UC_HRM_010 — Quản lý cấp bậc (JobLevel) ───────────

    public async Task<IReadOnlyList<JobLevelPolishDto>> ListJobLevelsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.JobLevels.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.LevelOrder)
            .ThenBy(x => x.Code)
            .Select(x => new JobLevelPolishDto(x.Id, x.Code, x.Name, x.LevelOrder, x.DefaultScopeType.ToString(), x.Description, x.IsActive))
            .ToListAsync(ct);
        return list;
    }

    public async Task<JobLevelPolishDto> UpsertJobLevelAsync(Guid tenantId, Guid? actorId, UpsertJobLevelRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            throw new AppException("Mã cấp bậc (Code) không được để trống.", 400);

        if (string.IsNullOrWhiteSpace(req.Name))
            throw new AppException("Tên cấp bậc (Name) không được để trống.", 400);

        if (req.LevelOrder < 0)
            throw new AppException("Thứ tự cấp bậc (LevelOrder) không được nhỏ hơn 0.", 400);

        if (!Enum.TryParse<ScopeType>(req.DefaultScopeType, true, out var scopeType))
            throw new AppException($"DefaultScopeType '{req.DefaultScopeType}' không hợp lệ.", 400);

        var code = req.Code.Trim().ToUpperInvariant();
        var dupCode = await _db.JobLevels.AnyAsync(x => x.TenantId == tenantId && x.Code == code && x.Id != (req.Id ?? Guid.Empty) && !x.IsDeleted, ct);
        if (dupCode)
            throw new AppException($"Mã cấp bậc '{code}' đã tồn tại trong hệ thống.", 400);

        JobLevel e;
        if (req.Id.HasValue && req.Id.Value != Guid.Empty)
        {
            e = await _db.JobLevels.FirstOrDefaultAsync(x => x.Id == req.Id.Value && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Cấp bậc không tồn tại.", 404);
            e.UpdatedBy = actorId;
            e.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            e = new JobLevel { TenantId = tenantId, CreatedBy = actorId };
            _db.JobLevels.Add(e);
        }

        e.Code = code;
        e.Name = req.Name.Trim();
        e.LevelOrder = req.LevelOrder;
        e.DefaultScopeType = scopeType;
        e.Description = req.Description?.Trim();
        e.IsActive = req.IsActive;

        await _db.SaveChangesAsync(ct);
        return new JobLevelPolishDto(e.Id, e.Code, e.Name, e.LevelOrder, e.DefaultScopeType.ToString(), e.Description, e.IsActive);
    }


    public async Task DeleteJobLevelAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.JobLevels.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Cấp bậc không tồn tại.", 404);

        var inUseInEmployees = await _db.Employees.AnyAsync(x => x.TenantId == tenantId && x.JobLevelId == id && !x.IsDeleted, ct);
        if (inUseInEmployees)
            throw new AppException("Không thể xóa cấp bậc đang được gán cho nhân sự.", 400);

        e.IsDeleted = true;
        e.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ─────────── UC_HRM_012 — Sinh mã nhân sự tự động ───────────

    public async Task<EmployeeCodeGeneratedDto> GenerateNextEmployeeCodeAsync(
        Guid tenantId, EmployeeCodeGenerateRequest req, CancellationToken ct = default)
    {
        var docType = string.IsNullOrWhiteSpace(req.DocType) ? "EMP" : req.DocType.Trim().ToUpperInvariant();
        var pattern = string.IsNullOrWhiteSpace(req.Pattern) ? "EMP-{SEQ:4}" : req.Pattern.Trim();

        var seq = await _db.NumberSequences.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.DocType == docType, ct);
        if (seq is null)
        {
            seq = new NumberSequence { TenantId = tenantId, DocType = docType, Pattern = pattern, NextValue = 1 };
            _db.NumberSequences.Add(seq);
        }

        string generatedCode;
        int maxAttempts = 1000;
        int attempts = 0;

        do
        {
            attempts++;
            var nextVal = seq.NextValue;
            seq.NextValue++;

            // Replace pattern {SEQ:n} or {YYYY} or {YY}
            var year = DateTime.UtcNow.Year.ToString();
            var shortYear = DateTime.UtcNow.ToString("yy");
            generatedCode = pattern.Replace("{YYYY}", year).Replace("{YY}", shortYear);

            if (generatedCode.Contains("{SEQ:"))
            {
                var startIndex = generatedCode.IndexOf("{SEQ:");
                var endIndex = generatedCode.IndexOf("}", startIndex);
                if (endIndex > startIndex)
                {
                    var lenStr = generatedCode.Substring(startIndex + 5, endIndex - startIndex - 5);
                    if (int.TryParse(lenStr, out var digits) && digits > 0)
                    {
                        var seqStr = nextVal.ToString().PadLeft(digits, '0');
                        generatedCode = generatedCode.Substring(0, startIndex) + seqStr + generatedCode.Substring(endIndex + 1);
                    }
                }
            }
            else if (generatedCode.Contains("{SEQ}"))
            {
                generatedCode = generatedCode.Replace("{SEQ}", nextVal.ToString());
            }

            var isDuplicate = await _db.Employees.AnyAsync(x => x.TenantId == tenantId && x.EmployeeCode == generatedCode && !x.IsDeleted, ct);
            if (!isDuplicate) break;

        } while (attempts < maxAttempts);

        await _db.SaveChangesAsync(ct);
        return new EmployeeCodeGeneratedDto(generatedCode, seq.NextValue - 1, pattern);
    }
}
