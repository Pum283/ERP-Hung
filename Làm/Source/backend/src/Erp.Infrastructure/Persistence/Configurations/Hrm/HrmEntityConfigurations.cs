using Erp.Domain.Entities.Hrm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Hrm;

public sealed class JobTitleConfig : IEntityTypeConfiguration<JobTitle>
{
    public void Configure(EntityTypeBuilder<JobTitle> b)
    {
        b.ToTable("job_title", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class EmployeeTypeConfig : IEntityTypeConfiguration<EmployeeType>
{
    public void Configure(EntityTypeBuilder<EmployeeType> b)
    {
        b.ToTable("employee_type", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.ToTable("employee", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeCode }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.DepartmentId, x.Status });
        b.Property(x => x.EmployeeCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Gender).HasMaxLength(20);
        b.Property(x => x.Email).HasMaxLength(255);
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LeaveTypeConfig : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> b)
    {
        b.ToTable("leave_type", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.DefaultDaysPerYear).HasPrecision(5, 1);
    }
}

public sealed class ContractConfig : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("contract", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ContractNo }).IsUnique();
        b.Property(x => x.ContractNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.ContractType).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.BaseSalary).HasPrecision(18, 2);
    }
}

public sealed class LeaveBalanceConfig : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> b)
    {
        b.ToTable("leave_balance", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.LeaveTypeId, x.Year }).IsUnique();
        b.Property(x => x.Entitled).HasPrecision(5, 1);
        b.Property(x => x.Used).HasPrecision(5, 1);
        b.Property(x => x.Remaining).HasPrecision(5, 1);
    }
}

public sealed class LeaveRequestConfig : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> b)
    {
        b.ToTable("leave_request", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Status, x.FromDate });
        b.Property(x => x.Days).HasPrecision(5, 1);
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LeaveEntitlementRuleConfig : IEntityTypeConfiguration<LeaveEntitlementRule>
{
    public void Configure(EntityTypeBuilder<LeaveEntitlementRule> b)
    {
        b.ToTable("leave_entitlement_rule", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.LeaveTypeId, x.EmployeeTypeId });
        b.Property(x => x.DaysPerYear).HasPrecision(5, 1);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class HolidayConfig : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> b)
    {
        b.ToTable("holiday", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Date }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Year });
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class RecruitmentRequestConfig : IEntityTypeConfiguration<RecruitmentRequest>
{
    public void Configure(EntityTypeBuilder<RecruitmentRequest> b)
    {
        b.ToTable("recruitment_request", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DocNo }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Status });
        b.Property(x => x.DocNo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class JobPostingConfig : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> b)
    {
        b.ToTable("job_posting", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.RecruitmentRequestId });
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Channel).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class CandidateConfig : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> b)
    {
        b.ToTable("candidate", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.JobPostingId, x.PipelineStatus });
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.CvStorageKey).HasMaxLength(500);
        b.Property(x => x.PipelineStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.EvalComment).HasMaxLength(1000);
        b.Property(x => x.CareNotes).HasMaxLength(4000);
    }
}

public sealed class OnboardingSettingConfig : IEntityTypeConfiguration<OnboardingSetting>
{
    public void Configure(EntityTypeBuilder<OnboardingSetting> b)
    {
        b.ToTable("onboarding_setting", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TenantId).IsUnique();
    }
}

public sealed class OnboardingCaseConfig : IEntityTypeConfiguration<OnboardingCase>
{
    public void Configure(EntityTypeBuilder<OnboardingCase> b)
    {
        b.ToTable("onboarding_case", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.TrialEndDate });
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.TrialComment).HasMaxLength(1000);
        b.Property(x => x.ChecklistJson).HasMaxLength(4000).IsRequired();
    }
}

public sealed class OnboardingDocumentConfig : IEntityTypeConfiguration<OnboardingDocument>
{
    public void Configure(EntityTypeBuilder<OnboardingDocument> b)
    {
        b.ToTable("onboarding_document", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.OnboardingCaseId });
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
    }
}

public sealed class EmployeeDocumentConfig : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> b)
    {
        b.ToTable("employee_document", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId });
        b.Property(x => x.DocType).HasMaxLength(40).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
    }
}

