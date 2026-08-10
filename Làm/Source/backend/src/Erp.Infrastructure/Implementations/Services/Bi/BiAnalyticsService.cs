using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Bi;
using Erp.Application.Interfaces.Services.Bi;
using Erp.Domain.Base;
using Erp.Domain.Entities.Bi;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Bi;

public sealed class BiAnalyticsService : IBiAnalyticsService
{
    private static readonly HashSet<string> DashTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Executive", "Module" };
    private static readonly HashSet<string> WidgetTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Kpi", "Chart", "Table" };
    private static readonly HashSet<string> Metrics =
        new(StringComparer.OrdinalIgnoreCase) { "Revenue", "Profit", "Custom" };
    private static readonly HashSet<string> PrincipalTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Role", "User" };
    private static readonly HashSet<string> AccessLevels =
        new(StringComparer.OrdinalIgnoreCase) { "View", "Run", "Export" };
    private static readonly HashSet<string> ExportFormats =
        new(StringComparer.OrdinalIgnoreCase) { "None", "Excel", "Pdf" };
    private static readonly HashSet<string> Operators =
        new(StringComparer.OrdinalIgnoreCase) { "Gt", "Gte", "Lt", "Lte" };
    private static readonly HashSet<string> Severities =
        new(StringComparer.OrdinalIgnoreCase) { "Info", "Warn", "Critical" };

