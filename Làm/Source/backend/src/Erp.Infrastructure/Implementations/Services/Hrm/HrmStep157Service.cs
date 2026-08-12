using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmStep157Service : IHrmStep157Service
{
    private readonly AppDbContext _db;

    public HrmStep157Service(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_024: Quản lý trình độ / kỹ năng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmEmployeeSkillDto>> GetSkillsAsync(Guid tenantId, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _db.HrmEmployeeSkills.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (employeeId.HasValue && employeeId.Value != Guid.Empty)
        {
            query = query.Where(s => s.EmployeeId == employeeId.Value);
        }

        var items = await query.OrderBy(s => s.SkillName).ToListAsync(ct);
        var empIds = items.Select(s => s.EmployeeId).Distinct().ToList();
        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => $"{e.EmployeeCode} - {e.FullName}", ct);

        return items.Select(s => new HrmEmployeeSkillDto(
            s.Id,
            s.EmployeeId,
            employees.TryGetValue(s.EmployeeId, out var empName) ? empName : null,
            s.SkillName,
            s.ProficiencyLevel,
            s.CertificateRef,
            s.CreatedAt
        )).ToList();
    }

    public async Task<HrmEmployeeSkillDto> GetSkillByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetSkillsAsync(tenantId, null, ct);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy kỹ năng {id}", 404);
        return item;
    }

    public async Task<HrmEmployeeSkillDto> CreateSkillAsync(Guid tenantId, HrmEmployeeSkillUpsertRequest req, CancellationToken ct = default)
    {
        ValidateSkillRequest(req);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        var entity = new HrmEmployeeSkill
        {
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            SkillName = req.SkillName.Trim(),
            ProficiencyLevel = NormalizeProficiency(req.ProficiencyLevel),
            CertificateRef = req.CertificateRef?.Trim()
        };

        _db.HrmEmployeeSkills.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetSkillByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmEmployeeSkillDto> UpdateSkillAsync(Guid tenantId, Guid id, HrmEmployeeSkillUpsertRequest req, CancellationToken ct = default)
    {
        ValidateSkillRequest(req);

        var entity = await _db.HrmEmployeeSkills.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy kỹ năng {id}", 404);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        entity.EmployeeId = req.EmployeeId;
        entity.SkillName = req.SkillName.Trim();
        entity.ProficiencyLevel = NormalizeProficiency(req.ProficiencyLevel);
        entity.CertificateRef = req.CertificateRef?.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetSkillByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteSkillAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HrmEmployeeSkills.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy kỹ năng {id}", 404);

        _db.HrmEmployeeSkills.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateSkillRequest(HrmEmployeeSkillUpsertRequest req)
    {
        if (req.EmployeeId == Guid.Empty) throw new AppException("Nhân sự không được để trống.");
        if (string.IsNullOrWhiteSpace(req.SkillName)) throw new AppException("Tên kỹ năng không được để trống.");
    }

    private static string NormalizeProficiency(string level)
    {
        var valid = new[] { "Basic", "Intermediate", "Advanced", "Expert" };
        var found = valid.FirstOrDefault(v => string.Equals(v, level, StringComparison.OrdinalIgnoreCase));
        return found ?? "Intermediate";
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_037: Báo cáo biến động nhân sự
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<HrmPersonnelMovementReportDto> GetPersonnelMovementReportAsync(Guid tenantId, HrmPersonnelMovementFilter filter, CancellationToken ct = default)
    {
        var toDate = filter.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = filter.FromDate ?? toDate.AddMonths(-1);

        var query = _db.Employees.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (filter.OrgUnitId.HasValue && filter.OrgUnitId.Value != Guid.Empty)
        {
            query = query.Where(e => e.OrgUnitId == filter.OrgUnitId.Value);
        }

        var employees = await query.ToListAsync(ct);
        var total = employees.Count;
        var active = employees.Count(e => string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase));
        var onLeave = employees.Count(e => string.Equals(e.Status, "OnLeave", StringComparison.OrdinalIgnoreCase));
        var terminated = employees.Count(e => string.Equals(e.Status, "Terminated", StringComparison.OrdinalIgnoreCase));

        var joiners = employees.Count(e => e.HireDate.HasValue && e.HireDate.Value >= fromDate && e.HireDate.Value <= toDate);
        var leavers = employees.Count(e => e.TerminateDate.HasValue && e.TerminateDate.Value >= fromDate && e.TerminateDate.Value <= toDate);

        var turnoverRate = total > 0 ? Math.Round((decimal)leavers / total * 100m, 2) : 0m;

        // Department breakdown
        var depIds = employees.Where(e => e.DepartmentId.HasValue).Select(e => e.DepartmentId!.Value).Distinct().ToList();
        var departments = await _db.Departments.AsNoTracking()
            .Where(d => d.TenantId == tenantId && depIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        var breakdown = employees
            .Where(e => e.DepartmentId.HasValue)
            .GroupBy(e => e.DepartmentId!.Value)
            .Select(g => new DepartmentHeadcountStatDto(
                g.Key,
                departments.TryGetValue(g.Key, out var dName) ? dName : "Phòng chưa phân loại",
                g.Count(),
                g.Count(e => string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase))
            ))
            .ToList();

        return new HrmPersonnelMovementReportDto(
            total,
            active,
            onLeave,
            terminated,
            joiners,
            leavers,
            turnoverRate,
            breakdown,
            fromDate,
            toDate
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_044: In / xuất mẫu hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<HrmContractTemplatePrintDto> PrintContractTemplateAsync(Guid tenantId, HrmContractExportRequest req, CancellationToken ct = default)
    {
        var contract = await _db.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.ContractId, ct);
        if (contract == null) throw new AppException($"Không tìm thấy hợp đồng {req.ContractId}", 404);

        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == contract.EmployeeId, ct);
        var empCode = employee?.EmployeeCode ?? "N/A";
        var empName = employee?.FullName ?? "Không xác định";

        var sb = new StringBuilder();
        sb.AppendLine("==================================================================");
        sb.AppendLine("                    CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM");
        sb.AppendLine("                       Độc lập - Tự do - Hạnh phúc");
        sb.AppendLine("==================================================================");
        sb.AppendLine($"                     HỢP ĐỒNG LAO ĐỘNG ({contract.ContractType.ToUpper()})");
        sb.AppendLine($"Số hợp đồng: {contract.ContractNo}");
        sb.AppendLine($"Ngày lập: {DateTime.UtcNow:dd/MM/yyyy}");
        sb.AppendLine("------------------------------------------------------------------");
        sb.AppendLine("BÊN A (NĂNG LƯỢNG SỬ DỤNG LAO ĐỘNG): CÔNG TY ERP HÙNG DEMO");
        sb.AppendLine($"BÊN B (NGƯỜI LAO ĐỘNG): {empName.ToUpper()} (Mã NV: {empCode})");
        sb.AppendLine($"Ngày bắt đầu: {contract.StartDate:dd/MM/yyyy}");
        sb.AppendLine($"Ngày kết thúc: {(contract.EndDate.HasValue ? contract.EndDate.Value.ToString("dd/MM/yyyy") : "Vô thời hạn")}");
        sb.AppendLine($"Mức lương chính: {contract.BaseSalary:N0} VNĐ");
        sb.AppendLine("------------------------------------------------------------------");
        sb.AppendLine("Hợp đồng có hiệu lực kể từ ngày ký.");
        sb.AppendLine("==================================================================");

        return new HrmContractTemplatePrintDto(
            contract.Id,
            contract.ContractNo,
            empCode,
            empName,
            contract.ContractType,
            contract.StartDate,
            contract.EndDate,
            contract.BaseSalary,
            sb.ToString(),
            DateTimeOffset.UtcNow
        );
    }

    public async Task<byte[]> ExportContractTextAsync(Guid tenantId, HrmContractExportRequest req, CancellationToken ct = default)
    {
        var dto = await PrintContractTemplateAsync(tenantId, req, ct);
        return Encoding.UTF8.GetBytes(dto.FormattedTemplateText);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_058: Import ứng viên hàng loạt
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<HrmBulkCandidateImportResult> ImportCandidatesBulkAsync(Guid tenantId, IReadOnlyList<HrmBulkCandidateImportItem> items, CancellationToken ct = default)
    {
        if (items == null || items.Count == 0)
        {
            throw new AppException("Danh sách ứng viên import không được rỗng.");
        }

        var importedIds = new List<Guid>();
        var errors = new List<HrmBulkCandidateImportError>();

        var jobPostingIds = items.Select(i => i.JobPostingId).Distinct().ToList();
        var existingJobPostings = await _db.JobPostings.AsNoTracking()
            .Where(j => j.TenantId == tenantId && jobPostingIds.Contains(j.Id))
            .Select(j => j.Id)
            .ToListAsync(ct);

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (string.IsNullOrWhiteSpace(item.FullName))
            {
                errors.Add(new HrmBulkCandidateImportError(i + 1, item.FullName ?? "", "Họ tên ứng viên không được để trống."));
                continue;
            }

            if (!existingJobPostings.Contains(item.JobPostingId))
            {
                errors.Add(new HrmBulkCandidateImportError(i + 1, item.FullName, $"Tin tuyển dụng {item.JobPostingId} không tồn tại."));
                continue;
            }

            var candidate = new Candidate
            {
                TenantId = tenantId,
                JobPostingId = item.JobPostingId,
                FullName = item.FullName.Trim(),
                Email = item.Email?.Trim(),
                Phone = item.Phone?.Trim(),
                PipelineStatus = "New"
            };

            _db.Candidates.Add(candidate);
            importedIds.Add(candidate.Id);
        }

        if (importedIds.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return new HrmBulkCandidateImportResult(
            items.Count,
            importedIds.Count,
            errors.Count,
            importedIds,
            errors
        );
    }
}
