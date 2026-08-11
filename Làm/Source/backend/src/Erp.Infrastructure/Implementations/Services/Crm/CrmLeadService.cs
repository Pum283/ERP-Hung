using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Domain.Base;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Crm;

public sealed class CrmLeadService : ICrmLeadService
{
    private static readonly HashSet<string> LeadStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "New", "Contacted", "Qualified", "Converted", "Lost" };
    private static readonly HashSet<string> OppStages =
        new(StringComparer.OrdinalIgnoreCase)
            { "Qualification", "Proposal", "Negotiation", "Won", "Lost" };
    private static readonly HashSet<string> ChannelTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Manual", "Website", "Social", "Other" };
    private static readonly HashSet<string> ActivityTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Call", "Email", "Meeting", "Note", "Other" };

    private readonly AppDbContext _db;
    private readonly ICrmSalesService _sales;
    public CrmLeadService(AppDbContext db, ICrmSalesService sales)
    {
        _db = db;
        _sales = sales;
    }

    public async Task<IReadOnlyList<CrmLeadSourceDto>> ListSourcesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmLeadSources.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        var counts = await _db.CrmLeads.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.SourceId != null)
            .GroupBy(x => x.SourceId!.Value).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(s => new CrmLeadSourceDto(
            s.Id, s.Code, s.Name, s.ChannelType, s.Status, s.Note, counts.GetValueOrDefault(s.Id))).ToList();
    }

    public async Task<CrmLeadSourceDto> UpsertSourceAsync(
        Guid tenantId, Guid userId, CrmLeadSourceUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên nguồn");
        var ch = (req.ChannelType ?? "").Trim();
        if (!ChannelTypes.Contains(ch)) throw new AppException("ChannelType: Manual|Website|Social|Other.");
        CrmLeadSource entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmLeadSources, tenantId, id, "nguồn lead", ct);
        else
        {
            if (await _db.CrmLeadSources.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã nguồn đã tồn tại.");
            entity = new CrmLeadSource { TenantId = tenantId, CreatedBy = userId };
            _db.CrmLeadSources.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.ChannelType = ChannelTypes.First(x => x.Equals(ch, StringComparison.OrdinalIgnoreCase));
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var count = await _db.CrmLeads.CountAsync(x => x.TenantId == tenantId && x.SourceId == entity.Id && !x.IsDeleted, ct);
        return new CrmLeadSourceDto(entity.Id, entity.Code, entity.Name, entity.ChannelType, entity.Status, entity.Note, count);
    }

    public async Task<IReadOnlyList<CrmLeadDto>> ListLeadsAsync(
        Guid tenantId, string? q, string? status, Guid? ownerUserId, CancellationToken ct = default)
    {
        var query = _db.CrmLeads.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.MergedIntoId == null);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term)
                || (x.Phone != null && x.Phone.Contains(term)) || (x.Email != null && x.Email.Contains(term))
                || (x.CompanyName != null && x.CompanyName.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = status.Trim();
            query = query.Where(x => x.PipelineStatus == s);
        }
        if (ownerUserId is Guid oid) query = query.Where(x => x.OwnerUserId == oid);
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return await MapLeadsAsync(tenantId, list, ct);
    }

    public async Task<CrmLeadDetailDto> GetLeadDetailAsync(Guid tenantId, Guid leadId, CancellationToken ct = default)
    {
        var lead = await _db.CrmLeads.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == leadId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy lead.", 404);
        var dto = (await MapLeadsAsync(tenantId, [lead], ct))[0];
        var tasks = await LoadTasksAsync(tenantId, leadId, ct);
        var acts = await LoadActivitiesAsync(tenantId, leadId, ct);
        return new CrmLeadDetailDto(dto, tasks, acts);
    }

    public async Task<CrmLeadDto> UpsertLeadAsync(
        Guid tenantId, Guid userId, CrmLeadUpsertRequest req, CancellationToken ct = default)
    {
        var name = Req(req.Name, 200, "Tên lead");
        if (NullIfEmpty(req.Phone) == null && NullIfEmpty(req.Email) == null)
            throw new AppException("Cần ít nhất SĐT hoặc Email để liên hệ.");

        if (req.SourceId is Guid sid)
            _ = await RequireAsync(_db.CrmLeadSources, tenantId, sid, "nguồn lead", ct);
        if (req.CustomerId is Guid cid)
            _ = await RequireAsync(_db.CrmCustomers, tenantId, cid, "khách hàng", ct);
        if (req.OwnerUserId is Guid oid)
            await EnsureUserAsync(tenantId, oid, ct);

        var status = string.IsNullOrWhiteSpace(req.PipelineStatus) ? "New" : req.PipelineStatus.Trim();
        if (!LeadStatuses.Contains(status)) throw new AppException("Pipeline: New|Contacted|Qualified|Converted|Lost.");
        var intake = string.IsNullOrWhiteSpace(req.IntakeChannel) ? "Manual" : req.IntakeChannel.Trim();
        if (intake is not ("Manual" or "Auto")) throw new AppException("IntakeChannel: Manual|Auto.");

        CrmLead entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmLeads, tenantId, id, "lead", ct);
        else
        {
            entity = new CrmLead
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "LD", _db.CrmLeads, ct) : NormCode(req.Code),
                CreatedBy = userId
            };
            if (await _db.CrmLeads.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã lead đã tồn tại.");
            _db.CrmLeads.Add(entity);
        }

        entity.Name = name;
        entity.Phone = NullIfEmpty(req.Phone);
        entity.Email = NullIfEmpty(req.Email)?.ToLowerInvariant();
        entity.CompanyName = NullIfEmpty(req.CompanyName);
        entity.SourceId = req.SourceId;
        entity.OwnerUserId = req.OwnerUserId;
        entity.CustomerId = req.CustomerId;
        entity.PipelineStatus = LeadStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
        entity.Score = Math.Clamp(req.Score ?? entity.Score, 0, 100);
        entity.NextFollowUpAt = req.NextFollowUpAt;
        entity.Note = NullIfEmpty(req.Note);
        entity.IntakeChannel = intake;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapLeadsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<CrmLeadDto> AutoIntakeAsync(
        Guid tenantId, Guid userId, CrmLeadAutoIntakeRequest req, CancellationToken ct = default)
    {
        var name = (req.Name ?? "").Trim();
        if (name.Length is < 1 or > 200) throw new AppException("Tên lead 1–200 ký tự.");
        var phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        var email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim().ToLowerInvariant();
        if (phone is null && email is null)
            throw new AppException("Auto-intake cần SĐT hoặc Email.");

        // UC_CRM_050: dedup theo SĐT/Email — lead mở (chưa Converted/Lost) thì ghi nhận lại, không tạo trùng.
        CrmLead? existing = null;
        if (phone is not null)
        {
            existing = await _db.CrmLeads.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.Phone == phone
                     && x.PipelineStatus != "Converted" && x.PipelineStatus != "Lost", ct);
        }
        if (existing is null && email is not null)
        {
            existing = await _db.CrmLeads.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.Email == email
                     && x.PipelineStatus != "Converted" && x.PipelineStatus != "Lost", ct);
        }
        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(req.Note))
                existing.Note = string.IsNullOrWhiteSpace(existing.Note)
                    ? req.Note.Trim()
                    : $"{existing.Note}\n{req.Note.Trim()}";
            if (req.OwnerUserId is Guid oid && existing.OwnerUserId is null)
            {
                await EnsureUserAsync(tenantId, oid, ct);
                existing.OwnerUserId = oid;
            }
            existing.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
            await AddActivityInternal(tenantId, userId, existing.Id, "Note",
                $"Re-intake website: {name}" + (phone is not null ? $" · {phone}" : "") + (email is not null ? $" · {email}" : ""),
                ct);
            return (await MapLeadsAsync(tenantId, [existing], ct))[0];
        }

        Guid? sourceId = null;
        var sourceCode = string.IsNullOrWhiteSpace(req.SourceCode) ? "WEBSITE" : NormCode(req.SourceCode);
        {
            var src = await _db.CrmLeadSources
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == sourceCode && !x.IsDeleted, ct);
            if (src is null)
            {
                src = new CrmLeadSource
                {
                    TenantId = tenantId, Code = sourceCode, Name = sourceCode,
                    ChannelType = "Website", Status = "Active", CreatedBy = userId
                };
                _db.CrmLeadSources.Add(src);
                await _db.SaveChangesAsync(ct);
            }
            sourceId = src.Id;
        }

        Guid? ownerId = req.OwnerUserId;
        if (ownerId is null)
        {
            // Round-robin nhẹ: sales đang phụ trách ít lead New/Contacted nhất.
            ownerId = await _db.CrmLeads.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.OwnerUserId != null
                            && (x.PipelineStatus == "New" || x.PipelineStatus == "Contacted"))
                .GroupBy(x => x.OwnerUserId!.Value)
                .Select(g => new { OwnerId = g.Key, C = g.Count() })
                .OrderBy(x => x.C)
                .Select(x => (Guid?)x.OwnerId)
                .FirstOrDefaultAsync(ct);
        }

        var lead = await UpsertLeadAsync(tenantId, userId, new CrmLeadUpsertRequest(
            null, null, name, phone, email, req.CompanyName,
            sourceId, ownerId, null, "New", 10, null, req.Note, "Auto"), ct);

        await AddActivityInternal(tenantId, userId, lead.Id, "Note",
            $"Auto-intake website · nguồn {sourceCode}" + (ownerId is Guid o ? $" · owner {o:N}" : " · chưa phân bổ"),
            ct);
        return lead;
    }

    public async Task<CrmLeadDto> AssignAsync(
        Guid tenantId, Guid userId, Guid leadId, CrmLeadAssignRequest req, CancellationToken ct = default)
    {
        var lead = await RequireAsync(_db.CrmLeads, tenantId, leadId, "lead", ct);
        if (lead.PipelineStatus is "Converted" or "Lost")
            throw new AppException("Không phân bổ lead đã Converted/Lost.");
        await EnsureUserAsync(tenantId, req.OwnerUserId, ct);
        lead.OwnerUserId = req.OwnerUserId;
        lead.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await AddActivityInternal(tenantId, userId, leadId, "Note", $"Phân bổ sales {req.OwnerUserId:N}", ct);
        return (await MapLeadsAsync(tenantId, [lead], ct))[0];
    }

    public async Task<CrmLeadDto> SetStatusAsync(
        Guid tenantId, Guid userId, Guid leadId, CrmLeadStatusRequest req, CancellationToken ct = default)
    {
        var lead = await RequireAsync(_db.CrmLeads, tenantId, leadId, "lead", ct);
        var status = (req.PipelineStatus ?? "").Trim();
        if (!LeadStatuses.Contains(status)) throw new AppException("Pipeline: New|Contacted|Qualified|Converted|Lost.");
        if (status.Equals("Converted", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Dùng API convert để chuyển thành cơ hội.");
        if (status.Equals("Lost", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Dùng API mark-lost.");
        lead.PipelineStatus = LeadStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(req.Note)) lead.Note = req.Note.Trim();
        lead.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await AddActivityInternal(tenantId, userId, leadId, "Note", $"Cập nhật pipeline → {lead.PipelineStatus}", ct);
        return (await MapLeadsAsync(tenantId, [lead], ct))[0];
    }

    public async Task<CrmLeadDto> MarkLostAsync(
        Guid tenantId, Guid userId, Guid leadId, CrmLeadLostRequest req, CancellationToken ct = default)
    {
        var lead = await RequireAsync(_db.CrmLeads, tenantId, leadId, "lead", ct);
        if (lead.PipelineStatus == "Converted") throw new AppException("Lead đã convert.");
        lead.PipelineStatus = "Lost";
        lead.LostReason = Req(req.LostReason, 500, "Lý do mất");
        lead.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await AddActivityInternal(tenantId, userId, leadId, "Note", $"Lead mất: {lead.LostReason}", ct);
        return (await MapLeadsAsync(tenantId, [lead], ct))[0];
    }

    public async Task<CrmOpportunityDto> ConvertToOpportunityAsync(
        Guid tenantId, Guid userId, Guid leadId, CancellationToken ct = default)
    {
        var lead = await RequireAsync(_db.CrmLeads, tenantId, leadId, "lead", ct);
        if (lead.PipelineStatus == "Lost") throw new AppException("Lead đã Lost.");
        if (lead.OpportunityId is not null) throw new AppException("Lead đã có cơ hội.");

        Guid? customerId = lead.CustomerId;
        if (customerId is null)
        {
            var cust = new CrmCustomer
            {
                TenantId = tenantId,
                Code = await NextCodeAsync(tenantId, "CUS", _db.CrmCustomers, ct),
                CustomerType = string.IsNullOrWhiteSpace(lead.CompanyName) ? "Person" : "Organization",
                DisplayName = lead.Name,
                CompanyName = lead.CompanyName,
                Phone = lead.Phone,
                Email = lead.Email,
                Segment = "Prospect",
                OwnerUserId = lead.OwnerUserId,
                Status = "Active",
                CreatedBy = userId
            };
            _db.CrmCustomers.Add(cust);
            await _db.SaveChangesAsync(ct);
            customerId = cust.Id;
            lead.CustomerId = customerId;
        }

        var opp = new CrmOpportunity
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "OPP", _db.CrmOpportunities, ct),
            Name = $"Cơ hội · {lead.Name}",
            LeadId = lead.Id,
            CustomerId = customerId,
            OwnerUserId = lead.OwnerUserId,
            Stage = "Qualification",
            EstimatedValue = 0,
            ProbabilityPercent = 20,
            CreatedBy = userId
        };
        _db.CrmOpportunities.Add(opp);
        await _db.SaveChangesAsync(ct);

        lead.PipelineStatus = "Converted";
        lead.OpportunityId = opp.Id;
        lead.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await AddActivityInternal(tenantId, userId, leadId, "Note", $"Convert → {opp.Code}", ct);
        return (await MapOppsAsync(tenantId, [opp], ct))[0];
    }

    public async Task<CrmLeadTaskDto> UpsertTaskAsync(
        Guid tenantId, Guid userId, CrmLeadTaskUpsertRequest req, CancellationToken ct = default)
    {
        var lead = await RequireAsync(_db.CrmLeads, tenantId, req.LeadId, "lead", ct);
        var title = Req(req.Title, 200, "Tiêu đề task");
        if (req.AssigneeUserId is Guid aid) await EnsureUserAsync(tenantId, aid, ct);
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Open" : req.Status.Trim();
        if (status is not ("Open" or "Done" or "Cancelled")) throw new AppException("TT task: Open|Done|Cancelled.");

        CrmLeadTask entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmLeadTasks, tenantId, id, "task", ct);
        else
        {
            entity = new CrmLeadTask { TenantId = tenantId, LeadId = req.LeadId, CreatedBy = userId };
            _db.CrmLeadTasks.Add(entity);
        }
        entity.Title = title;
        entity.DueAt = req.DueAt;
        entity.AssigneeUserId = req.AssigneeUserId ?? lead.OwnerUserId;
        entity.Status = status;
        entity.IsReminder = req.IsReminder ?? entity.IsReminder;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;

        if (entity.IsReminder || lead.NextFollowUpAt is null || req.DueAt < lead.NextFollowUpAt)
            lead.NextFollowUpAt = req.DueAt;
        lead.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await LoadTasksAsync(tenantId, req.LeadId, ct)).First(x => x.Id == entity.Id);
    }

    public async Task<CrmLeadActivityDto> AddActivityAsync(
        Guid tenantId, Guid userId, CrmLeadActivityUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.CrmLeads, tenantId, req.LeadId, "lead", ct);
        var type = (req.ActivityType ?? "").Trim();
        if (!ActivityTypes.Contains(type)) throw new AppException("ActivityType: Call|Email|Meeting|Note|Other.");
        var content = Req(req.Content, 2000, "Nội dung");
        var act = await AddActivityInternal(tenantId, userId, req.LeadId, type, content, ct, req.ActivityAt);
        var name = await UserNameAsync(userId, ct);
        return new CrmLeadActivityDto(act.Id, act.LeadId, act.ActivityType, act.Content, act.CreatedByUserId, name, act.ActivityAt);
    }

    public async Task<CrmLeadImportResult> ImportCsvAsync(
        Guid tenantId, Guid userId, CrmLeadImportRequest req, CancellationToken ct = default)
    {
        var lines = (req.CsvContent ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0) throw new AppException("CSV trống.");
        var start = lines[0].Contains("Name", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        var created = 0; var skipped = 0; var errors = new List<string>();
        for (var i = start; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 1 || string.IsNullOrWhiteSpace(cols[0])) { skipped++; continue; }
            try
            {
                await UpsertLeadAsync(tenantId, userId, new CrmLeadUpsertRequest(
                    null, null, cols[0].Trim(),
                    cols.ElementAtOrDefault(1)?.Trim(),
                    cols.ElementAtOrDefault(2)?.Trim(),
                    cols.ElementAtOrDefault(3)?.Trim(),
                    null, null, null, "New", null, null, null, "Manual"), ct);
                created++;
            }
            catch (Exception ex) { errors.Add($"Dòng {i + 1}: {ex.Message}"); skipped++; }
        }
        return new CrmLeadImportResult(created, skipped, errors);
    }

    public async Task<CrmLeadConversionReportDto> GetConversionReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var leads = await _db.CrmLeads.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.MergedIntoId == null).ToListAsync(ct);
        var total = leads.Count;
        var converted = leads.Count(x => x.PipelineStatus == "Converted");
        var lost = leads.Count(x => x.PipelineStatus == "Lost");
        var rate = total == 0 ? 0 : Math.Round(100m * converted / total, 2);
        var rows = LeadStatuses.Select(s =>
        {
            var c = leads.Count(x => x.PipelineStatus.Equals(s, StringComparison.OrdinalIgnoreCase));
            return new CrmLeadConversionRowDto(s, c, total == 0 ? 0 : Math.Round(100m * c / total, 2));
        }).ToList();
        return new CrmLeadConversionReportDto(total, converted, lost, rate, rows);
    }

    public async Task<CrmLeadDto> CalculateLeadScoreAsync(
        Guid tenantId, Guid userId, Guid leadId, CancellationToken ct = default)
    {
        var lead = await RequireAsync(_db.CrmLeads, tenantId, leadId, "lead", ct);
        var score = 0;
        if (!string.IsNullOrWhiteSpace(lead.Phone)) score += 20;
        if (!string.IsNullOrWhiteSpace(lead.Email)) score += 20;
        if (!string.IsNullOrWhiteSpace(lead.CompanyName)) score += 20;
        var actCount = await _db.CrmLeadActivities.CountAsync(
            x => x.TenantId == tenantId && x.LeadId == leadId && !x.IsDeleted, ct);
        score += Math.Min(actCount * 10, 40);

        lead.Score = score;
        lead.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapLeadsAsync(tenantId, [lead], ct))[0];
    }

    public async Task<CrmLeadDto> MergeLeadsAsync(
        Guid tenantId, Guid userId, CrmLeadMergeRequest req, CancellationToken ct = default)
    {
        if (req.PrimaryLeadId == req.SecondaryLeadId)
            throw new AppException("Chỉ định 2 Lead khác nhau để gộp.");
        var primary = await RequireAsync(_db.CrmLeads, tenantId, req.PrimaryLeadId, "lead chính", ct);
        var secondary = await RequireAsync(_db.CrmLeads, tenantId, req.SecondaryLeadId, "lead phụ", ct);

        if (secondary.MergedIntoId != null)
            throw new AppException("Lead phụ đã được gộp trước đó.");

        // Chuyển toàn bộ Tasks và Activities sang Lead chính
        var tasks = await _db.CrmLeadTasks
            .Where(x => x.TenantId == tenantId && x.LeadId == secondary.Id && !x.IsDeleted).ToListAsync(ct);
        foreach (var t in tasks) { t.LeadId = primary.Id; t.UpdatedBy = userId; }

        var acts = await _db.CrmLeadActivities
            .Where(x => x.TenantId == tenantId && x.LeadId == secondary.Id && !x.IsDeleted).ToListAsync(ct);
        foreach (var a in acts) { a.LeadId = primary.Id; }

        // Bổ sung thông tin thiếu từ secondary sang primary nếu primary trống
        if (string.IsNullOrWhiteSpace(primary.Phone) && !string.IsNullOrWhiteSpace(secondary.Phone))
            primary.Phone = secondary.Phone;
        if (string.IsNullOrWhiteSpace(primary.Email) && !string.IsNullOrWhiteSpace(secondary.Email))
            primary.Email = secondary.Email;
        if (string.IsNullOrWhiteSpace(primary.CompanyName) && !string.IsNullOrWhiteSpace(secondary.CompanyName))
            primary.CompanyName = secondary.CompanyName;

        secondary.MergedIntoId = primary.Id;
        secondary.PipelineStatus = "Lost";
        secondary.LostReason = NullIfEmpty(req.Reason) ?? $"Gộp trùng vào lead {primary.Code}";
        secondary.UpdatedBy = userId;
        primary.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);

        // Tự động tính lại Score cho Lead chính
        return await CalculateLeadScoreAsync(tenantId, userId, primary.Id, ct);
    }

    public async Task<IReadOnlyList<CrmOpportunityDto>> ListOpportunitiesAsync(
        Guid tenantId, string? q, string? stage, CancellationToken ct = default)
    {
        var query = _db.CrmOpportunities.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(stage))
        {
            var s = stage.Trim();
            query = query.Where(x => x.Stage == s);
        }
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return await MapOppsAsync(tenantId, list, ct);
    }

    public async Task<CrmOpportunityDetailDto> GetOpportunityDetailAsync(
        Guid tenantId, Guid opportunityId, CancellationToken ct = default)
    {
        var opp = await _db.CrmOpportunities.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == opportunityId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy cơ hội.", 404);
        var dto = (await MapOppsAsync(tenantId, [opp], ct))[0];
        var lines = await _db.CrmOpportunityLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OpportunityId == opportunityId && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        return new CrmOpportunityDetailDto(dto, lines.Select(l => new CrmOpportunityLineDto(
            l.Id, l.OpportunityId, l.ItemCode, l.ItemName, l.Quantity, l.UnitPrice, l.LineAmount, l.LineNo)).ToList());
    }

    public async Task<CrmOpportunityDto> UpsertOpportunityAsync(
        Guid tenantId, Guid userId, CrmOpportunityUpsertRequest req, CancellationToken ct = default)
    {
        var name = Req(req.Name, 200, "Tên cơ hội");
        if (req.LeadId is Guid lid) _ = await RequireAsync(_db.CrmLeads, tenantId, lid, "lead", ct);
        if (req.CustomerId is Guid cid) _ = await RequireAsync(_db.CrmCustomers, tenantId, cid, "khách hàng", ct);
        if (req.OwnerUserId is Guid oid) await EnsureUserAsync(tenantId, oid, ct);
        var stage = string.IsNullOrWhiteSpace(req.Stage) ? "Qualification" : req.Stage.Trim();
        if (!OppStages.Contains(stage)) throw new AppException("Stage: Qualification|Proposal|Negotiation|Won|Lost.");

        CrmOpportunity entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmOpportunities, tenantId, id, "cơ hội", ct);
        else
        {
            entity = new CrmOpportunity
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "OPP", _db.CrmOpportunities, ct) : NormCode(req.Code),
                CreatedBy = userId
            };
            _db.CrmOpportunities.Add(entity);
        }
        entity.Name = name;
        entity.LeadId = req.LeadId;
        entity.CustomerId = req.CustomerId;
        entity.OwnerUserId = req.OwnerUserId;
        entity.Stage = OppStages.First(x => x.Equals(stage, StringComparison.OrdinalIgnoreCase));
        entity.EstimatedValue = req.EstimatedValue ?? entity.EstimatedValue;
        entity.ProbabilityPercent = req.ProbabilityPercent ?? entity.ProbabilityPercent;
        entity.ExpectedCloseDate = req.ExpectedCloseDate;
        if (!string.IsNullOrWhiteSpace(req.CompetitorName)) entity.CompetitorName = NullIfEmpty(req.CompetitorName);
        if (!string.IsNullOrWhiteSpace(req.NegotiationNotes)) entity.NegotiationNotes = NullIfEmpty(req.NegotiationNotes);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOppsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<CrmOpportunityDto> UpdateCompetitorInfoAsync(
        Guid tenantId, Guid userId, Guid opportunityId, CrmOpportunityCompetitorRequest req, CancellationToken ct = default)
    {
        var opp = await RequireAsync(_db.CrmOpportunities, tenantId, opportunityId, "cơ hội", ct);
        opp.CompetitorName = Req(req.CompetitorName, 200, "Tên đối thủ");
        opp.NegotiationNotes = NullIfEmpty(req.NegotiationNotes);
        opp.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOppsAsync(tenantId, [opp], ct))[0];
    }

    public async Task<CrmOpportunityLineDto> UpsertOpportunityLineAsync(
        Guid tenantId, Guid userId, Guid opportunityId, CrmOpportunityLineUpsertRequest req, CancellationToken ct = default)
    {
        var opp = await RequireAsync(_db.CrmOpportunities, tenantId, opportunityId, "cơ hội", ct);
        if (req.Quantity <= 0 || req.UnitPrice < 0) throw new AppException("SL > 0, đơn giá ≥ 0.");
        CrmOpportunityLine entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmOpportunityLines, tenantId, id, "dòng SP", ct);
        else
        {
            var max = await _db.CrmOpportunityLines
                .Where(x => x.TenantId == tenantId && x.OpportunityId == opportunityId && !x.IsDeleted)
                .Select(x => (int?)x.LineNo).MaxAsync(ct) ?? 0;
            entity = new CrmOpportunityLine
            {
                TenantId = tenantId, OpportunityId = opportunityId, LineNo = max + 1, CreatedBy = userId
            };
            _db.CrmOpportunityLines.Add(entity);
        }
        entity.ItemCode = NormCode(req.ItemCode);
        entity.ItemName = Req(req.ItemName, 200, "Tên SP");
        entity.Quantity = req.Quantity;
        entity.UnitPrice = req.UnitPrice;
        entity.LineAmount = Math.Round(req.Quantity * req.UnitPrice, 2);
        entity.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        var total = await _db.CrmOpportunityLines
            .Where(x => x.TenantId == tenantId && x.OpportunityId == opportunityId && !x.IsDeleted)
            .SumAsync(x => x.LineAmount, ct);
        opp.EstimatedValue = total;
        opp.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new CrmOpportunityLineDto(
            entity.Id, entity.OpportunityId, entity.ItemCode, entity.ItemName,
            entity.Quantity, entity.UnitPrice, entity.LineAmount, entity.LineNo);
    }

    public async Task<CrmOpportunityDto> SetOpportunityStageAsync(
        Guid tenantId, Guid userId, Guid opportunityId, CrmOpportunityStageRequest req, CancellationToken ct = default)
    {
        var opp = await RequireAsync(_db.CrmOpportunities, tenantId, opportunityId, "cơ hội", ct);
        var stage = (req.Stage ?? "").Trim();
        if (!OppStages.Contains(stage)) throw new AppException("Stage: Qualification|Proposal|Negotiation|Won|Lost.");
        opp.Stage = OppStages.First(x => x.Equals(stage, StringComparison.OrdinalIgnoreCase));
        if (opp.Stage == "Lost")
            opp.LostReason = Req(req.LostReason, 500, "Lý do thua");
        if (opp.Stage == "Won")
        {
            opp.ProbabilityPercent = 100;
            opp.LostReason = null;
        }
        opp.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOppsAsync(tenantId, [opp], ct))[0];
    }

    public Task<CrmQuoteDto> CreateQuoteFromOpportunityAsync(
        Guid tenantId, Guid userId, Guid opportunityId, CancellationToken ct = default)
        => _sales.CreateQuoteFromOpportunityAsync(tenantId, userId, opportunityId, ct);

    public async Task<CrmRevenueForecastDto> GetRevenueForecastAsync(Guid tenantId, CancellationToken ct = default)
    {
        var opps = await _db.CrmOpportunities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Stage != "Lost").ToListAsync(ct);

        var totalEstimated = opps.Sum(x => x.EstimatedValue);
        var weightedForecast = opps.Sum(x => Math.Round(x.EstimatedValue * x.ProbabilityPercent / 100m, 2));

        var monthly = opps
            .GroupBy(x => (x.ExpectedCloseDate ?? x.CreatedAt).ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new CrmRevenueForecastMonthlyDto(
                g.Key,
                g.Count(),
                g.Sum(x => x.EstimatedValue),
                g.Sum(x => Math.Round(x.EstimatedValue * x.ProbabilityPercent / 100m, 2))))
            .ToList();

        return new CrmRevenueForecastDto(totalEstimated, weightedForecast, monthly);
    }

    public async Task<CrmWinRateReportDto> GetWinRateReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var opps = await _db.CrmOpportunities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);

        var total = opps.Count;
        var won = opps.Count(x => x.Stage == "Won");
        var lost = opps.Count(x => x.Stage == "Lost");
        var inProgress = total - won - lost;

        var winRate = total > 0 ? Math.Round(100m * won / total, 2) : 0m;
        var lossRate = total > 0 ? Math.Round(100m * lost / total, 2) : 0m;

        var lossReasons = opps
            .Where(x => x.Stage == "Lost" && !string.IsNullOrWhiteSpace(x.LostReason))
            .GroupBy(x => x.LostReason!.Trim())
            .Select(g => new CrmLossReasonBreakdownDto(
                g.Key,
                g.Count(),
                lost > 0 ? Math.Round(100m * g.Count() / lost, 2) : 0m))
            .OrderByDescending(x => x.Count)
            .ToList();

        return new CrmWinRateReportDto(total, won, lost, inProgress, winRate, lossRate, lossReasons);
    }

    private async Task<CrmLeadActivity> AddActivityInternal(
        Guid tenantId, Guid userId, Guid leadId, string type, string content, CancellationToken ct,
        DateTimeOffset? at = null)
    {
        var act = new CrmLeadActivity
        {
            TenantId = tenantId, LeadId = leadId,
            ActivityType = ActivityTypes.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase)),
            Content = content, CreatedByUserId = userId,
            ActivityAt = at ?? DateTimeOffset.UtcNow, CreatedBy = userId
        };
        _db.CrmLeadActivities.Add(act);
        await _db.SaveChangesAsync(ct);
        return act;
    }

    private async Task<IReadOnlyList<CrmLeadDto>> MapLeadsAsync(Guid tenantId, List<CrmLead> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var sids = list.Where(x => x.SourceId.HasValue).Select(x => x.SourceId!.Value).Distinct().ToList();
        var uids = list.Where(x => x.OwnerUserId.HasValue).Select(x => x.OwnerUserId!.Value).Distinct().ToList();
        var sources = sids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmLeadSources.AsNoTracking().Where(x => sids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var users = uids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        var taskCounts = await _db.CrmLeadTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.LeadId) && !x.IsDeleted && x.Status == "Open")
            .GroupBy(x => x.LeadId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var actCounts = await _db.CrmLeadActivities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.LeadId) && !x.IsDeleted)
            .GroupBy(x => x.LeadId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(l => new CrmLeadDto(
            l.Id, l.Code, l.Name, l.Phone, l.Email, l.CompanyName, l.SourceId,
            l.SourceId is Guid s ? sources.GetValueOrDefault(s) : null,
            l.OwnerUserId, l.OwnerUserId is Guid u ? users.GetValueOrDefault(u) : null,
            l.CustomerId, l.PipelineStatus, l.Score, l.NextFollowUpAt, l.LostReason,
            l.OpportunityId, l.IntakeChannel, l.Note,
            taskCounts.GetValueOrDefault(l.Id), actCounts.GetValueOrDefault(l.Id))).ToList();
    }

    private async Task<IReadOnlyList<CrmOpportunityDto>> MapOppsAsync(
        Guid tenantId, List<CrmOpportunity> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var lids = list.Where(x => x.LeadId.HasValue).Select(x => x.LeadId!.Value).Distinct().ToList();
        var cids = list.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();
        var uids = list.Where(x => x.OwnerUserId.HasValue).Select(x => x.OwnerUserId!.Value).Distinct().ToList();
        var qids = list.Where(x => x.QuoteId.HasValue).Select(x => x.QuoteId!.Value).Distinct().ToList();
        var leads = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmLeads.AsNoTracking().Where(x => lids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var custs = cids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, ct);
        var users = uids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        var quotes = qids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmQuotes.AsNoTracking().Where(x => qids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var lineCounts = await _db.CrmOpportunityLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.OpportunityId) && !x.IsDeleted)
            .GroupBy(x => x.OpportunityId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(o => new CrmOpportunityDto(
            o.Id, o.Code, o.Name, o.LeadId, o.LeadId is Guid l ? leads.GetValueOrDefault(l) : null,
            o.CustomerId, o.CustomerId is Guid c ? custs.GetValueOrDefault(c) : null,
            o.OwnerUserId, o.OwnerUserId is Guid u ? users.GetValueOrDefault(u) : null,
            o.Stage, o.EstimatedValue, o.ProbabilityPercent, o.ExpectedCloseDate,
            o.QuoteId, o.QuoteId is Guid q ? quotes.GetValueOrDefault(q) : null,
            o.LostReason, o.CompetitorName, o.NegotiationNotes, o.Note, lineCounts.GetValueOrDefault(o.Id))).ToList();
    }

    private async Task<IReadOnlyList<CrmLeadTaskDto>> LoadTasksAsync(Guid tenantId, Guid leadId, CancellationToken ct)
    {
        var list = await _db.CrmLeadTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.LeadId == leadId && !x.IsDeleted)
            .OrderBy(x => x.DueAt).ToListAsync(ct);
        var uids = list.Where(x => x.AssigneeUserId.HasValue).Select(x => x.AssigneeUserId!.Value).Distinct().ToList();
        var users = uids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        return list.Select(t => new CrmLeadTaskDto(
            t.Id, t.LeadId, t.Title, t.DueAt, t.AssigneeUserId,
            t.AssigneeUserId is Guid u ? users.GetValueOrDefault(u) : null,
            t.Status, t.IsReminder, t.Note)).ToList();
    }

    private async Task<IReadOnlyList<CrmLeadActivityDto>> LoadActivitiesAsync(Guid tenantId, Guid leadId, CancellationToken ct)
    {
        var list = await _db.CrmLeadActivities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.LeadId == leadId && !x.IsDeleted)
            .OrderByDescending(x => x.ActivityAt).Take(100).ToListAsync(ct);
        var uids = list.Select(x => x.CreatedByUserId).Distinct().ToList();
        var users = await _db.Users.AsNoTracking()
            .Where(x => uids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        return list.Select(a => new CrmLeadActivityDto(
            a.Id, a.LeadId, a.ActivityType, a.Content, a.CreatedByUserId,
            users.GetValueOrDefault(a.CreatedByUserId), a.ActivityAt)).ToList();
    }

    private async Task EnsureUserAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var ok = await _db.Users.AnyAsync(x => x.Id == userId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Người dùng không hợp lệ.");
    }

    private async Task<string?> UserNameAsync(Guid userId, CancellationToken ct)
    {
        var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct);
        return u?.DisplayName ?? u?.Username;
    }

    private static async Task<string> NextCodeAsync<T>(
        Guid tenantId, string prefix, DbSet<T> set, CancellationToken ct) where T : TenantEntity
    {
        var p = $"{prefix}-{DateTime.UtcNow:yyyyMM}-";
        var last = await set.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && EF.Property<string>(x, "Code").StartsWith(p))
            .OrderByDescending(x => EF.Property<string>(x, "Code"))
            .Select(x => EF.Property<string>(x, "Code")).FirstOrDefaultAsync(ct);
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

    private static string ActiveInactive(string? s)
    {
        var v = string.IsNullOrWhiteSpace(s) ? "Active" : s.Trim();
        if (v is not ("Active" or "Inactive")) throw new AppException("Trạng thái: Active | Inactive.");
        return v;
    }

    private static string? NullIfEmpty(string? s)
    {
        var v = s?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }
}