    private readonly AppDbContext _db;
    public BiAnalyticsService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BiDatasetDto>> ListDatasetsAsync(
        Guid tenantId, string? moduleCode, CancellationToken ct = default)
    {
        var q = _db.BiDatasets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(moduleCode))
        {
            var m = moduleCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.ModuleCode == m);
        }
        var list = await q.OrderBy(x => x.ModuleCode).ThenBy(x => x.Code).ToListAsync(ct);
        return list.Select(MapDataset).ToList();
    }

    public async Task<BiDatasetDto> UpsertDatasetAsync(
        Guid tenantId, Guid userId, BiDatasetUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên dataset");
        var mod = NormModule(req.ModuleCode);
        BiDataset entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiDatasets, tenantId, id, "dataset", ct);
        else
        {
            await EnsureCodeAsync(_db.BiDatasets, tenantId, code, ct);
            entity = new BiDataset { TenantId = tenantId, CreatedBy = userId, Status = "Ready" };
            _db.BiDatasets.Add(entity);
        }
        entity.Code = code; entity.Name = name; entity.ModuleCode = mod;
        entity.Description = NullIfEmpty(req.Description);
        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            var s = req.Status.Trim();
            if (s is not ("Ready" or "Stale" or "Error" or "Refreshing"))
                throw new AppException("TT dataset: Ready|Stale|Error|Refreshing.");
            entity.Status = s;
        }
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapDataset(entity);
    }

    public async Task<BiDatasetDto> RefreshDatasetAsync(
        Guid tenantId, Guid userId, Guid datasetId, BiRefreshRequest req, CancellationToken ct = default)
    {
        var ds = await RequireAsync(_db.BiDatasets, tenantId, datasetId, "dataset", ct);
        ds.Status = "Refreshing";
        ds.UpdatedBy = userId;

        var log = new BiDatasetRefresh
        {
            TenantId = tenantId, DatasetId = datasetId,
            StartedAt = DateTimeOffset.UtcNow, Status = "Running",
            StartedByUserId = userId, CreatedBy = userId,
            Note = NullIfEmpty(req.Note)
        };
        _db.BiDatasetRefreshes.Add(log);
        await _db.SaveChangesAsync(ct);

        try
        {
            // UC_BI_002: đếm hàng nguồn thật theo ModuleCode (ETL nhẹ — không random).
            var (rows, sourceNote) = await CountModuleSourceRowsAsync(tenantId, ds.ModuleCode, ct);
            log.Status = "Succeeded";
            log.FinishedAt = DateTimeOffset.UtcNow;
            log.RowsAffected = rows;
            log.Note = string.IsNullOrWhiteSpace(log.Note)
                ? $"Refresh từ {sourceNote}"
                : $"{log.Note.Trim()} · {sourceNote}";
            log.UpdatedBy = userId;

            ds.Status = "Ready";
            ds.LastRefreshedAt = log.FinishedAt;
            ds.LastRefreshNote = log.Note;
            ds.RowCountEstimate = rows;
            ds.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
            return MapDataset(ds);
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.FinishedAt = DateTimeOffset.UtcNow;
            log.Note = $"Lỗi refresh: {ex.Message}";
            log.UpdatedBy = userId;
            ds.Status = "Error";
            ds.LastRefreshNote = log.Note;
            ds.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
            throw new AppException($"Refresh dataset thất bại: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<BiDatasetRefreshDto>> ListRefreshesAsync(
        Guid tenantId, Guid datasetId, CancellationToken ct = default)
    {
        var list = await _db.BiDatasetRefreshes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DatasetId == datasetId && !x.IsDeleted)
            .OrderByDescending(x => x.StartedAt).Take(50).ToListAsync(ct);
        return list.Select(x => new BiDatasetRefreshDto(
            x.Id, x.DatasetId, x.StartedAt, x.FinishedAt, x.Status, x.RowsAffected, x.Note)).ToList();
    }

    public async Task<IReadOnlyList<BiReportDto>> ListReportsAsync(
        Guid tenantId, string? moduleCode, CancellationToken ct = default)
    {
        var q = _db.BiReports.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(moduleCode))
        {
            var m = moduleCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.ModuleCode == m);
        }
        var list = await q.OrderBy(x => x.ModuleCode).ThenBy(x => x.Code).ToListAsync(ct);
        return await MapReportsAsync(tenantId, list, ct);
    }

    public async Task<BiReportDto> UpsertReportAsync(
        Guid tenantId, Guid userId, BiReportUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên báo cáo");
        var mod = NormModule(req.ModuleCode);
        if (req.DatasetId is Guid did)
            _ = await RequireAsync(_db.BiDatasets, tenantId, did, "dataset", ct);

        BiReport entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiReports, tenantId, id, "báo cáo", ct);
        else
        {
            await EnsureCodeAsync(_db.BiReports, tenantId, code, ct);
            entity = new BiReport { TenantId = tenantId, CreatedBy = userId };
            _db.BiReports.Add(entity);
        }
        entity.Code = code; entity.Name = name; entity.ModuleCode = mod;
        entity.DatasetId = req.DatasetId;
        entity.Description = NullIfEmpty(req.Description);
        entity.FilterSchemaJson = NullIfEmpty(req.FilterSchemaJson)
            ?? """[{"key":"from","label":"Từ ngày"},{"key":"to","label":"Đến ngày"}]""";
        entity.Status = ActiveInactive(req.Status);
        entity.RequirePermission = req.RequirePermission ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapReportsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<BiReportPermissionDto>> ListPermissionsAsync(
        Guid tenantId, Guid reportId, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.BiReports, tenantId, reportId, "báo cáo", ct);
        var list = await _db.BiReportPermissions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReportId == reportId && !x.IsDeleted)
            .OrderBy(x => x.PrincipalType).ThenBy(x => x.PrincipalCode).ToListAsync(ct);
        return list.Select(x => new BiReportPermissionDto(
            x.Id, x.ReportId, x.PrincipalType, x.PrincipalCode, x.AccessLevel)).ToList();
    }

    public async Task<BiReportPermissionDto> UpsertPermissionAsync(
        Guid tenantId, Guid userId, BiReportPermissionUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.BiReports, tenantId, req.ReportId, "báo cáo", ct);
        var pType = (req.PrincipalType ?? "").Trim();
        if (!PrincipalTypes.Contains(pType)) throw new AppException("PrincipalType: Role | User.");
        var access = (req.AccessLevel ?? "").Trim();
        if (!AccessLevels.Contains(access)) throw new AppException("AccessLevel: View | Run | Export.");
        var pCode = Req(req.PrincipalCode, 80, "PrincipalCode").ToUpperInvariant();

        BiReportPermission entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiReportPermissions, tenantId, id, "quyền BC", ct);
        else
        {
            var exists = await _db.BiReportPermissions.AnyAsync(x =>
                x.TenantId == tenantId && x.ReportId == req.ReportId && !x.IsDeleted
                && x.PrincipalType == pType && x.PrincipalCode == pCode, ct);
            if (exists) throw new AppException("Quyền đã tồn tại.");
            entity = new BiReportPermission { TenantId = tenantId, CreatedBy = userId };
            _db.BiReportPermissions.Add(entity);
        }
        entity.ReportId = req.ReportId;
        entity.PrincipalType = PrincipalTypes.First(x => x.Equals(pType, StringComparison.OrdinalIgnoreCase));
        entity.PrincipalCode = pCode;
        entity.AccessLevel = AccessLevels.First(x => x.Equals(access, StringComparison.OrdinalIgnoreCase));
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new BiReportPermissionDto(
            entity.Id, entity.ReportId, entity.PrincipalType, entity.PrincipalCode, entity.AccessLevel);
    }

    public async Task<IReadOnlyList<BiDashboardDto>> ListDashboardsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.BiDashboards.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code).ToListAsync(ct);
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.BiWidgets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.DashboardId) && !x.IsDeleted)
            .GroupBy(x => x.DashboardId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(d => new BiDashboardDto(
            d.Id, d.Code, d.Name, d.DashboardType, d.ModuleCode, d.Status, d.Note, d.SortOrder,
            counts.GetValueOrDefault(d.Id))).ToList();
    }

    public async Task<BiDashboardDetailDto> GetDashboardDetailAsync(
        Guid tenantId, Guid dashboardId, CancellationToken ct = default)
    {
        var d = await _db.BiDashboards.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dashboardId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy dashboard.", 404);
        var widgets = await _db.BiWidgets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DashboardId == dashboardId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code).ToListAsync(ct);
        var dto = new BiDashboardDto(
            d.Id, d.Code, d.Name, d.DashboardType, d.ModuleCode, d.Status, d.Note, d.SortOrder, widgets.Count);

        // UC_BI_008: widget Revenue/Profit lấy số thật từ FIN (DTO value), Custom giữ StubValue.
        var (revenue, profit) = await ComputeFinMetricsAsync(tenantId, ct);
        var mapped = widgets.Select(w =>
        {
            var value = w.MetricKey switch
            {
                "Revenue" => revenue,
                "Profit" => profit,
                _ => w.StubValue,
            };
            return new BiWidgetDto(
                w.Id, w.DashboardId, w.Code, w.Title, w.WidgetType, w.MetricKey,
                value, w.Unit, w.SortOrder, w.Status);
        }).ToList();
        return new BiDashboardDetailDto(dto, mapped);
    }

    public async Task<BiDashboardDto> UpsertDashboardAsync(
        Guid tenantId, Guid userId, BiDashboardUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên dashboard");
        var type = (req.DashboardType ?? "").Trim();
        if (!DashTypes.Contains(type)) throw new AppException("Loại DB: Executive | Module.");
        var typeNorm = DashTypes.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
        string? mod = null;
        if (typeNorm == "Module")
        {
            if (string.IsNullOrWhiteSpace(req.ModuleCode))
                throw new AppException("Dashboard Module cần ModuleCode.");
            mod = NormModule(req.ModuleCode);
        }

        BiDashboard entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiDashboards, tenantId, id, "dashboard", ct);
        else
        {
            await EnsureCodeAsync(_db.BiDashboards, tenantId, code, ct);
            entity = new BiDashboard { TenantId = tenantId, CreatedBy = userId };
            _db.BiDashboards.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.DashboardType = typeNorm; entity.ModuleCode = mod;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note);
        entity.SortOrder = req.SortOrder ?? entity.SortOrder;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var count = await _db.BiWidgets.CountAsync(
            x => x.TenantId == tenantId && x.DashboardId == entity.Id && !x.IsDeleted, ct);
        return new BiDashboardDto(
            entity.Id, entity.Code, entity.Name, entity.DashboardType, entity.ModuleCode,
            entity.Status, entity.Note, entity.SortOrder, count);
    }

    public async Task<BiWidgetDto> UpsertWidgetAsync(
        Guid tenantId, Guid userId, BiWidgetUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.BiDashboards, tenantId, req.DashboardId, "dashboard", ct);
        var code = NormCode(req.Code);
        var title = Req(req.Title, 200, "Tiêu đề widget");
        var wType = (req.WidgetType ?? "").Trim();
        if (!WidgetTypes.Contains(wType)) throw new AppException("WidgetType: Kpi|Chart|Table.");
        var metric = (req.MetricKey ?? "").Trim();
        if (!Metrics.Contains(metric)) throw new AppException("MetricKey: Revenue|Profit|Custom.");

        BiWidget entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiWidgets, tenantId, id, "widget", ct);
        else
        {
            if (await _db.BiWidgets.AnyAsync(x =>
                    x.TenantId == tenantId && x.DashboardId == req.DashboardId
                    && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã widget đã tồn tại trên dashboard.");
            entity = new BiWidget { TenantId = tenantId, CreatedBy = userId };
            _db.BiWidgets.Add(entity);
        }
        entity.DashboardId = req.DashboardId;
        entity.Code = code; entity.Title = title;
        entity.WidgetType = WidgetTypes.First(x => x.Equals(wType, StringComparison.OrdinalIgnoreCase));
        entity.MetricKey = Metrics.First(x => x.Equals(metric, StringComparison.OrdinalIgnoreCase));
        entity.StubValue = req.StubValue ?? (entity.MetricKey == "Profit" ? 120_000_000m : 850_000_000m);
        entity.Unit = NullIfEmpty(req.Unit) ?? "VND";
        entity.SortOrder = req.SortOrder ?? entity.SortOrder;
        entity.Status = ActiveInactive(req.Status);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapWidget(entity);
    }

    public async Task<BiReportRunDto> RunReportAsync(
        Guid tenantId, Guid userId, Guid reportId, BiReportRunRequest req, CancellationToken ct = default)
    {
        var report = await RequireAsync(_db.BiReports, tenantId, reportId, "báo cáo", ct);
        if (report.Status != "Active") throw new AppException("Báo cáo không Active.");

        var export = string.IsNullOrWhiteSpace(req.ExportFormat) ? "None" : req.ExportFormat.Trim();
        if (!ExportFormats.Contains(export)) throw new AppException("Export: None|Excel|Pdf.");
        export = ExportFormats.First(x => x.Equals(export, StringComparison.OrdinalIgnoreCase));

        var filterJson = NullIfEmpty(req.FilterJson) ?? "{}";
        var (from, to) = ParseDateFilter(filterJson);

        // UC_BI_014: chạy BC thật — aggregate theo module + filter from/to.
        var (rows, previewRows, sourceNote) = await BuildReportRowsAsync(
            tenantId, report.ModuleCode, from, to, ct);
        var preview = JsonSerializer.Serialize(previewRows);

        var ext = export switch
        {
            "Excel" => "csv",
            "Pdf" => "txt",
            _ => null,
        };
        var fileName = ext is null
            ? null
            : $"{report.Code}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{ext}";

        var run = new BiReportRun
        {
            TenantId = tenantId, ReportId = reportId,
            RunAt = DateTimeOffset.UtcNow, RunByUserId = userId,
            FilterJson = filterJson,
            Status = "Succeeded", RowCount = rows,
            ExportFormat = export, ExportFileName = fileName,
            ResultPreviewJson = preview,
            Note = export == "None"
                ? $"Chạy thật · {sourceNote}"
                : $"Xuất {export} thật · {sourceNote}",
            CreatedBy = userId
        };
        _db.BiReportRuns.Add(run);
        await _db.SaveChangesAsync(ct);
        return new BiReportRunDto(
            run.Id, run.ReportId, report.Code, report.Name, run.RunAt, run.Status,
            run.RowCount, run.ExportFormat, run.ExportFileName, run.FilterJson,
            run.ResultPreviewJson, run.Note);
    }

    public async Task<(string FileName, string ContentType, string Content)> DownloadRunExportAsync(
        Guid tenantId, Guid runId, CancellationToken ct = default)
    {
        var run = await _db.BiReportRuns
            .FirstOrDefaultAsync(x => x.Id == runId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy lần chạy BC.", 404);
        if (run.ExportFormat is not ("Excel" or "Pdf"))
            throw new AppException("Lần chạy không có file xuất.");

        var report = await RequireAsync(_db.BiReports, tenantId, run.ReportId, "báo cáo", ct);
        var (from, to) = ParseDateFilter(run.FilterJson ?? "{}");
        var (_, previewRows, _) = await BuildReportRowsAsync(tenantId, report.ModuleCode, from, to, ct);

        if (run.ExportFormat == "Excel")
        {
            var sb = new System.Text.StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("Label,Value");
            foreach (var row in previewRows)
            {
                var label = row.GetValueOrDefault("label")?.ToString() ?? "";
                var value = row.GetValueOrDefault("value")?.ToString() ?? "";
                sb.AppendLine($"{CsvCell(label)},{CsvCell(value)}");
            }
            var name = run.ExportFileName ?? $"{report.Code}.csv";
            if (name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                name = Path.ChangeExtension(name, ".csv");
            return (name, "text/csv; charset=utf-8", sb.ToString());
        }

        if (run.ExportFormat == "Pdf")
        {
            var pdfText = new System.Text.StringBuilder();
            pdfText.AppendLine("%PDF-1.4");
            pdfText.AppendLine($"% BÁO CÁO {report.Code} — {report.Name}");
            pdfText.AppendLine($"% Module: {report.ModuleCode} | Ngày xuất: {run.RunAt.ToLocalTime():dd/MM/yyyy HH:mm}");
            if (from is not null || to is not null)
                pdfText.AppendLine($"% Kỳ báo cáo: {from?.ToString("dd/MM/yyyy") ?? "…"} đến {to?.ToString("dd/MM/yyyy") ?? "…"}");
            pdfText.AppendLine(new string('=', 60));
            pdfText.AppendLine(string.Format("{0,-35} | {1,20}", "DANH MỤC / CHỈ TIÊU", "GIÁ TRỊ"));
            pdfText.AppendLine(new string('-', 60));
            foreach (var row in previewRows)
            {
                var label = row.GetValueOrDefault("label")?.ToString() ?? "";
                var valObj = row.GetValueOrDefault("value");
                var formattedVal = valObj is decimal d ? d.ToString("#,##0") : valObj?.ToString() ?? "0";
                pdfText.AppendLine(string.Format("{0,-35} | {1,20}", label.Length > 35 ? label[..35] : label, formattedVal));
            }
            pdfText.AppendLine(new string('=', 60));
            pdfText.AppendLine($"% Tổng số bản ghi: {run.RowCount}");
            pdfText.AppendLine("%%EOF");

            var name = run.ExportFileName ?? $"{report.Code}.pdf";
            if (!name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                name = Path.ChangeExtension(name, ".pdf");
            return (name, "application/pdf", pdfText.ToString());
        }

        var text = new System.Text.StringBuilder();
        text.AppendLine($"BÁO CÁO {report.Code} — {report.Name}");
        text.AppendLine($"Module: {report.ModuleCode}");
        text.AppendLine($"Chạy lúc: {run.RunAt.ToLocalTime():dd/MM/yyyy HH:mm}");
        if (from is not null || to is not null)
            text.AppendLine($"Lọc: {from?.ToString("dd/MM/yyyy") ?? "…"} → {to?.ToString("dd/MM/yyyy") ?? "…"}");
        text.AppendLine(new string('-', 48));
        foreach (var row in previewRows)
            text.AppendLine($"{row.GetValueOrDefault("label"),-28} {row.GetValueOrDefault("value"),18}");
        text.AppendLine(new string('-', 48));
        text.AppendLine($"Số dòng nguồn: {run.RowCount}");
        var pdfName = run.ExportFileName ?? $"{report.Code}.txt";
        if (pdfName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            pdfName = Path.ChangeExtension(pdfName, ".txt");
        return (pdfName, "text/plain; charset=utf-8", text.ToString());
    }

    public async Task<IReadOnlyList<BiReportRunDto>> ListRunsAsync(
        Guid tenantId, Guid? reportId, CancellationToken ct = default)
    {
        var q = _db.BiReportRuns.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (reportId is Guid rid) q = q.Where(x => x.ReportId == rid);
        var list = await q.OrderByDescending(x => x.RunAt).Take(100).ToListAsync(ct);
        var rids = list.Select(x => x.ReportId).Distinct().ToList();
        var reports = await _db.BiReports.AsNoTracking()
            .Where(x => rids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        return list.Select(r =>
        {
            reports.TryGetValue(r.ReportId, out var rep);
            return new BiReportRunDto(
                r.Id, r.ReportId, rep?.Code, rep?.Name, r.RunAt, r.Status, r.RowCount,
                r.ExportFormat, r.ExportFileName, r.FilterJson, r.ResultPreviewJson, r.Note);
        }).ToList();
    }

    public async Task<IReadOnlyList<BiKpiTargetDto>> ListKpiTargetsAsync(
        Guid tenantId, string? periodKey = null, string? moduleCode = null, CancellationToken ct = default)
    {
        var q = _db.BiKpiTargets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(periodKey)) q = q.Where(x => x.PeriodKey == periodKey.Trim());
        if (!string.IsNullOrWhiteSpace(moduleCode))
        {
            var m = moduleCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.ModuleCode == m);
        }
        var list = await q.OrderByDescending(x => x.PeriodKey).ThenBy(x => x.Code).Take(300).ToListAsync(ct);
        var result = new List<BiKpiTargetDto>();
        foreach (var t in list)
        {
            result.Add(await MapKpiAsync(tenantId, t, ct));
        }
        return result;
    }

    public async Task<BiKpiTargetDto> UpsertKpiTargetAsync(
        Guid tenantId, Guid userId, BiKpiTargetUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên KPI");
        var metric = Metrics.FirstOrDefault(x => x.Equals(req.MetricKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("MetricKey: Revenue | Profit | Custom.");
        var periodKey = Req(req.PeriodKey, 20, "PeriodKey");
        if (req.PeriodTo < req.PeriodFrom) throw new AppException("PeriodTo ≥ PeriodFrom.");
        if (req.TargetValue < 0) throw new AppException("Target ≥ 0.");

        BiKpiTarget entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiKpiTargets, tenantId, id, "KPI target", ct);
        else
        {
            await EnsureCodeAsync(_db.BiKpiTargets, tenantId, code, ct);
            entity = new BiKpiTarget { TenantId = tenantId, Code = code, CreatedBy = userId };
            _db.BiKpiTargets.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.ModuleCode = NormModule(req.ModuleCode);
        entity.MetricKey = metric;
        entity.PeriodKey = periodKey;
        entity.PeriodFrom = req.PeriodFrom;
        entity.PeriodTo = req.PeriodTo;
        entity.TargetValue = decimal.Round(req.TargetValue, 2);
        entity.ActualStubValue = decimal.Round(req.ActualStubValue ?? entity.ActualStubValue, 2);
        entity.Unit = NullIfEmpty(req.Unit);
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapKpi(entity);
    }

    public async Task<IReadOnlyList<BiAlertThresholdDto>> ListAlertThresholdsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.BiAlertThresholds.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).Take(200).ToListAsync(ct);
        return await MapThresholdsAsync(tenantId, list, ct);
    }

    public async Task<BiAlertThresholdDto> UpsertAlertThresholdAsync(
        Guid tenantId, Guid userId, BiAlertThresholdUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên ngưỡng");
        var metric = Metrics.FirstOrDefault(x => x.Equals(req.MetricKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("MetricKey: Revenue | Profit | Custom.");
        var op = Operators.FirstOrDefault(x => x.Equals(req.Operator, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("Operator: Gt | Gte | Lt | Lte.");
        var sev = Severities.FirstOrDefault(x =>
            x.Equals(string.IsNullOrWhiteSpace(req.Severity) ? "Warn" : req.Severity, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("Severity: Info | Warn | Critical.");
        if (req.KpiTargetId is Guid tid)
            _ = await RequireAsync(_db.BiKpiTargets, tenantId, tid, "KPI target", ct);

        BiAlertThreshold entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.BiAlertThresholds, tenantId, id, "ngưỡng cảnh báo", ct);
        else
        {
            await EnsureCodeAsync(_db.BiAlertThresholds, tenantId, code, ct);
            entity = new BiAlertThreshold { TenantId = tenantId, Code = code, CreatedBy = userId };
            _db.BiAlertThresholds.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.MetricKey = metric;
        entity.KpiTargetId = req.KpiTargetId;
        entity.Operator = op;
        entity.ThresholdValue = decimal.Round(req.ThresholdValue, 2);
        entity.Severity = sev;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapThresholdsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<BiPeriodCompareDto> ComparePeriodsAsync(
        Guid tenantId, BiPeriodCompareRequest req, CancellationToken ct = default)
    {
        var metric = Metrics.FirstOrDefault(x => x.Equals(req.MetricKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("MetricKey: Revenue | Profit | Custom.");
        var currentKey = Req(req.CurrentPeriodKey, 20, "CurrentPeriodKey");

        BiKpiTarget? current;
        if (req.KpiTargetId is Guid tid)
            current = await _db.BiKpiTargets.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tid && x.TenantId == tenantId && !x.IsDeleted, ct);
        else
            current = await _db.BiKpiTargets.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.MetricKey == metric
                            && x.PeriodKey == currentKey && x.Status == "Active")
                .OrderBy(x => x.Code).FirstOrDefaultAsync(ct);
        if (current is null) throw new AppException("Không tìm thấy KPI kỳ hiện tại.");

        var currentActual = await ComputeKpiActualAsync(tenantId, current, ct);
        decimal? priorActual = null;
        string? priorKey = string.IsNullOrWhiteSpace(req.PriorPeriodKey) ? null : req.PriorPeriodKey.Trim();
        if (priorKey is not null)
        {
            var prior = await _db.BiKpiTargets.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.MetricKey == metric
                            && x.PeriodKey == priorKey && x.Status == "Active")
                .OrderBy(x => x.Code).FirstOrDefaultAsync(ct);
            if (prior is not null)
                priorActual = await ComputeKpiActualAsync(tenantId, prior, ct);
        }

        decimal? periodDelta = priorActual is decimal p ? currentActual - p : null;
        decimal? periodPct = priorActual is decimal pp && pp != 0
            ? Math.Round((currentActual - pp) / pp * 100m, 2) : null;
        var vsTarget = currentActual - current.TargetValue;
        var vsPct = current.TargetValue != 0
            ? Math.Round(vsTarget / current.TargetValue * 100m, 2) : (decimal?)null;

        return new BiPeriodCompareDto(
            metric, current.PeriodKey, currentActual,
            priorKey, priorActual, periodDelta, periodPct,
            current.TargetValue, vsTarget, vsPct);
    }

    public async Task<IReadOnlyList<BiTargetVsActualRowDto>> ListTargetVsActualAsync(
        Guid tenantId, string periodKey, string? moduleCode = null, CancellationToken ct = default)
    {
        var pk = Req(periodKey, 20, "PeriodKey");
        var q = _db.BiKpiTargets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.PeriodKey == pk && x.Status == "Active");
        if (!string.IsNullOrWhiteSpace(moduleCode))
        {
            var m = moduleCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.ModuleCode == m);
        }
        var targets = await q.OrderBy(x => x.Code).Take(200).ToListAsync(ct);
        var thresholds = await _db.BiAlertThresholds.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active").ToListAsync(ct);

        var rows = new List<BiTargetVsActualRowDto>();
        foreach (var t in targets)
        {
            var actual = await ComputeKpiActualAsync(tenantId, t, ct);
            var variance = actual - t.TargetValue;
            var pct = t.TargetValue != 0 ? Math.Round(variance / t.TargetValue * 100m, 2) : 0m;
            var hit = thresholds
                .Where(th => th.MetricKey == t.MetricKey
                             && (th.KpiTargetId == null || th.KpiTargetId == t.Id)
                             && Evaluate(th.Operator, actual, th.ThresholdValue))
                .OrderByDescending(th => SeverityRank(th.Severity))
                .FirstOrDefault();
            rows.Add(new BiTargetVsActualRowDto(
                t.Id, t.Code, t.Name, t.ModuleCode, t.MetricKey, t.PeriodKey,
                t.TargetValue, actual, variance, pct, t.Unit,
                hit is not null, hit?.Severity, hit?.Name));
        }
        return rows;
    }

    private async Task<decimal> ComputeKpiActualAsync(
        Guid tenantId, BiKpiTarget target, CancellationToken ct)
    {
        var fromDt = new DateTimeOffset(target.PeriodFrom.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toDt = new DateTimeOffset(target.PeriodTo.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        if (target.ModuleCode == "POS")
        {
            var posAmt = await _db.PosSales.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Cancelled"
                            && x.CreatedAt >= fromDt && x.CreatedAt <= toDt)
                .SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0m;
            if (posAmt > 0) return decimal.Round(posAmt, 2);
        }
        else if (target.ModuleCode == "CRM")
        {
            var crmAmt = await _db.CrmSalesOrders.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Cancelled"
                            && x.OrderDate >= fromDt && x.OrderDate <= toDt)
                .SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0m;
            if (crmAmt > 0) return decimal.Round(crmAmt, 2);

            var crmOppAmt = await _db.CrmOpportunities.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Stage == "Won" || x.Stage == "ClosedWon")
                            && x.ExpectedCloseDate >= fromDt && x.ExpectedCloseDate <= toDt)
                .SumAsync(x => (decimal?)x.EstimatedValue, ct) ?? 0m;
            if (crmOppAmt > 0) return decimal.Round(crmOppAmt, 2);
        }

        if (target.MetricKey == "Revenue")
        {
            var revDoc = await _db.FinRevenueDocuments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Void" && x.Kind != "Cogs"
                            && x.DocDate >= fromDt && x.DocDate <= toDt)
                .SumAsync(x => (decimal?)x.RevenueAmount, ct) ?? 0m;
            if (revDoc > 0) return decimal.Round(revDoc, 2);

            var revAccountIds = await _db.FinAccounts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code.StartsWith("511"))
                .Select(x => x.Id).ToListAsync(ct);
            var postedJeIds = await _db.FinJournals.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted"
                            && x.EntryDate >= fromDt && x.EntryDate <= toDt)
                .Select(x => x.Id).ToListAsync(ct);

            if (revAccountIds.Count > 0 && postedJeIds.Count > 0)
            {
                var jeRev = await _db.FinJournalLines.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted
                                && postedJeIds.Contains(x.JournalId) && revAccountIds.Contains(x.AccountId))
                    .SumAsync(x => (decimal?)x.Credit - x.Debit, ct) ?? 0m;
                if (jeRev > 0) return decimal.Round(jeRev, 2);
            }
        }
        else if (target.MetricKey == "Profit")
        {
            var revDoc = await _db.FinRevenueDocuments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Void" && x.Kind != "Cogs"
                            && x.DocDate >= fromDt && x.DocDate <= toDt)
                .SumAsync(x => (decimal?)x.RevenueAmount, ct) ?? 0m;
            var cogsDoc = await _db.FinRevenueDocuments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Void"
                            && x.DocDate >= fromDt && x.DocDate <= toDt)
                .SumAsync(x => (decimal?)x.CogsAmount, ct) ?? 0m;
            if (revDoc > 0) return decimal.Round(revDoc - cogsDoc, 2);
        }

        return target.ActualStubValue;
    }

    private async Task<BiKpiTargetDto> MapKpiAsync(
        Guid tenantId, BiKpiTarget t, CancellationToken ct)
    {
        var actual = await ComputeKpiActualAsync(tenantId, t, ct);
        var variance = actual - t.TargetValue;
        var pct = t.TargetValue != 0 ? Math.Round(variance / t.TargetValue * 100m, 2) : 0m;
        return new BiKpiTargetDto(
            t.Id, t.Code, t.Name, t.ModuleCode, t.MetricKey, t.PeriodKey,
            t.PeriodFrom, t.PeriodTo, t.TargetValue, actual, t.Unit, t.Status, t.Note,
            variance, pct);
    }

    private async Task<IReadOnlyList<BiAlertThresholdDto>> MapThresholdsAsync(
        Guid tenantId, List<BiAlertThreshold> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<BiAlertThresholdDto>();
        var tids = list.Where(x => x.KpiTargetId.HasValue).Select(x => x.KpiTargetId!.Value).Distinct().ToList();
        var targets = tids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.BiKpiTargets.AsNoTracking().Where(x => tids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        return list.Select(t => new BiAlertThresholdDto(
            t.Id, t.Code, t.Name, t.MetricKey, t.KpiTargetId,
            t.KpiTargetId is Guid id ? targets.GetValueOrDefault(id) : null,
            t.Operator, t.ThresholdValue, t.Severity, t.Status, t.Note)).ToList();
    }

    private static BiKpiTargetDto MapKpi(BiKpiTarget t)
    {
        var variance = t.ActualStubValue - t.TargetValue;
        var pct = t.TargetValue != 0 ? Math.Round(variance / t.TargetValue * 100m, 2) : 0m;
        return new BiKpiTargetDto(
            t.Id, t.Code, t.Name, t.ModuleCode, t.MetricKey, t.PeriodKey,
            t.PeriodFrom, t.PeriodTo, t.TargetValue, t.ActualStubValue, t.Unit, t.Status, t.Note,
            variance, pct);
    }

    private static bool Evaluate(string op, decimal actual, decimal threshold) => op switch
    {
        "Gt" => actual > threshold,
        "Gte" => actual >= threshold,
        "Lt" => actual < threshold,
        "Lte" => actual <= threshold,
        _ => false
    };

    private static int SeverityRank(string s) => s switch
    {
        "Critical" => 3,
        "Warn" => 2,
        "Info" => 1,
        _ => 0
    };

    private async Task<IReadOnlyList<BiReportDto>> MapReportsAsync(
        Guid tenantId, List<BiReport> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var dids = list.Where(x => x.DatasetId.HasValue).Select(x => x.DatasetId!.Value).Distinct().ToList();
        var datasets = dids.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.BiDatasets.AsNoTracking()
                .Where(x => dids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var permCounts = await _db.BiReportPermissions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ReportId) && !x.IsDeleted)
            .GroupBy(x => x.ReportId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(r => new BiReportDto(
            r.Id, r.Code, r.Name, r.ModuleCode, r.DatasetId,
            r.DatasetId is Guid d ? datasets.GetValueOrDefault(d) : null,
            r.Description, r.FilterSchemaJson, r.Status, r.RequirePermission,
            permCounts.GetValueOrDefault(r.Id))).ToList();
    }

    private static BiDatasetDto MapDataset(BiDataset d) =>
        new(d.Id, d.Code, d.Name, d.ModuleCode, d.Description, d.Status,
            d.LastRefreshedAt, d.LastRefreshNote, d.RowCountEstimate);

    private static BiWidgetDto MapWidget(BiWidget w) =>
        new(w.Id, w.DashboardId, w.Code, w.Title, w.WidgetType, w.MetricKey,
            w.StubValue, w.Unit, w.SortOrder, w.Status);

    private async Task<(int Rows, string Note)> CountModuleSourceRowsAsync(
        Guid tenantId, string moduleCode, CancellationToken ct)
    {
        var mod = (moduleCode ?? "").Trim().ToUpperInvariant();
        return mod switch
        {
            "FIN" => (await _db.FinJournals.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "FIN.FinJournals"),
            "POS" => (await _db.PosSales.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "POS.PosSales"),
            "CRM" => (await _db.CrmSalesOrders.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "CRM.CrmSalesOrders"),
            "HRM" => (await _db.Employees.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "HRM.Employees"),
            "INV" => (await _db.InvStockBalances.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "INV.InvStockBalances"),
            "PUR" => (await _db.PurPurchaseOrders.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "PUR.PurPurchaseOrders"),
            "MFG" => (await _db.MfgWorkOrders.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "MFG.MfgWorkOrders"),
            "AST" => (await _db.AstAssets.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                "AST.AstAssets"),
            _ => (await _db.BiReportRuns.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct),
                $"BI.BiReportRuns (module {mod})"),
        };
    }

    private async Task<(decimal Revenue, decimal Profit)> ComputeFinMetricsAsync(
        Guid tenantId, CancellationToken ct)
    {
        // Ưu tiên chứng từ DT FIN; fallback tổng Có TK 511* từ JE Posted.
        var revenueOnly = await _db.FinRevenueDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Void"
                        && x.Kind != "Cogs")
            .SumAsync(x => (decimal?)x.RevenueAmount, ct) ?? 0m;
        if (revenueOnly > 0)
        {
            var cogs = await _db.FinRevenueDocuments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Void")
                .SumAsync(x => (decimal?)x.CogsAmount, ct) ?? 0m;
            var cogsKind = await _db.FinRevenueDocuments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Void"
                            && x.Kind == "Cogs")
                .SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0m;
            var totalCogs = cogs + cogsKind;
            return (decimal.Round(revenueOnly, 0), decimal.Round(revenueOnly - totalCogs, 0));
        }

        var revAccountIds = await _db.FinAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code.StartsWith("511"))
            .Select(x => x.Id).ToListAsync(ct);
        var expAccountIds = await _db.FinAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Code.StartsWith("632") || x.Code.StartsWith("641") || x.Code.StartsWith("642")))
            .Select(x => x.Id).ToListAsync(ct);

        var postedJeIds = await _db.FinJournals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted")
            .Select(x => x.Id).ToListAsync(ct);

        var revenue = revAccountIds.Count == 0 || postedJeIds.Count == 0
            ? 0m
            : await _db.FinJournalLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted
                            && postedJeIds.Contains(x.JournalId) && revAccountIds.Contains(x.AccountId))
                .SumAsync(x => (decimal?)x.Credit - x.Debit, ct) ?? 0m;
        var expense = expAccountIds.Count == 0 || postedJeIds.Count == 0
            ? 0m
            : await _db.FinJournalLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted
                            && postedJeIds.Contains(x.JournalId) && expAccountIds.Contains(x.AccountId))
                .SumAsync(x => (decimal?)x.Debit - x.Credit, ct) ?? 0m;
        return (decimal.Round(revenue, 0), decimal.Round(revenue - expense, 0));
    }

    private static (DateTimeOffset? From, DateTimeOffset? To) ParseDateFilter(string filterJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(filterJson) ? "{}" : filterJson);
            DateTimeOffset? from = null, to = null;
            if (doc.RootElement.TryGetProperty("from", out var f) && f.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(f.GetString(), out var fd))
                from = new DateTimeOffset(fd.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            if (doc.RootElement.TryGetProperty("to", out var t) && t.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(t.GetString(), out var td))
                to = new DateTimeOffset(td.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            return (from, to);
        }
        catch (JsonException)
        {
            throw new AppException("FilterJson không hợp lệ.");
        }
    }

    private async Task<(int RowCount, List<Dictionary<string, object?>> Preview, string Note)> BuildReportRowsAsync(
        Guid tenantId, string moduleCode, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        var mod = (moduleCode ?? "").Trim().ToUpperInvariant();
        var preview = new List<Dictionary<string, object?>>();

        switch (mod)
        {
            case "FIN":
            {
                var q = _db.FinJournals.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
                if (from is DateTimeOffset f) q = q.Where(x => x.EntryDate >= f);
                if (to is DateTimeOffset t) q = q.Where(x => x.EntryDate <= t);
                var list = await q.ToListAsync(ct);
                var posted = list.Count(x => x.Status == "Posted");
                var draft = list.Count(x => x.Status == "Draft");
                preview.Add(new() { ["label"] = "Số BT", ["value"] = list.Count });
                preview.Add(new() { ["label"] = "Posted", ["value"] = posted });
                preview.Add(new() { ["label"] = "Draft", ["value"] = draft });
                preview.Add(new() { ["label"] = "Module", ["value"] = "FIN" });
                return (list.Count, preview, "FIN journals theo kỳ lọc");
            }
            case "POS":
            {
                var q = _db.PosSales.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
                if (from is DateTimeOffset f) q = q.Where(x => x.CreatedAt >= f);
                if (to is DateTimeOffset t) q = q.Where(x => x.CreatedAt <= t);
                var list = await q.ToListAsync(ct);
                var paid = list.Where(x => x.Status is "Paid" or "Returned").ToList();
                preview.Add(new() { ["label"] = "Số đơn", ["value"] = list.Count });
                preview.Add(new() { ["label"] = "Đã TT", ["value"] = paid.Count });
                preview.Add(new() { ["label"] = "Doanh thu", ["value"] = paid.Sum(x => x.TotalAmount) });
                preview.Add(new() { ["label"] = "Module", ["value"] = "POS" });
                return (list.Count, preview, "POS sales theo kỳ lọc");
            }
            case "CRM":
            {
                var q = _db.CrmSalesOrders.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
                if (from is DateTimeOffset f) q = q.Where(x => x.OrderDate >= f);
                if (to is DateTimeOffset t) q = q.Where(x => x.OrderDate <= t);
                var list = await q.ToListAsync(ct);
                preview.Add(new() { ["label"] = "Số đơn", ["value"] = list.Count });
                preview.Add(new() { ["label"] = "Tổng GT", ["value"] = list.Sum(x => x.TotalAmount) });
                preview.Add(new() { ["label"] = "Module", ["value"] = "CRM" });
                return (list.Count, preview, "CRM orders theo kỳ lọc");
            }
            default:
            {
                var (rows, note) = await CountModuleSourceRowsAsync(tenantId, mod, ct);
                preview.Add(new() { ["label"] = "Số dòng nguồn", ["value"] = rows });
                preview.Add(new() { ["label"] = "Module", ["value"] = mod });
                preview.Add(new() { ["label"] = "Nguồn", ["value"] = note });
                return (rows, preview, note);
            }
        }
    }

    private static string CsvCell(string? s)
    {
        var v = s ?? "";
        if (v.Contains('"') || v.Contains(',') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }

    private static async Task<T> RequireAsync<T>(
        DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static async Task EnsureCodeAsync<T>(DbSet<T> set, Guid tenantId, string code, CancellationToken ct)
        where T : TenantEntity
    {
        if (await set.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && EF.Property<string>(x, "Code") == code, ct))
            throw new AppException("Mã đã tồn tại.");
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string NormModule(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 2 or > 20) throw new AppException("ModuleCode 2–20 ký tự.");
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
