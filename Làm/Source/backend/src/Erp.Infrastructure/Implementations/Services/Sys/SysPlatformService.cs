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
        NotificationRule e;
        if (req.Id != Guid.Empty && await _db.NotificationRules.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.NotificationRules.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else
        {
            e = new NotificationRule { TenantId = tenantId, CreatedBy = actorId };
            _db.NotificationRules.Add(e);
        }
        e.EventType = req.EventType.Trim();
        e.TitleTemplate = req.TitleTemplate;
        e.BodyTemplate = req.BodyTemplate;
        e.IsEnabled = req.IsEnabled;
        await _db.SaveChangesAsync(ct);
        return new NotificationRuleDto(e.Id, e.EventType, e.TitleTemplate, e.BodyTemplate, e.IsEnabled);
    }

    public async Task NotifyEventAsync(Guid tenantId, Guid targetUserId, string eventType, string? link, IDictionary<string, string>? vars, CancellationToken ct = default)
    {
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
        WorkCalendar e;
        if (req.Id != Guid.Empty && await _db.WorkCalendars.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.WorkCalendars.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new WorkCalendar { TenantId = tenantId, CreatedBy = actorId }; _db.WorkCalendars.Add(e); }
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.WeekMask = req.WeekMask; e.HolidaysJson = req.HolidaysJson; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new WorkCalendarDto(e.Id, e.Code, e.Name, e.WeekMask, e.HolidaysJson, e.IsActive);
    }

    public async Task<IReadOnlyList<MessageTemplateDto>> ListMessageTemplatesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.MessageTemplates.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new MessageTemplateDto(x.Id, x.Code, x.Channel, x.Subject, x.Body, x.IsActive)).ToListAsync(ct);

    public async Task<MessageTemplateDto> UpsertMessageTemplateAsync(Guid tenantId, Guid? actorId, MessageTemplateDto req, CancellationToken ct = default)
    {
        MessageTemplate e;
        if (req.Id != Guid.Empty && await _db.MessageTemplates.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.MessageTemplates.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new MessageTemplate { TenantId = tenantId, CreatedBy = actorId }; _db.MessageTemplates.Add(e); }
        e.Code = req.Code.Trim(); e.Channel = req.Channel; e.Subject = req.Subject; e.Body = req.Body; e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new MessageTemplateDto(e.Id, e.Code, e.Channel, e.Subject, e.Body, e.IsActive);
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

    public async Task<ChannelSendResultDto> SendChannelMessageAsync(
        Guid tenantId, Guid? actorId, ChannelSendRequest req, CancellationToken ct = default)
    {
        var channel = (req.Channel ?? "").Trim();
        if (!channel.Equals("Email", StringComparison.OrdinalIgnoreCase)
            && !channel.Equals("SMS", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Channel phải là Email hoặc SMS.");
        channel = channel.Equals("SMS", StringComparison.OrdinalIgnoreCase) ? "SMS" : "Email";

        var templateCode = (req.TemplateCode ?? "").Trim();
        if (string.IsNullOrEmpty(templateCode)) throw new AppException("TemplateCode bắt buộc.");
        var target = (req.Target ?? "").Trim();
        if (string.IsNullOrEmpty(target)) throw new AppException("Target (email/SĐT) bắt buộc.");

        var tpl = await EnsureMessageTemplateAsync(tenantId, actorId, templateCode, channel, ct);
        var integ = await EnsureChannelIntegrationAsync(tenantId, actorId, channel, ct);

        var subject = RenderTemplate(tpl.Subject, req.Vars);
        var body = RenderTemplate(tpl.Body, req.Vars);
        var eventType = string.IsNullOrWhiteSpace(req.EventType) ? $"sys.message.{channel.ToLowerInvariant()}" : req.EventType.Trim();

        var log = new IntegrationCallLog
        {
            TenantId = tenantId,
            Kind = channel,
            Target = target,
            StatusCode = 200,
            RequestSummary = $"[{templateCode}] {subject}".Trim(),
            ResponseSummary = body.Length > 2000 ? body[..2000] : body,
            CalledAt = DateTimeOffset.UtcNow,
            CreatedBy = actorId,
        };
        _db.IntegrationCallLogs.Add(log);

        await _outbox.EnqueueAsync(tenantId, eventType, "SYS", new
        {
            channel,
            templateCode,
            target,
            subject,
            bodyPreview = body.Length > 240 ? body[..240] : body,
            integrationCode = integ.Code,
        }, ct: ct);

        await _db.SaveChangesAsync(ct);
        return new ChannelSendResultDto(
            log.Id, channel, target, templateCode, subject, body, "Logged", integ.Id, integ.Code);
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
}
