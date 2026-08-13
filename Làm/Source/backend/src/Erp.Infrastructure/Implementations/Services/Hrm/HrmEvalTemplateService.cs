using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmEvalTemplateService : IHrmEvalTemplateService
{
    private readonly AppDbContext _db;

    public HrmEvalTemplateService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_177: Mẫu đánh giá KPI / năng lực
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmKpiTemplateDto>> GetKpiTemplatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var items = await _db.HrmKpiTemplates.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

        return items.Select(t => new HrmKpiTemplateDto(
            t.Id, t.Code, t.Title, t.TargetRole, t.CriteriaDescription, t.MaxScore, t.WeightPercentage, t.CreatedAt
        )).ToList();
    }

    public async Task<HrmKpiTemplateDto> GetKpiTemplateByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var item = await _db.HrmKpiTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);
        if (item == null) throw new AppException($"Không tìm thấy mẫu KPI {id}", 404);
        return new HrmKpiTemplateDto(item.Id, item.Code, item.Title, item.TargetRole, item.CriteriaDescription, item.MaxScore, item.WeightPercentage, item.CreatedAt);
    }

    public async Task<HrmKpiTemplateDto> CreateKpiTemplateAsync(Guid tenantId, HrmKpiTemplateUpsertRequest req, CancellationToken ct = default)
    {
        ValidateKpiTemplateRequest(req);

        var exists = await _db.HrmKpiTemplates.AnyAsync(t => t.TenantId == tenantId && t.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (exists) throw new AppException($"Mã mẫu KPI {req.Code} đã tồn tại.");

        var entity = new HrmKpiTemplate
        {
            TenantId = tenantId,
            Code = req.Code.Trim().ToUpper(),
            Title = req.Title.Trim(),
            TargetRole = req.TargetRole?.Trim(),
            CriteriaDescription = req.CriteriaDescription.Trim(),
            MaxScore = req.MaxScore,
            WeightPercentage = req.WeightPercentage
        };

        _db.HrmKpiTemplates.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetKpiTemplateByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmKpiTemplateDto> UpdateKpiTemplateAsync(Guid tenantId, Guid id, HrmKpiTemplateUpsertRequest req, CancellationToken ct = default)
    {
        ValidateKpiTemplateRequest(req);

        var entity = await _db.HrmKpiTemplates.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy mẫu KPI {id}", 404);

        var dup = await _db.HrmKpiTemplates.AnyAsync(t => t.TenantId == tenantId && t.Id != id && t.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (dup) throw new AppException($"Mã mẫu KPI {req.Code} đã tồn tại.");

        entity.Code = req.Code.Trim().ToUpper();
        entity.Title = req.Title.Trim();
        entity.TargetRole = req.TargetRole?.Trim();
        entity.CriteriaDescription = req.CriteriaDescription.Trim();
        entity.MaxScore = req.MaxScore;
        entity.WeightPercentage = req.WeightPercentage;

        await _db.SaveChangesAsync(ct);
        return await GetKpiTemplateByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteKpiTemplateAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HrmKpiTemplates.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy mẫu KPI {id}", 404);

        _db.HrmKpiTemplates.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateKpiTemplateRequest(HrmKpiTemplateUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) throw new AppException("Mã mẫu KPI không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new AppException("Tên mẫu KPI không được để trống.");
        if (req.MaxScore <= 0m) throw new AppException("Điểm tối đa phải lớn hơn 0.");
        if (req.WeightPercentage <= 0m) throw new AppException("Tỷ trọng % phải lớn hơn 0.");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_178: Tạo kỳ đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmEvaluationCycleDto>> GetEvaluationCyclesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var items = await _db.HrmEvaluationCycles.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);

        var templateIds = items.Where(c => c.KpiTemplateId.HasValue).Select(c => c.KpiTemplateId!.Value).Distinct().ToList();
        var templates = await _db.HrmKpiTemplates.AsNoTracking()
            .Where(t => t.TenantId == tenantId && templateIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Title, ct);

        return items.Select(c => new HrmEvaluationCycleDto(
            c.Id,
            c.CycleName,
            c.PeriodKey,
            c.StartDate,
            c.EndDate,
            c.KpiTemplateId,
            c.KpiTemplateId.HasValue && templates.TryGetValue(c.KpiTemplateId.Value, out var title) ? title : null,
            c.Status,
            c.CreatedAt
        )).ToList();
    }

    public async Task<HrmEvaluationCycleDto> GetEvaluationCycleByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetEvaluationCyclesAsync(tenantId, ct);
        var item = list.FirstOrDefault(c => c.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy kỳ đánh giá {id}", 404);
        return item;
    }

    public async Task<HrmEvaluationCycleDto> CreateEvaluationCycleAsync(Guid tenantId, HrmEvaluationCycleUpsertRequest req, CancellationToken ct = default)
    {
        ValidateEvaluationCycleRequest(req);

        if (req.KpiTemplateId.HasValue && req.KpiTemplateId.Value != Guid.Empty)
        {
            var tmplExists = await _db.HrmKpiTemplates.AnyAsync(t => t.TenantId == tenantId && t.Id == req.KpiTemplateId.Value, ct);
            if (!tmplExists) throw new AppException($"Không tìm thấy mẫu KPI {req.KpiTemplateId.Value}.", 404);
        }

        var entity = new HrmEvaluationCycle
        {
            TenantId = tenantId,
            CycleName = req.CycleName.Trim(),
            PeriodKey = req.PeriodKey.Trim(),
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            KpiTemplateId = req.KpiTemplateId,
            Status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim()
        };

        _db.HrmEvaluationCycles.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetEvaluationCycleByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmEvaluationCycleDto> UpdateEvaluationCycleAsync(Guid tenantId, Guid id, HrmEvaluationCycleUpsertRequest req, CancellationToken ct = default)
    {
        ValidateEvaluationCycleRequest(req);

        var entity = await _db.HrmEvaluationCycles.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy kỳ đánh giá {id}", 404);

        if (req.KpiTemplateId.HasValue && req.KpiTemplateId.Value != Guid.Empty)
        {
            var tmplExists = await _db.HrmKpiTemplates.AnyAsync(t => t.TenantId == tenantId && t.Id == req.KpiTemplateId.Value, ct);
            if (!tmplExists) throw new AppException($"Không tìm thấy mẫu KPI {req.KpiTemplateId.Value}.", 404);
        }

        entity.CycleName = req.CycleName.Trim();
        entity.PeriodKey = req.PeriodKey.Trim();
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        entity.KpiTemplateId = req.KpiTemplateId;
        entity.Status = string.IsNullOrWhiteSpace(req.Status) ? entity.Status : req.Status.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetEvaluationCycleByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteEvaluationCycleAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HrmEvaluationCycles.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy kỳ đánh giá {id}", 404);

        _db.HrmEvaluationCycles.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateEvaluationCycleRequest(HrmEvaluationCycleUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.CycleName)) throw new AppException("Tên kỳ đánh giá không được để trống.");
        if (string.IsNullOrWhiteSpace(req.PeriodKey)) throw new AppException("Mã kỳ đánh giá không được để trống.");
        if (req.StartDate > req.EndDate) throw new AppException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_179: Quản lý đánh giá nhân viên
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmManagerEvaluationDto>> GetManagerEvaluationsAsync(Guid tenantId, Guid? cycleId = null, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _db.HrmManagerEvaluations.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (cycleId.HasValue && cycleId.Value != Guid.Empty) query = query.Where(e => e.EvaluationCycleId == cycleId.Value);
        if (employeeId.HasValue && employeeId.Value != Guid.Empty) query = query.Where(e => e.EmployeeId == employeeId.Value);

        var items = await query.OrderByDescending(e => e.CreatedAt).ToListAsync(ct);

        var cycleIds = items.Select(e => e.EvaluationCycleId).Distinct().ToList();
        var cycles = await _db.HrmEvaluationCycles.AsNoTracking()
            .Where(c => c.TenantId == tenantId && cycleIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.CycleName, ct);

        var empIds = items.Select(e => e.EmployeeId).Concat(items.Select(e => e.EvaluatorId)).Distinct().ToList();
        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => $"{e.EmployeeCode} - {e.FullName}", ct);

        return items.Select(e => new HrmManagerEvaluationDto(
            e.Id,
            e.EvaluationCycleId,
            cycles.TryGetValue(e.EvaluationCycleId, out var cycleName) ? cycleName : null,
            e.EmployeeId,
            employees.TryGetValue(e.EmployeeId, out var empName) ? empName : null,
            e.EvaluatorId,
            employees.TryGetValue(e.EvaluatorId, out var evalName) ? evalName : null,
            e.KpiScore,
            e.CompetencyScore,
            e.FinalGrade,
            e.ManagerComments,
            e.Status,
            e.CreatedAt
        )).ToList();
    }

    public async Task<HrmManagerEvaluationDto> GetManagerEvaluationByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetManagerEvaluationsAsync(tenantId, null, null, ct);
        var item = list.FirstOrDefault(e => e.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy phiếu đánh giá {id}", 404);
        return item;
    }

    public async Task<HrmManagerEvaluationDto> CreateManagerEvaluationAsync(Guid tenantId, HrmManagerEvaluationUpsertRequest req, CancellationToken ct = default)
    {
        ValidateManagerEvaluationRequest(req);

        var cycleExists = await _db.HrmEvaluationCycles.AnyAsync(c => c.TenantId == tenantId && c.Id == req.EvaluationCycleId, ct);
        if (!cycleExists) throw new AppException($"Không tìm thấy kỳ đánh giá {req.EvaluationCycleId}.", 404);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự được đánh giá {req.EmployeeId}.", 404);

        var entity = new HrmManagerEvaluation
        {
            TenantId = tenantId,
            EvaluationCycleId = req.EvaluationCycleId,
            EmployeeId = req.EmployeeId,
            EvaluatorId = req.EvaluatorId,
            KpiScore = req.KpiScore,
            CompetencyScore = req.CompetencyScore,
            FinalGrade = NormalizeGrade(req.FinalGrade),
            ManagerComments = req.ManagerComments?.Trim(),
            Status = string.IsNullOrWhiteSpace(req.Status) ? "Pending" : req.Status.Trim()
        };

        _db.HrmManagerEvaluations.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetManagerEvaluationByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmManagerEvaluationDto> UpdateManagerEvaluationAsync(Guid tenantId, Guid id, HrmManagerEvaluationUpsertRequest req, CancellationToken ct = default)
    {
        ValidateManagerEvaluationRequest(req);

        var entity = await _db.HrmManagerEvaluations.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy phiếu đánh giá {id}", 404);

        entity.KpiScore = req.KpiScore;
        entity.CompetencyScore = req.CompetencyScore;
        entity.FinalGrade = NormalizeGrade(req.FinalGrade);
        entity.ManagerComments = req.ManagerComments?.Trim();
        entity.Status = string.IsNullOrWhiteSpace(req.Status) ? entity.Status : req.Status.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetManagerEvaluationByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteManagerEvaluationAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HrmManagerEvaluations.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy phiếu đánh giá {id}", 404);

        _db.HrmManagerEvaluations.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateManagerEvaluationRequest(HrmManagerEvaluationUpsertRequest req)
    {
        if (req.EvaluationCycleId == Guid.Empty) throw new AppException("Chưa chọn kỳ đánh giá.");
        if (req.EmployeeId == Guid.Empty) throw new AppException("Chưa chọn nhân sự được đánh giá.");
        if (req.KpiScore < 0m || req.CompetencyScore < 0m) throw new AppException("Điểm số đánh giá phải lớn hơn hoặc bằng 0.");
    }

    private static string NormalizeGrade(string grade)
    {
        var valid = new[] { "A", "B", "C", "D" };
        var found = valid.FirstOrDefault(v => string.Equals(v, grade, StringComparison.OrdinalIgnoreCase));
        return found ?? "B";
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_180: Nhân viên tự đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmSelfEvaluationDto>> GetSelfEvaluationsAsync(Guid tenantId, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _db.HrmSelfEvaluations.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (employeeId.HasValue && employeeId.Value != Guid.Empty) query = query.Where(e => e.EmployeeId == employeeId.Value);

        var items = await query.OrderByDescending(e => e.CreatedAt).ToListAsync(ct);

        var empIds = items.Select(e => e.EmployeeId).Distinct().ToList();
        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => $"{e.EmployeeCode} - {e.FullName}", ct);

        return items.Select(e => new HrmSelfEvaluationDto(
            e.Id,
            e.EmployeeId,
            employees.TryGetValue(e.EmployeeId, out var empName) ? empName : null,
            e.AppraisalPeriod,
            e.KeyAchievements,
            e.AreasForImprovement,
            e.SelfRating,
            e.Status,
            e.CreatedAt
        )).ToList();
    }

    public async Task<HrmSelfEvaluationDto> GetSelfEvaluationByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetSelfEvaluationsAsync(tenantId, null, ct);
        var item = list.FirstOrDefault(e => e.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy phiếu tự đánh giá {id}", 404);
        return item;
    }

    public async Task<HrmSelfEvaluationDto> CreateSelfEvaluationAsync(Guid tenantId, HrmSelfEvaluationUpsertRequest req, CancellationToken ct = default)
    {
        ValidateSelfEvaluationRequest(req);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        var entity = new HrmSelfEvaluation
        {
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            AppraisalPeriod = req.AppraisalPeriod.Trim(),
            KeyAchievements = req.KeyAchievements.Trim(),
            AreasForImprovement = req.AreasForImprovement.Trim(),
            SelfRating = Math.Clamp(req.SelfRating, 1, 5),
            Status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim()
        };

        _db.HrmSelfEvaluations.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetSelfEvaluationByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmSelfEvaluationDto> UpdateSelfEvaluationAsync(Guid tenantId, Guid id, HrmSelfEvaluationUpsertRequest req, CancellationToken ct = default)
    {
        ValidateSelfEvaluationRequest(req);

        var entity = await _db.HrmSelfEvaluations.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy phiếu tự đánh giá {id}", 404);

        entity.AppraisalPeriod = req.AppraisalPeriod.Trim();
        entity.KeyAchievements = req.KeyAchievements.Trim();
        entity.AreasForImprovement = req.AreasForImprovement.Trim();
        entity.SelfRating = Math.Clamp(req.SelfRating, 1, 5);
        entity.Status = string.IsNullOrWhiteSpace(req.Status) ? entity.Status : req.Status.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetSelfEvaluationByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteSelfEvaluationAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HrmSelfEvaluations.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy phiếu tự đánh giá {id}", 404);

        _db.HrmSelfEvaluations.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateSelfEvaluationRequest(HrmSelfEvaluationUpsertRequest req)
    {
        if (req.EmployeeId == Guid.Empty) throw new AppException("Nhân sự không được để trống.");
        if (string.IsNullOrWhiteSpace(req.AppraisalPeriod)) throw new AppException("Kỳ tự đánh giá không được để trống.");
        if (req.SelfRating < 1 || req.SelfRating > 5) throw new AppException("Điểm tự đánh giá phải từ 1 đến 5 sao.");
    }
}