public sealed class HeadcountPlanConfig : IEntityTypeConfiguration<HeadcountPlan>
{
    public void Configure(EntityTypeBuilder<HeadcountPlan> b)
    {
        b.ToTable("headcount_plan", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Status, x.ScopeType });
        b.HasIndex(x => new { x.TenantId, x.OrgUnitId });
        b.Property(x => x.ScopeType).HasMaxLength(30).IsRequired();
        b.Property(x => x.ShiftCode).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class WorkShiftConfig : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> b)
    {
        b.ToTable("work_shift", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class ShiftAssignmentConfig : IEntityTypeConfiguration<ShiftAssignment>
{
    public void Configure(EntityTypeBuilder<ShiftAssignment> b)
    {
        b.ToTable("shift_assignment", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.WorkDate }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.WorkDate });
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class ShiftPeriodLockConfig : IEntityTypeConfiguration<ShiftPeriodLock>
{
    public void Configure(EntityTypeBuilder<ShiftPeriodLock> b)
    {
        b.ToTable("shift_period_lock", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.OrgUnitId, x.PeriodKey }).IsUnique();
        b.Property(x => x.PeriodKey).HasMaxLength(7).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class StaffTransferConfig : IEntityTypeConfiguration<StaffTransfer>
{
    public void Configure(EntityTypeBuilder<StaffTransfer> b)
    {
        b.ToTable("staff_transfer", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DocNo }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Kind, x.Status });
        b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.StartDate });
        b.Property(x => x.DocNo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Kind).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.Property(x => x.AttendanceTag).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.PlannedHours).HasPrecision(12, 2);
        b.Property(x => x.ActualHours).HasPrecision(12, 2);
        b.Property(x => x.CostRate).HasPrecision(18, 2);
    }
}

public sealed class AttendancePolicyConfig : IEntityTypeConfiguration<AttendancePolicy>
{
    public void Configure(EntityTypeBuilder<AttendancePolicy> b)
    {
        b.ToTable("attendance_policy", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TenantId).IsUnique();
        b.Property(x => x.LateDeductWorkUnit).HasPrecision(5, 2);
    }
}

public sealed class AttendanceDeviceConfig : IEntityTypeConfiguration<AttendanceDevice>
{
    public void Configure(EntityTypeBuilder<AttendanceDevice> b)
    {
        b.ToTable("attendance_device", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.DeviceType).HasMaxLength(30).IsRequired();
        b.Property(x => x.SerialNo).HasMaxLength(80);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class AttendanceGeoFenceConfig : IEntityTypeConfiguration<AttendanceGeoFence>
{
    public void Configure(EntityTypeBuilder<AttendanceGeoFence> b)
    {
        b.ToTable("attendance_geofence", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Name });
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class AttendanceRecordConfig : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> b)
    {
        b.ToTable("attendance_record", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.WorkDate }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.WorkDate, x.Status });
        b.Property(x => x.CheckInMethod).HasMaxLength(30);
        b.Property(x => x.CheckOutMethod).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Tag).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.DeductedWorkUnit).HasPrecision(5, 2);
        b.Property(x => x.WorkUnit).HasPrecision(5, 2);
    }
}

