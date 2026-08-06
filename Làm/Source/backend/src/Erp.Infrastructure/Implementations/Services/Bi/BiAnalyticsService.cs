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

        // Cap-1 stub: không ETL thật — giả lập refresh
        var rows = Random.Shared.Next(50, 5000);
        log.Status = "Succeeded";
        log.FinishedAt = DateTimeOffset.UtcNow;
        log.RowsAffected = rows;
        log.Note ??= "Refresh stub thành công";
        log.UpdatedBy = userId;

        ds.Status = "Ready";
        ds.LastRefreshedAt = log.FinishedAt;
        ds.LastRefreshNote = log.Note;
        ds.RowCountEstimate = rows;
        ds.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapDataset(ds);
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
        return new BiDashboardDetailDto(dto, widgets.Select(MapWidget).ToList());
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

        // Stub rows from dataset estimate or random
        var rows = 25;
        if (report.DatasetId is Guid did)
        {
            var ds = await _db.BiDatasets.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == did && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (ds is not null) rows = Math.Min(100, Math.Max(5, ds.RowCountEstimate / 10));
        }

        var preview = JsonSerializer.Serialize(new[]
        {
            new Dictionary<string, object?> { ["label"] = "Tổng", ["value"] = rows * 1_000_000 },
            new Dictionary<string, object?> { ["label"] = "Số dòng", ["value"] = rows },
            new Dictionary<string, object?> { ["label"] = "Module", ["value"] = report.ModuleCode },
        });

        var fileName = export == "None"
            ? null
            : $"{report.Code}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{(export == "Excel" ? "xlsx" : "pdf")}";

        var run = new BiReportRun
        {
            TenantId = tenantId, ReportId = reportId,
            RunAt = DateTimeOffset.UtcNow, RunByUserId = userId,
            FilterJson = NullIfEmpty(req.FilterJson) ?? "{}",
            Status = "Succeeded", RowCount = rows,
            ExportFormat = export, ExportFileName = fileName,
            ResultPreviewJson = preview,
            Note = export == "None" ? "Chạy stub" : $"Xuất {export} stub (file metadata only)",
            CreatedBy = userId
        };
        _db.BiReportRuns.Add(run);
        await _db.SaveChangesAsync(ct);
        return new BiReportRunDto(
            run.Id, run.ReportId, report.Code, report.Name, run.RunAt, run.Status,
            run.RowCount, run.ExportFormat, run.ExportFileName, run.FilterJson,
            run.ResultPreviewJson, run.Note);
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
        return list.Select(MapKpi).ToList();
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

        decimal? priorActual = null;
        string? priorKey = string.IsNullOrWhiteSpace(req.PriorPeriodKey) ? null : req.PriorPeriodKey.Trim();
        if (priorKey is not null)
        {
            var prior = await _db.BiKpiTargets.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.MetricKey == metric
                            && x.PeriodKey == priorKey && x.Status == "Active")
                .OrderBy(x => x.Code).FirstOrDefaultAsync(ct);
            priorActual = prior?.ActualStubValue;
        }

        decimal? periodDelta = priorActual is decimal p ? current.ActualStubValue - p : null;
        decimal? periodPct = priorActual is decimal pp && pp != 0
            ? Math.Round((current.ActualStubValue - pp) / pp * 100m, 2) : null;
        var vsTarget = current.ActualStubValue - current.TargetValue;
        var vsPct = current.TargetValue != 0
            ? Math.Round(vsTarget / current.TargetValue * 100m, 2) : (decimal?)null;

        return new BiPeriodCompareDto(
            metric, current.PeriodKey, current.ActualStubValue,
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

        return targets.Select(t =>
        {
            var variance = t.ActualStubValue - t.TargetValue;
            var pct = t.TargetValue != 0 ? Math.Round(variance / t.TargetValue * 100m, 2) : 0m;
            var hit = thresholds
                .Where(th => th.MetricKey == t.MetricKey
                             && (th.KpiTargetId == null || th.KpiTargetId == t.Id)
                             && Evaluate(th.Operator, t.ActualStubValue, th.ThresholdValue))
                .OrderByDescending(th => SeverityRank(th.Severity))
                .FirstOrDefault();
            return new BiTargetVsActualRowDto(
                t.Id, t.Code, t.Name, t.ModuleCode, t.MetricKey, t.PeriodKey,
                t.TargetValue, t.ActualStubValue, variance, pct, t.Unit,
                hit is not null, hit?.Severity, hit?.Name);
        }).ToList();
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
