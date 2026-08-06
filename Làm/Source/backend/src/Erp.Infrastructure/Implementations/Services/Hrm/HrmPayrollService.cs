using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmPayrollService : IHrmPayrollService
{
    private readonly AppDbContext _db;

    public HrmPayrollService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SalaryGradeDto>> ListGradesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.SalaryGrades.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Level).ThenBy(x => x.Code)
            .Select(x => new SalaryGradeDto(x.Id, x.Code, x.Name, x.Level, x.BaseAmount, x.IsActive, x.Note))
            .ToListAsync(ct);

    public async Task<SalaryGradeDto> UpsertGradeAsync(
        Guid tenantId, Guid userId, SalaryGradeUpsertRequest req, CancellationToken ct = default)
    {
        var code = req.Code.Trim().ToUpperInvariant();
        var name = req.Name.Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã bậc 1–40 ký tự.");
        if (name.Length is < 1 or > 100) throw new AppException("Tên bậc 1–100 ký tự.");
        if (req.BaseAmount < 0) throw new AppException("Mức lương cơ bản không hợp lệ.");

        SalaryGrade entity;
        if (req.Id is Guid id)
        {
            entity = await _db.SalaryGrades.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Bậc lương không tồn tại.", 404);
            if (await _db.SalaryGrades.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code && x.Id != id, ct))
                throw new AppException("Mã bậc đã tồn tại.");
        }
        else
        {
            if (await _db.SalaryGrades.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code, ct))
                throw new AppException("Mã bậc đã tồn tại.");
            entity = new SalaryGrade { Id = Guid.NewGuid(), TenantId = tenantId, CreatedBy = userId };
            _db.SalaryGrades.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.Level = req.Level;
        entity.BaseAmount = req.BaseAmount;
        entity.IsActive = req.IsActive;
        entity.Note = req.Note;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new SalaryGradeDto(entity.Id, entity.Code, entity.Name, entity.Level, entity.BaseAmount, entity.IsActive, entity.Note);
    }

    public async Task<IReadOnlyList<EmployeeSalaryDto>> ListEmployeeSalariesAsync(
        Guid tenantId, Guid? employeeId, CancellationToken ct = default)
    {
        var q =
            from s in _db.EmployeeSalaries.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on s.EmployeeId equals e.Id
            join g in _db.SalaryGrades.AsNoTracking() on s.SalaryGradeId equals g.Id into gj
            from g in gj.DefaultIfEmpty()
            where s.TenantId == tenantId && !s.IsDeleted && !e.IsDeleted
            select new { s, e, GradeName = g != null ? g.Name : null };

        if (employeeId is Guid eid) q = q.Where(x => x.s.EmployeeId == eid);

        return await q.OrderByDescending(x => x.s.EffectiveFrom)
            .Select(x => new EmployeeSalaryDto(
                x.s.Id, x.s.EmployeeId, x.e.EmployeeCode, x.e.FullName, x.s.SalaryGradeId, x.GradeName,
                x.s.BaseSalary, x.s.HourlyRate, x.s.DailyRate, x.s.AppliesToStatus,
                x.s.EffectiveFrom, x.s.EffectiveTo, x.s.IsActive, x.s.Note))
            .ToListAsync(ct);
    }

    public async Task<EmployeeSalaryDto> UpsertEmployeeSalaryAsync(
        Guid tenantId, Guid userId, EmployeeSalaryUpsertRequest req, CancellationToken ct = default)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        if (req.BaseSalary < 0) throw new AppException("Lương cơ bản không hợp lệ.");
        if (req.SalaryGradeId is Guid gid &&
            !await _db.SalaryGrades.AnyAsync(x => x.Id == gid && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Bậc lương không tồn tại.", 404);

        EmployeeSalary entity;
        if (req.Id is Guid id)
        {
            entity = await _db.EmployeeSalaries.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Bản ghi lương không tồn tại.", 404);
        }
        else
        {
            entity = new EmployeeSalary { Id = Guid.NewGuid(), TenantId = tenantId, CreatedBy = userId };
            _db.EmployeeSalaries.Add(entity);
        }

        entity.EmployeeId = req.EmployeeId;
        entity.SalaryGradeId = req.SalaryGradeId;
        entity.BaseSalary = req.BaseSalary;
        entity.HourlyRate = req.HourlyRate;
        entity.DailyRate = req.DailyRate;
        entity.AppliesToStatus = string.IsNullOrWhiteSpace(req.AppliesToStatus) ? null : req.AppliesToStatus.Trim();
        entity.EffectiveFrom = req.EffectiveFrom;
        entity.EffectiveTo = req.EffectiveTo;
        entity.IsActive = req.IsActive;
        entity.Note = req.Note;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        string? gradeName = null;
        if (entity.SalaryGradeId is Guid sg)
            gradeName = await _db.SalaryGrades.AsNoTracking().Where(x => x.Id == sg).Select(x => x.Name).FirstOrDefaultAsync(ct);

        return new EmployeeSalaryDto(
            entity.Id, entity.EmployeeId, emp.EmployeeCode, emp.FullName, entity.SalaryGradeId, gradeName,
            entity.BaseSalary, entity.HourlyRate, entity.DailyRate, entity.AppliesToStatus,
            entity.EffectiveFrom, entity.EffectiveTo, entity.IsActive, entity.Note);
    }

    public async Task<IReadOnlyList<AllowanceTypeDto>> ListAllowanceTypesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.AllowanceTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new AllowanceTypeDto(x.Id, x.Code, x.Name, x.DefaultAmount, x.IsTaxable, x.IsActive))
            .ToListAsync(ct);

    public async Task<AllowanceTypeDto> UpsertAllowanceTypeAsync(
        Guid tenantId, Guid userId, AllowanceTypeUpsertRequest req, CancellationToken ct = default)
    {
        var code = req.Code.Trim().ToUpperInvariant();
        var name = req.Name.Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã phụ cấp 1–40 ký tự.");
        if (name.Length is < 1 or > 100) throw new AppException("Tên phụ cấp 1–100 ký tự.");

        AllowanceType entity;
        if (req.Id is Guid id)
        {
            entity = await _db.AllowanceTypes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Loại phụ cấp không tồn tại.", 404);
            if (await _db.AllowanceTypes.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code && x.Id != id, ct))
                throw new AppException("Mã phụ cấp đã tồn tại.");
        }
        else
        {
            if (await _db.AllowanceTypes.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code, ct))
                throw new AppException("Mã phụ cấp đã tồn tại.");
            entity = new AllowanceType { Id = Guid.NewGuid(), TenantId = tenantId, CreatedBy = userId };
            _db.AllowanceTypes.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.DefaultAmount = Math.Max(0, req.DefaultAmount);
        entity.IsTaxable = req.IsTaxable;
        entity.IsActive = req.IsActive;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new AllowanceTypeDto(entity.Id, entity.Code, entity.Name, entity.DefaultAmount, entity.IsTaxable, entity.IsActive);
    }

    public async Task<IReadOnlyList<AllowanceRuleDto>> ListAllowanceRulesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var q =
            from r in _db.AllowanceRules.AsNoTracking()
            join t in _db.AllowanceTypes.AsNoTracking() on r.AllowanceTypeId equals t.Id
            where r.TenantId == tenantId && !r.IsDeleted && !t.IsDeleted
            orderby t.Code, r.ShiftCode
            select new AllowanceRuleDto(r.Id, r.AllowanceTypeId, t.Name, r.ShiftCode, r.Amount, r.IsActive, r.Note);
        return await q.ToListAsync(ct);
    }

    public async Task<AllowanceRuleDto> UpsertAllowanceRuleAsync(
        Guid tenantId, Guid userId, AllowanceRuleUpsertRequest req, CancellationToken ct = default)
    {
        var type = await _db.AllowanceTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.AllowanceTypeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Loại phụ cấp không tồn tại.", 404);

        AllowanceRule entity;
        if (req.Id is Guid id)
        {
            entity = await _db.AllowanceRules.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Rule phụ cấp không tồn tại.", 404);
        }
        else
        {
            entity = new AllowanceRule { Id = Guid.NewGuid(), TenantId = tenantId, CreatedBy = userId };
            _db.AllowanceRules.Add(entity);
        }

        entity.AllowanceTypeId = req.AllowanceTypeId;
        entity.ShiftCode = string.IsNullOrWhiteSpace(req.ShiftCode) ? null : req.ShiftCode.Trim().ToUpperInvariant();
        entity.Amount = Math.Max(0, req.Amount);
        entity.IsActive = req.IsActive;
        entity.Note = req.Note;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new AllowanceRuleDto(entity.Id, entity.AllowanceTypeId, type.Name, entity.ShiftCode, entity.Amount, entity.IsActive, entity.Note);
    }

    public async Task<PayrollPolicyDto> GetPolicyAsync(Guid tenantId, CancellationToken ct = default)
        => MapPolicy(await EnsurePolicyAsync(tenantId, ct));

    public async Task<PayrollPolicyDto> UpsertPolicyAsync(
        Guid tenantId, Guid userId, PayrollPolicyUpsertRequest req, CancellationToken ct = default)
    {
        var p = await EnsurePolicyAsync(tenantId, ct);
        if (req.StandardWorkDays is < 1 or > 31) throw new AppException("Ngày công chuẩn 1–31.");
        if (req.OtMultiplier is < 1 or > 5) throw new AppException("Hệ số OT 1–5.");
        if (req.FlatTaxRate is < 0 or > 1) throw new AppException("Thuế flat 0–1.");
        if (req.SocialInsuranceEmpRate is < 0 or > 1) throw new AppException("BHXH NV 0–1.");
        if (req.HealthInsuranceEmpRate is < 0 or > 1) throw new AppException("BHYT NV 0–1.");
        if (req.UnemploymentEmpRate is < 0 or > 1) throw new AppException("BHTN NV 0–1.");

        p.SocialInsuranceEmpRate = req.SocialInsuranceEmpRate;
        p.HealthInsuranceEmpRate = req.HealthInsuranceEmpRate;
        p.UnemploymentEmpRate = req.UnemploymentEmpRate;
        p.PersonalDeduction = Math.Max(0, req.PersonalDeduction);
        p.FlatTaxRate = req.FlatTaxRate;
        p.StandardWorkDays = req.StandardWorkDays;
        p.OtMultiplier = req.OtMultiplier;
        p.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapPolicy(p);
    }

    public async Task<IReadOnlyList<PayrollPeriodDto>> ListPeriodsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var periods = await _db.PayrollPeriods.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.PeriodKey)
            .ToListAsync(ct);
        var ids = periods.Select(x => x.Id).ToList();
        var aggs = await _db.PayrollLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ids.Contains(x.PayrollPeriodId))
            .GroupBy(x => x.PayrollPeriodId)
            .Select(g => new { PeriodId = g.Key, Count = g.Count(), Net = g.Sum(x => x.NetPay) })
            .ToListAsync(ct);
        var map = aggs.ToDictionary(x => x.PeriodId);
        return periods.Select(p =>
        {
            map.TryGetValue(p.Id, out var a);
            return new PayrollPeriodDto(p.Id, p.PeriodKey, p.PeriodFrom, p.PeriodTo, p.Status, p.Note,
                a?.Count ?? 0, a?.Net ?? 0, p.CreatedAt);
        }).ToList();
    }

    public async Task<PayrollPeriodDto> CreatePeriodAsync(
        Guid tenantId, Guid userId, PayrollPeriodCreateRequest req, CancellationToken ct = default)
    {
        var key = req.PeriodKey.Trim();
        if (!DateOnly.TryParseExact(key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            throw new AppException("PeriodKey phải dạng yyyy-MM.");
        var to = from.AddMonths(1).AddDays(-1);
        if (await _db.PayrollPeriods.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.PeriodKey == key, ct))
            throw new AppException("Kỳ lương đã tồn tại.");

        var p = new PayrollPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PeriodKey = key,
            PeriodFrom = from,
            PeriodTo = to,
            Status = "Draft",
            Note = req.Note,
            CreatedBy = userId
        };
        _db.PayrollPeriods.Add(p);
        await _db.SaveChangesAsync(ct);
        return new PayrollPeriodDto(p.Id, p.PeriodKey, p.PeriodFrom, p.PeriodTo, p.Status, p.Note, 0, 0, p.CreatedAt);
    }

    public async Task<PayrollPeriodDto> CalculateAsync(
        Guid tenantId, Guid userId, Guid periodId, CancellationToken ct = default)
    {
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Kỳ lương không tồn tại.", 404);
        if (period.Status is "Locked") throw new AppException("Kỳ đã khóa — không tính lại.");

        var policy = await EnsurePolicyAsync(tenantId, ct);
        var employees = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Active" || x.Status == "Probation"))
            .ToListAsync(ct);
        var salaries = await _db.EmployeeSalaries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .ToListAsync(ct);
        var att = await _db.AttendanceRecords.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.WorkDate >= period.PeriodFrom && x.WorkDate <= period.PeriodTo)
            .GroupBy(x => x.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, WorkUnits = g.Sum(r => r.WorkUnit), OtMinutes = g.Sum(r => r.OtMinutes) })
            .ToListAsync(ct);
        var attMap = att.ToDictionary(x => x.EmployeeId);

        var defaultAllow = await _db.AllowanceTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .SumAsync(x => (decimal?)x.DefaultAmount, ct) ?? 0m;

        var rules = await (
            from r in _db.AllowanceRules.AsNoTracking()
            join t in _db.AllowanceTypes.AsNoTracking() on r.AllowanceTypeId equals t.Id
            where r.TenantId == tenantId && !r.IsDeleted && r.IsActive && t.IsActive && !t.IsDeleted
            select r).ToListAsync(ct);

        var shiftAssigns = await (
            from a in _db.ShiftAssignments.AsNoTracking()
            join s in _db.WorkShifts.AsNoTracking() on a.WorkShiftId equals s.Id
            where a.TenantId == tenantId && !a.IsDeleted && !s.IsDeleted
                  && a.WorkDate >= period.PeriodFrom && a.WorkDate <= period.PeriodTo
            select new { a.EmployeeId, s.Code }).ToListAsync(ct);

        var adjustments = await _db.PayrollAdjustments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.PayrollPeriodId == periodId)
            .ToListAsync(ct);

        var existing = await _db.PayrollLines
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.PayrollPeriodId == periodId)
            .ToListAsync(ct);
        var byEmp = existing.ToDictionary(x => x.EmployeeId);

        foreach (var emp in employees)
        {
            var sal = ResolveSalary(salaries, emp, period.PeriodTo);
            var baseSalary = sal?.BaseSalary ?? 0m;
            attMap.TryGetValue(emp.Id, out var a);
            var workUnits = a?.WorkUnits ?? 0m;
            var otMinutes = a?.OtMinutes ?? 0;
            var daily = sal?.DailyRate ?? (policy.StandardWorkDays > 0 ? baseSalary / policy.StandardWorkDays : 0m);
            var hourly = sal?.HourlyRate ?? (daily / 8m);
            var attendancePay = Math.Round(daily * workUnits, 0, MidpointRounding.AwayFromZero);
            var otPay = Math.Round(hourly * (otMinutes / 60m) * policy.OtMultiplier, 0, MidpointRounding.AwayFromZero);

            var shiftCodes = shiftAssigns.Where(x => x.EmployeeId == emp.Id).Select(x => x.Code).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
            var ruleAllow = rules
                .Where(r => r.ShiftCode is null || shiftCodes.Contains(r.ShiftCode))
                .Sum(r => r.Amount);
            var allowBase = defaultAllow + ruleAllow;

            var adjBonus = adjustments.Where(x => x.EmployeeId == emp.Id && x.Kind is "Bonus" or "Allowance").Sum(x => x.Amount);
            var adjDed = adjustments.Where(x => x.EmployeeId == emp.Id && x.Kind is "Deduction" or "Advance").Sum(x => x.Amount);

            if (!byEmp.TryGetValue(emp.Id, out var line))
            {
                line = new PayrollLine
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PayrollPeriodId = periodId,
                    EmployeeId = emp.Id,
                    CreatedBy = userId
                };
                _db.PayrollLines.Add(line);
                byEmp[emp.Id] = line;
            }
            else if (line.IsConfirmed && period.Status == "Confirmed")
            {
                continue;
            }

            // Recalc luôn lấy phụ cấp từ rule; thưởng/khấu trừ từ bảng adjustment (patch tay sau khi tính).
            var allowanceTotal = allowBase;
            var bonus = adjBonus;
            var deduction = adjDed;

            var insRate = policy.SocialInsuranceEmpRate + policy.HealthInsuranceEmpRate + policy.UnemploymentEmpRate;
            var insurance = Math.Round(baseSalary * insRate, 0, MidpointRounding.AwayFromZero);
            var gross = attendancePay + otPay + allowanceTotal + bonus;
            var taxable = Math.Max(0, gross - insurance - policy.PersonalDeduction);
            var tax = Math.Round(taxable * policy.FlatTaxRate, 0, MidpointRounding.AwayFromZero);
            var net = gross - insurance - tax - deduction;

            line.WorkUnits = workUnits;
            line.OtMinutes = otMinutes;
            line.BaseSalary = baseSalary;
            line.AttendancePay = attendancePay;
            line.OtPay = otPay;
            line.AllowanceTotal = allowanceTotal;
            line.Bonus = bonus;
            line.DeductionTotal = deduction;
            line.InsuranceEmployee = insurance;
            line.Tax = tax;
            line.GrossPay = gross;
            line.NetPay = net;
            line.UpdatedBy = userId;
        }

        period.Status = "Calculated";
        period.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var dtoList = await ListPeriodsAsync(tenantId, ct);
        return dtoList.First(x => x.Id == periodId);
    }

    public async Task ConfirmAsync(Guid tenantId, Guid userId, Guid periodId, CancellationToken ct = default)
    {
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Kỳ lương không tồn tại.", 404);
        if (period.Status is "Locked") throw new AppException("Kỳ đã khóa.");
        if (period.Status is not "Calculated" and not "Confirmed")
            throw new AppException("Chỉ xác nhận kỳ đã tính lương.");

        var lines = await _db.PayrollLines.Where(x => x.TenantId == tenantId && !x.IsDeleted && x.PayrollPeriodId == periodId).ToListAsync(ct);
        foreach (var l in lines) { l.IsConfirmed = true; l.UpdatedBy = userId; }
        period.Status = "Confirmed";
        period.ConfirmedAt = DateTimeOffset.UtcNow;
        period.ConfirmedByUserId = userId;
        period.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task LockAsync(Guid tenantId, Guid userId, Guid periodId, CancellationToken ct = default)
    {
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Kỳ lương không tồn tại.", 404);
        if (period.Status is not "Confirmed" and not "Locked")
            throw new AppException("Khóa kỳ sau khi xác nhận bảng lương.");
        period.Status = "Locked";
        period.LockedAt = DateTimeOffset.UtcNow;
        period.LockedByUserId = userId;
        period.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PayrollLineDto>> ListLinesAsync(Guid tenantId, Guid periodId, CancellationToken ct = default)
        => await MapLinesQuery(tenantId, periodId, null).ToListAsync(ct);

    public async Task<IReadOnlyList<PayrollLineDto>> MyPayslipAsync(
        Guid tenantId, Guid userId, Guid? periodId, CancellationToken ct = default)
    {
        var empId = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
            .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct)
            ?? throw new AppException("Không tìm thấy hồ sơ nhân viên của bạn.", 404);

        Guid pid;
        if (periodId is Guid p) pid = p;
        else
        {
            pid = await _db.PayrollPeriods.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Confirmed" || x.Status == "Locked"))
                .OrderByDescending(x => x.PeriodKey)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(ct);
            if (pid == Guid.Empty) return Array.Empty<PayrollLineDto>();
        }

        return await MapLinesQuery(tenantId, pid, empId).ToListAsync(ct);
    }

    public async Task<PayrollLineDto> PatchLineAsync(
        Guid tenantId, Guid userId, Guid lineId, PayrollLinePatchRequest req, CancellationToken ct = default)
    {
        var line = await _db.PayrollLines.FirstOrDefaultAsync(x => x.Id == lineId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Dòng lương không tồn tại.", 404);
        var period = await _db.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == line.PayrollPeriodId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Kỳ lương không tồn tại.", 404);
        if (period.Status is "Locked") throw new AppException("Kỳ đã khóa.");
        if (line.IsConfirmed && period.Status == "Confirmed") throw new AppException("Dòng đã xác nhận.");

        if (req.Bonus is decimal b) line.Bonus = Math.Max(0, b);
        if (req.DeductionTotal is decimal d) line.DeductionTotal = Math.Max(0, d);
        if (req.AllowanceTotal is decimal a) line.AllowanceTotal = Math.Max(0, a);
        if (req.Note is not null) line.Note = req.Note;
        line.UpdatedBy = userId;

        // Recalc net for this line with current policy
        var policy = await EnsurePolicyAsync(tenantId, ct);
        var insRate = policy.SocialInsuranceEmpRate + policy.HealthInsuranceEmpRate + policy.UnemploymentEmpRate;
        line.InsuranceEmployee = Math.Round(line.BaseSalary * insRate, 0, MidpointRounding.AwayFromZero);
        line.GrossPay = line.AttendancePay + line.OtPay + line.AllowanceTotal + line.Bonus;
        var taxable = Math.Max(0, line.GrossPay - line.InsuranceEmployee - policy.PersonalDeduction);
        line.Tax = Math.Round(taxable * policy.FlatTaxRate, 0, MidpointRounding.AwayFromZero);
        line.NetPay = line.GrossPay - line.InsuranceEmployee - line.Tax - line.DeductionTotal;
        await _db.SaveChangesAsync(ct);

        return (await MapLinesQuery(tenantId, line.PayrollPeriodId, line.EmployeeId).FirstAsync(ct));
    }

    public async Task<IReadOnlyList<PayrollAdjustmentDto>> ListAdjustmentsAsync(
        Guid tenantId, Guid periodId, CancellationToken ct = default)
    {
        var q =
            from a in _db.PayrollAdjustments.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on a.EmployeeId equals e.Id
            where a.TenantId == tenantId && !a.IsDeleted && a.PayrollPeriodId == periodId && !e.IsDeleted
            orderby e.FullName, a.Kind
            select new PayrollAdjustmentDto(a.Id, a.PayrollPeriodId, a.EmployeeId, e.FullName, a.Kind, a.Title, a.Amount, a.Note);
        return await q.ToListAsync(ct);
    }

    public async Task<PayrollAdjustmentDto> AddAdjustmentAsync(
        Guid tenantId, Guid userId, PayrollAdjustmentCreateRequest req, CancellationToken ct = default)
    {
        var period = await _db.PayrollPeriods.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.PayrollPeriodId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Kỳ lương không tồn tại.", 404);
        if (period.Status is "Locked") throw new AppException("Kỳ đã khóa.");
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        var kind = req.Kind.Trim();
        if (kind is not ("Bonus" or "Allowance" or "Deduction" or "Advance"))
            throw new AppException("Kind: Bonus | Allowance | Deduction | Advance.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new AppException("Tiêu đề bắt buộc.");

        var a = new PayrollAdjustment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PayrollPeriodId = req.PayrollPeriodId,
            EmployeeId = req.EmployeeId,
            Kind = kind,
            Title = req.Title.Trim(),
            Amount = Math.Abs(req.Amount),
            Note = req.Note,
            CreatedBy = userId
        };
        _db.PayrollAdjustments.Add(a);
        await _db.SaveChangesAsync(ct);
        return new PayrollAdjustmentDto(a.Id, a.PayrollPeriodId, a.EmployeeId, emp.FullName, a.Kind, a.Title, a.Amount, a.Note);
    }

    public async Task<string> ExportCsvAsync(Guid tenantId, Guid periodId, CancellationToken ct = default)
    {
        var lines = await ListLinesAsync(tenantId, periodId, ct);
        var sb = new StringBuilder();
        sb.AppendLine("EmployeeCode,EmployeeName,OrgUnit,WorkUnits,OtMinutes,BaseSalary,AttendancePay,OtPay,Allowance,Bonus,Deduction,Insurance,Tax,Gross,Net");
        foreach (var l in lines)
        {
            sb.AppendLine(string.Join(',',
                Csv(l.EmployeeCode), Csv(l.EmployeeName), Csv(l.OrgUnitName),
                l.WorkUnits.ToString(CultureInfo.InvariantCulture),
                l.OtMinutes,
                l.BaseSalary.ToString(CultureInfo.InvariantCulture),
                l.AttendancePay.ToString(CultureInfo.InvariantCulture),
                l.OtPay.ToString(CultureInfo.InvariantCulture),
                l.AllowanceTotal.ToString(CultureInfo.InvariantCulture),
                l.Bonus.ToString(CultureInfo.InvariantCulture),
                l.DeductionTotal.ToString(CultureInfo.InvariantCulture),
                l.InsuranceEmployee.ToString(CultureInfo.InvariantCulture),
                l.Tax.ToString(CultureInfo.InvariantCulture),
                l.GrossPay.ToString(CultureInfo.InvariantCulture),
                l.NetPay.ToString(CultureInfo.InvariantCulture)));
        }
        return sb.ToString();
    }

    public async Task<string> ExportBankCsvAsync(Guid tenantId, Guid periodId, CancellationToken ct = default)
    {
        var lines = await ListLinesAsync(tenantId, periodId, ct);
        var sb = new StringBuilder();
        sb.AppendLine("EmployeeCode,EmployeeName,Amount,Content");
        foreach (var l in lines.Where(x => x.NetPay > 0))
            sb.AppendLine($"{Csv(l.EmployeeCode)},{Csv(l.EmployeeName)},{l.NetPay.ToString(CultureInfo.InvariantCulture)},{Csv("Chi luong " + l.EmployeeCode)}");
        return sb.ToString();
    }

    public async Task<IReadOnlyList<PayrollCostByOrgDto>> CostByOrgAsync(
        Guid tenantId, Guid periodId, CancellationToken ct = default)
    {
        var rows = await (
            from l in _db.PayrollLines.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on l.EmployeeId equals e.Id
            where l.TenantId == tenantId && !l.IsDeleted && l.PayrollPeriodId == periodId
            select new { e.OrgUnitId, l.GrossPay, l.NetPay, l.InsuranceEmployee }
        ).ToListAsync(ct);

        var orgIds = rows.Select(x => x.OrgUnitId).Distinct().ToList();
        var orgNames = await _db.OrgUnits.AsNoTracking()
            .Where(x => orgIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows.GroupBy(x => x.OrgUnitId)
            .Select(g => new PayrollCostByOrgDto(
                g.Key, orgNames.GetValueOrDefault(g.Key, ""), g.Count(),
                g.Sum(x => x.GrossPay), g.Sum(x => x.NetPay), g.Sum(x => x.InsuranceEmployee)))
            .OrderBy(x => x.OrgUnitName)
            .ToList();
    }

    public async Task<IReadOnlyList<PayrollCompareDto>> CompareAsync(
        Guid tenantId, string periodKey, CancellationToken ct = default)
    {
        if (!DateOnly.TryParseExact(periodKey.Trim() + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var curFrom))
            throw new AppException("PeriodKey phải dạng yyyy-MM.");
        var prevKey = curFrom.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var keys = new[] { periodKey.Trim(), prevKey };
        var periods = await _db.PayrollPeriods.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && keys.Contains(x.PeriodKey))
            .ToListAsync(ct);
        var result = new List<PayrollCompareDto>();
        foreach (var key in keys)
        {
            var p = periods.FirstOrDefault(x => x.PeriodKey == key);
            if (p is null)
            {
                result.Add(new PayrollCompareDto(key, 0, 0, 0, 0));
                continue;
            }
            var agg = await _db.PayrollLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.PayrollPeriodId == p.Id)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Gross = g.Sum(x => x.GrossPay),
                    Net = g.Sum(x => x.NetPay),
                    Ins = g.Sum(x => x.InsuranceEmployee),
                    Cnt = g.Count()
                }).FirstOrDefaultAsync(ct);
            result.Add(new PayrollCompareDto(key, agg?.Gross ?? 0, agg?.Net ?? 0, agg?.Ins ?? 0, agg?.Cnt ?? 0));
        }
        return result;
    }

    private IQueryable<PayrollLineDto> MapLinesQuery(Guid tenantId, Guid periodId, Guid? employeeId)
    {
        var q =
            from l in _db.PayrollLines.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on l.EmployeeId equals e.Id
            join o in _db.OrgUnits.AsNoTracking() on e.OrgUnitId equals o.Id into oj
            from o in oj.DefaultIfEmpty()
            where l.TenantId == tenantId && !l.IsDeleted && l.PayrollPeriodId == periodId && !e.IsDeleted
            select new PayrollLineDto(
                l.Id, l.PayrollPeriodId, l.EmployeeId, e.EmployeeCode, e.FullName, o != null ? o.Name : "",
                l.WorkUnits, l.OtMinutes, l.BaseSalary, l.AttendancePay, l.OtPay,
                l.AllowanceTotal, l.Bonus, l.DeductionTotal, l.InsuranceEmployee,
                l.Tax, l.GrossPay, l.NetPay, l.IsConfirmed, l.Note);
        if (employeeId is Guid eid) q = q.Where(x => x.EmployeeId == eid);
        return q.OrderBy(x => x.EmployeeCode);
    }

    private static EmployeeSalary? ResolveSalary(List<EmployeeSalary> all, Employee emp, DateOnly asOf)
    {
        return all
            .Where(x => x.EmployeeId == emp.Id
                        && x.EffectiveFrom <= asOf
                        && (x.EffectiveTo == null || x.EffectiveTo >= asOf)
                        && (x.AppliesToStatus == null || string.Equals(x.AppliesToStatus, emp.Status, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(x => x.AppliesToStatus != null)
            .ThenByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();
    }

    private async Task<PayrollPolicy> EnsurePolicyAsync(Guid tenantId, CancellationToken ct)
    {
        var p = await _db.PayrollPolicies.FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        if (p is not null) return p;
        p = new PayrollPolicy { Id = Guid.NewGuid(), TenantId = tenantId };
        _db.PayrollPolicies.Add(p);
        await _db.SaveChangesAsync(ct);
        return p;
    }

    private static PayrollPolicyDto MapPolicy(PayrollPolicy p) => new(
        p.SocialInsuranceEmpRate, p.HealthInsuranceEmpRate, p.UnemploymentEmpRate,
        p.PersonalDeduction, p.FlatTaxRate, p.StandardWorkDays, p.OtMultiplier);

    private static string Csv(string? v)
    {
        v ??= "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }
}
