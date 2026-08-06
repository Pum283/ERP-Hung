using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmPayrollService
{
    Task<IReadOnlyList<SalaryGradeDto>> ListGradesAsync(Guid tenantId, CancellationToken ct = default);
    Task<SalaryGradeDto> UpsertGradeAsync(Guid tenantId, Guid userId, SalaryGradeUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<EmployeeSalaryDto>> ListEmployeeSalariesAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default);
    Task<EmployeeSalaryDto> UpsertEmployeeSalaryAsync(Guid tenantId, Guid userId, EmployeeSalaryUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AllowanceTypeDto>> ListAllowanceTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<AllowanceTypeDto> UpsertAllowanceTypeAsync(Guid tenantId, Guid userId, AllowanceTypeUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AllowanceRuleDto>> ListAllowanceRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task<AllowanceRuleDto> UpsertAllowanceRuleAsync(Guid tenantId, Guid userId, AllowanceRuleUpsertRequest req, CancellationToken ct = default);

    Task<PayrollPolicyDto> GetPolicyAsync(Guid tenantId, CancellationToken ct = default);
    Task<PayrollPolicyDto> UpsertPolicyAsync(Guid tenantId, Guid userId, PayrollPolicyUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PayrollPeriodDto>> ListPeriodsAsync(Guid tenantId, CancellationToken ct = default);
    Task<PayrollPeriodDto> CreatePeriodAsync(Guid tenantId, Guid userId, PayrollPeriodCreateRequest req, CancellationToken ct = default);
    Task<PayrollPeriodDto> CalculateAsync(Guid tenantId, Guid userId, Guid periodId, CancellationToken ct = default);
    Task ConfirmAsync(Guid tenantId, Guid userId, Guid periodId, CancellationToken ct = default);
    Task LockAsync(Guid tenantId, Guid userId, Guid periodId, CancellationToken ct = default);

    Task<IReadOnlyList<PayrollLineDto>> ListLinesAsync(Guid tenantId, Guid periodId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollLineDto>> MyPayslipAsync(Guid tenantId, Guid userId, Guid? periodId, CancellationToken ct = default);
    Task<PayrollLineDto> PatchLineAsync(Guid tenantId, Guid userId, Guid lineId, PayrollLinePatchRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PayrollAdjustmentDto>> ListAdjustmentsAsync(Guid tenantId, Guid periodId, CancellationToken ct = default);
    Task<PayrollAdjustmentDto> AddAdjustmentAsync(Guid tenantId, Guid userId, PayrollAdjustmentCreateRequest req, CancellationToken ct = default);

    Task<string> ExportCsvAsync(Guid tenantId, Guid periodId, CancellationToken ct = default);
    Task<string> ExportBankCsvAsync(Guid tenantId, Guid periodId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollCostByOrgDto>> CostByOrgAsync(Guid tenantId, Guid periodId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollCompareDto>> CompareAsync(Guid tenantId, string periodKey, CancellationToken ct = default);
}