public sealed class AttendanceAdjustRequestConfig : IEntityTypeConfiguration<AttendanceAdjustRequest>
{
    public void Configure(EntityTypeBuilder<AttendanceAdjustRequest> b)
    {
        b.ToTable("attendance_adjust_request", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Status, x.WorkDate });
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.Property(x => x.EvidenceStorageKey).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class AttendancePeriodLockConfig : IEntityTypeConfiguration<AttendancePeriodLock>
{
    public void Configure(EntityTypeBuilder<AttendancePeriodLock> b)
    {
        b.ToTable("attendance_period_lock", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PeriodKey }).IsUnique();
        b.Property(x => x.PeriodKey).HasMaxLength(7).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class EmploymentStatusHistoryConfig : IEntityTypeConfiguration<EmploymentStatusHistory>
{
    public void Configure(EntityTypeBuilder<EmploymentStatusHistory> b)
    {
        b.ToTable("employment_status_history", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId });
        b.Property(x => x.FromStatus).HasMaxLength(30);
        b.Property(x => x.ToStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class SalaryGradeConfig : IEntityTypeConfiguration<SalaryGrade>
{
    public void Configure(EntityTypeBuilder<SalaryGrade> b)
    {
        b.ToTable("salary_grade", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.BaseAmount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class EmployeeSalaryConfig : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> b)
    {
        b.ToTable("employee_salary", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.EffectiveFrom });
        b.Property(x => x.BaseSalary).HasPrecision(18, 2);
        b.Property(x => x.HourlyRate).HasPrecision(18, 2);
        b.Property(x => x.DailyRate).HasPrecision(18, 2);
        b.Property(x => x.AppliesToStatus).HasMaxLength(30);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class AllowanceTypeConfig : IEntityTypeConfiguration<AllowanceType>
{
    public void Configure(EntityTypeBuilder<AllowanceType> b)
    {
        b.ToTable("allowance_type", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.DefaultAmount).HasPrecision(18, 2);
    }
}

public sealed class AllowanceRuleConfig : IEntityTypeConfiguration<AllowanceRule>
{
    public void Configure(EntityTypeBuilder<AllowanceRule> b)
    {
        b.ToTable("allowance_rule", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.AllowanceTypeId, x.ShiftCode });
        b.Property(x => x.ShiftCode).HasMaxLength(40);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class PayrollPolicyConfig : IEntityTypeConfiguration<PayrollPolicy>
{
    public void Configure(EntityTypeBuilder<PayrollPolicy> b)
    {
        b.ToTable("payroll_policy", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TenantId).IsUnique();
        b.Property(x => x.SocialInsuranceEmpRate).HasPrecision(8, 4);
        b.Property(x => x.HealthInsuranceEmpRate).HasPrecision(8, 4);
        b.Property(x => x.UnemploymentEmpRate).HasPrecision(8, 4);
        b.Property(x => x.PersonalDeduction).HasPrecision(18, 2);
        b.Property(x => x.FlatTaxRate).HasPrecision(8, 4);
        b.Property(x => x.OtMultiplier).HasPrecision(8, 4);
    }
}

public sealed class PayrollPeriodConfig : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> b)
    {
        b.ToTable("payroll_period", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PeriodKey }).IsUnique();
        b.Property(x => x.PeriodKey).HasMaxLength(7).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class PayrollLineConfig : IEntityTypeConfiguration<PayrollLine>
{
    public void Configure(EntityTypeBuilder<PayrollLine> b)
    {
        b.ToTable("payroll_line", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PayrollPeriodId, x.EmployeeId }).IsUnique();
        b.Property(x => x.WorkUnits).HasPrecision(8, 2);
        b.Property(x => x.BaseSalary).HasPrecision(18, 2);
        b.Property(x => x.AttendancePay).HasPrecision(18, 2);
        b.Property(x => x.OtPay).HasPrecision(18, 2);
        b.Property(x => x.AllowanceTotal).HasPrecision(18, 2);
        b.Property(x => x.Bonus).HasPrecision(18, 2);
        b.Property(x => x.DeductionTotal).HasPrecision(18, 2);
        b.Property(x => x.InsuranceEmployee).HasPrecision(18, 2);
        b.Property(x => x.Tax).HasPrecision(18, 2);
        b.Property(x => x.GrossPay).HasPrecision(18, 2);
        b.Property(x => x.NetPay).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class PayrollAdjustmentConfig : IEntityTypeConfiguration<PayrollAdjustment>
{
    public void Configure(EntityTypeBuilder<PayrollAdjustment> b)
    {
        b.ToTable("payroll_adjustment", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PayrollPeriodId, x.EmployeeId });
        b.Property(x => x.Kind).HasMaxLength(30).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class RewardDisciplineDecisionConfig : IEntityTypeConfiguration<RewardDisciplineDecision>
{
    public void Configure(EntityTypeBuilder<RewardDisciplineDecision> b)
    {
        b.ToTable("reward_discipline_decision", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Kind, x.DecisionDate });
        b.Property(x => x.Kind).HasMaxLength(30).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.Property(x => x.PayrollImpactAmount).HasPrecision(18, 2);
        b.Property(x => x.PayrollImpactKind).HasMaxLength(30).IsRequired();
        b.Property(x => x.DecisionStorageKey).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class OffboardingSettingConfig : IEntityTypeConfiguration<OffboardingSetting>
{
    public void Configure(EntityTypeBuilder<OffboardingSetting> b)
    {
        b.ToTable("offboarding_setting", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TenantId).IsUnique();
    }
}

public sealed class OffboardingCaseConfig : IEntityTypeConfiguration<OffboardingCase>
{
    public void Configure(EntityTypeBuilder<OffboardingCase> b)
    {
        b.ToTable("offboarding_case", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Status, x.RequestDate });
        b.Property(x => x.ReasonCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ReasonDetail).HasMaxLength(1000);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ChecklistJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.LeaveDaysRemaining).HasPrecision(8, 2);
        b.Property(x => x.LeaveSettlementAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalPayEstimate).HasPrecision(18, 2);
        b.Property(x => x.SettlementNote).HasMaxLength(1000);
        b.Property(x => x.InterviewNotes).HasMaxLength(2000);
        b.Property(x => x.RejectReason).HasMaxLength(500);
    }
}
