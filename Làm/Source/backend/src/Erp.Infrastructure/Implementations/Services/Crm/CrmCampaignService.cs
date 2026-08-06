using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Domain.Base;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Crm;

/// <summary>Campaign marketing — UC_CRM_016, 019, 023, 026, 029, 031.</summary>
public sealed class CrmCampaignService : ICrmCampaignService
{
    private static readonly HashSet<string> Channels =
        new(StringComparer.OrdinalIgnoreCase) { "Email", "Social", "SEM", "Event", "Other" };
    private static readonly HashSet<string> ExpenseTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Ads", "Media", "Event", "Agency", "Other" };

    private readonly AppDbContext _db;
    public CrmCampaignService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CrmCampaignDto>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmCampaigns.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<CrmCampaignDto> GetAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => Map(await RequireAsync(_db.CrmCampaigns, tenantId, id, "campaign", ct));

    public async Task<CrmCampaignDto> UpsertAsync(
        Guid tenantId, Guid userId, CrmCampaignUpsertRequest req, CancellationToken ct = default)
    {
        var name = Req(req.Name, 200, "Tên campaign");
        var channel = (req.Channel ?? "").Trim();
        if (!Channels.Contains(channel))
            throw new AppException("Channel: Email|Social|SEM|Event|Other.");
        if (req.BudgetAmount < 0) throw new AppException("Ngân sách không được âm.");
        if (req.StartDate is DateTimeOffset s && req.EndDate is DateTimeOffset e && e < s)
            throw new AppException("Ngày kết thúc phải ≥ ngày bắt đầu.");
        if (req.OwnerUserId is Guid oid)
            await EnsureUserAsync(tenantId, oid, ct);

        CrmCampaign entity;
        if (req.Id is Guid id)
        {
            entity = await RequireAsync(_db.CrmCampaigns, tenantId, id, "campaign", ct);
            if (entity.Status == "Closed")
                throw new AppException("Campaign đã đóng — không sửa.");
        }
        else
        {
            var code = string.IsNullOrWhiteSpace(req.Code)
                ? await NextCodeAsync(tenantId, "CAMP", _db.CrmCampaigns, ct)
                : NormCode(req.Code);
            if (await _db.CrmCampaigns.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã campaign đã tồn tại.");
            entity = new CrmCampaign
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Code = code,
                Status = "Draft",
            };
            _db.CrmCampaigns.Add(entity);
        }

        entity.Name = name;
        entity.Description = NullIfEmpty(req.Description);
        entity.Channel = Channels.First(x => x.Equals(channel, StringComparison.OrdinalIgnoreCase));
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        entity.BudgetAmount = req.BudgetAmount;
        entity.OwnerUserId = req.OwnerUserId;
        if (entity.Status == "Draft" && req.StartDate is not null)
            entity.Status = "Active";
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<CrmCampaignDto> CloseAsync(
        Guid tenantId, Guid userId, Guid campaignId, CrmCampaignCloseRequest req, CancellationToken ct = default)
    {
        var entity = await RequireAsync(_db.CrmCampaigns, tenantId, campaignId, "campaign", ct);
        if (entity.Status == "Closed") throw new AppException("Campaign đã đóng.");
        entity.Status = "Closed";
        entity.ClosedAt = DateTimeOffset.UtcNow;
        entity.ClosedReason = NullIfEmpty(req.Reason) ?? "Đóng thủ công";
        entity.UpdatedBy = userId;
        // Snapshot spent từ expenses
        entity.SpentAmount = await _db.CrmCampaignExpenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CampaignId == campaignId && !x.IsDeleted)
            .SumAsync(x => x.Amount, ct);
        entity.LeadCount = await _db.CrmWebLeads.CountAsync(
            x => x.TenantId == tenantId && x.CampaignId == campaignId && !x.IsDeleted && x.SyncStatus == "Synced", ct);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<IReadOnlyList<CrmCampaignExpenseDto>> ListExpensesAsync(
        Guid tenantId, Guid campaignId, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.CrmCampaigns, tenantId, campaignId, "campaign", ct);
        var list = await _db.CrmCampaignExpenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CampaignId == campaignId && !x.IsDeleted)
            .OrderByDescending(x => x.ExpenseDate).Take(200).ToListAsync(ct);
        return list.Select(MapExpense).ToList();
    }

    public async Task<CrmCampaignExpenseDto> UpsertExpenseAsync(
        Guid tenantId, Guid userId, Guid campaignId, CrmCampaignExpenseUpsertRequest req, CancellationToken ct = default)
    {
        var camp = await RequireAsync(_db.CrmCampaigns, tenantId, campaignId, "campaign", ct);
        if (camp.Status == "Closed") throw new AppException("Campaign đã đóng — không ghi chi phí.");
        var type = (req.ExpenseType ?? "").Trim();
        if (!ExpenseTypes.Contains(type)) throw new AppException("ExpenseType: Ads|Media|Event|Agency|Other.");
        if (req.Amount <= 0) throw new AppException("Số tiền chi phí phải > 0.");

        CrmCampaignExpense entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmCampaignExpenses, tenantId, id, "chi phí", ct);
        else
        {
            entity = new CrmCampaignExpense { TenantId = tenantId, CreatedBy = userId, CampaignId = campaignId };
            _db.CrmCampaignExpenses.Add(entity);
        }

        entity.ExpenseType = ExpenseTypes.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
        entity.Description = NullIfEmpty(req.Description);
        entity.Amount = req.Amount;
        entity.ExpenseDate = req.ExpenseDate ?? DateTimeOffset.UtcNow;
        entity.InvoiceRef = NullIfEmpty(req.InvoiceRef);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        camp.SpentAmount = await _db.CrmCampaignExpenses
            .Where(x => x.TenantId == tenantId && x.CampaignId == campaignId && !x.IsDeleted)
            .SumAsync(x => x.Amount, ct);
        camp.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapExpense(entity);
    }

    public async Task<CrmWebLeadDto> SyncWebLeadAsync(
        Guid tenantId, CrmWebLeadSyncRequest req, CancellationToken ct = default)
    {
        var name = Req(req.ContactName, 200, "Tên liên hệ");
        if (string.IsNullOrWhiteSpace(req.Phone) && string.IsNullOrWhiteSpace(req.Email))
            throw new AppException("Cần ít nhất SĐT hoặc Email.");

        if (req.CampaignId is Guid cid)
            _ = await RequireAsync(_db.CrmCampaigns, tenantId, cid, "campaign", ct);

        // Nguồn Website mặc định
        var source = await _db.CrmLeadSources
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == "WEB" && !x.IsDeleted, ct);
        if (source is null)
        {
            source = new CrmLeadSource
            {
                TenantId = tenantId,
                Code = "WEB",
                Name = "Website / Landing",
                ChannelType = "Website",
                Status = "Active",
            };
            _db.CrmLeadSources.Add(source);
            await _db.SaveChangesAsync(ct);
        }

        var leadCode = await NextLeadCodeAsync(tenantId, ct);
        var lead = new CrmLead
        {
            TenantId = tenantId,
            Code = leadCode,
            Name = name,
            Phone = NullIfEmpty(req.Phone),
            Email = NullIfEmpty(req.Email),
            SourceId = source.Id,
            PipelineStatus = "New",
            IntakeChannel = "Auto",
            Note = NullIfEmpty(req.LandingPage) is { } lp ? $"Landing: {lp}" : null,
        };
        _db.CrmLeads.Add(lead);

        var web = new CrmWebLead
        {
            TenantId = tenantId,
            ContactName = name,
            Phone = NullIfEmpty(req.Phone),
            Email = NullIfEmpty(req.Email),
            SourceUrl = NullIfEmpty(req.SourceUrl),
            LandingPage = NullIfEmpty(req.LandingPage),
            UtmSource = NullIfEmpty(req.UtmSource),
            UtmMedium = NullIfEmpty(req.UtmMedium),
            UtmCampaign = NullIfEmpty(req.UtmCampaign),
            SyncStatus = "Synced",
            LeadId = lead.Id,
            CampaignId = req.CampaignId,
        };
        _db.CrmWebLeads.Add(web);
        await _db.SaveChangesAsync(ct);

        if (req.CampaignId is Guid campId)
        {
            var camp = await _db.CrmCampaigns.FirstAsync(x => x.Id == campId && x.TenantId == tenantId, ct);
            camp.LeadCount = await _db.CrmWebLeads.CountAsync(
                x => x.TenantId == tenantId && x.CampaignId == campId && !x.IsDeleted && x.SyncStatus == "Synced", ct);
            await _db.SaveChangesAsync(ct);
        }

        return MapWeb(web);
    }

    public async Task<IReadOnlyList<CrmWebLeadDto>> ListWebLeadsAsync(
        Guid tenantId, string? syncStatus, CancellationToken ct = default)
    {
        var q = _db.CrmWebLeads.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(syncStatus))
        {
            var s = syncStatus.Trim();
            q = q.Where(x => x.SyncStatus == s);
        }
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return list.Select(MapWeb).ToList();
    }

    public async Task<CrmMarketingMetricsDto> GetMetricsAsync(
        Guid tenantId, Guid campaignId, CancellationToken ct = default)
    {
        var camp = await RequireAsync(_db.CrmCampaigns, tenantId, campaignId, "campaign", ct);
        return await BuildMetricsAsync(tenantId, camp, ct);
    }

    public async Task<CrmMarketingDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken ct = default)
    {
        var camps = await _db.CrmCampaigns.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var metrics = new List<CrmMarketingMetricsDto>();
        foreach (var c in camps)
            metrics.Add(await BuildMetricsAsync(tenantId, c, ct));

        var totalSpent = metrics.Sum(m => m.TotalSpent);
        var totalRev = metrics.Sum(m => m.Revenue);
        var roi = totalSpent > 0 ? Math.Round((totalRev - totalSpent) / totalSpent * 100m, 2) : 0m;
        return new CrmMarketingDashboardDto(
            camps.Count,
            camps.Count(c => c.Status is "Active" or "Paused"),
            camps.Sum(c => c.BudgetAmount),
            totalSpent,
            totalRev,
            roi,
            metrics);
    }

    private async Task<CrmMarketingMetricsDto> BuildMetricsAsync(
        Guid tenantId, CrmCampaign camp, CancellationToken ct)
    {
        var spent = await _db.CrmCampaignExpenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CampaignId == camp.Id && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.Amount, ct) ?? 0m;
        var leads = await _db.CrmWebLeads.CountAsync(
            x => x.TenantId == tenantId && x.CampaignId == camp.Id && !x.IsDeleted && x.SyncStatus == "Synced", ct);
        var revenue = camp.RevenueGenerated;
        var cpl = leads > 0 ? Math.Round(spent / leads, 2) : 0m;
        var cac = cpl; // Cap-2: CAC ≈ CPL khi chưa tách khách mua
        var roas = spent > 0 ? Math.Round(revenue / spent, 4) : 0m;
        var roi = spent > 0 ? Math.Round((revenue - spent) / spent * 100m, 2) : 0m;
        return new CrmMarketingMetricsDto(
            camp.Id, camp.Name, leads, spent, revenue, cpl, cac, roas, roi);
    }

    private static CrmCampaignDto Map(CrmCampaign x) => new(
        x.Id, x.Code, x.Name, x.Description, x.Channel, x.Status,
        x.StartDate, x.EndDate, x.BudgetAmount, x.SpentAmount,
        x.OwnerUserId, x.LeadCount, x.RevenueGenerated, x.ClosedAt, x.ClosedReason);

    private static CrmCampaignExpenseDto MapExpense(CrmCampaignExpense x) => new(
        x.Id, x.CampaignId, x.ExpenseType, x.Description, x.Amount, x.ExpenseDate, x.InvoiceRef);

    private static CrmWebLeadDto MapWeb(CrmWebLead x) => new(
        x.Id, x.SourceUrl, x.LandingPage, x.UtmSource, x.UtmMedium, x.UtmCampaign,
        x.ContactName, x.Phone, x.Email, x.SyncStatus, x.LeadId, x.CampaignId);

    private async Task EnsureUserAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var ok = await _db.Users.AnyAsync(x => x.Id == userId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Người dùng không hợp lệ.");
    }

    private async Task<string> NextLeadCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var p = $"LD-{DateTime.UtcNow:yyyyMM}-";
        var last = await _db.CrmLeads.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code.StartsWith(p))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(p.Length), out var parsed)) n = parsed + 1;
        return $"{p}{n:D4}";
    }

    private static async Task<string> NextCodeAsync(
        Guid tenantId, string prefix, DbSet<CrmCampaign> set, CancellationToken ct)
    {
        var p = $"{prefix}-{DateTime.UtcNow:yyyyMM}-";
        var last = await set.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code.StartsWith(p))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(p.Length), out var parsed)) n = parsed + 1;
        return $"{p}{n:D4}";
    }

    private static async Task<T> RequireAsync<T>(DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? value, int max, string label)
    {
        var v = (value ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string? NullIfEmpty(string? s)
    {
        var v = s?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }
}
