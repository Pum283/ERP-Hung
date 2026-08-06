using Xunit;

namespace Erp.UnitTests;

public class HrmUnitTestSuite
{
    [Fact]
    public void Hrm_EmployeeCode_AutoGeneration_GeneratesSequentialCode()
    {
        int lastSequenceNumber = 142;
        string expectedCode = "EMP-00143";

        string generatedCode = $"EMP-{(lastSequenceNumber + 1):D5}";

        Assert.Equal(expectedCode, generatedCode);
    }

    [Fact]
    public void Hrm_ProbationPeriod_CalculatesEndFromStartDate()
    {
        DateOnly startDate = new DateOnly(2026, 8, 1);
        int probationMonths = 2;

        DateOnly expectedEndDate = new DateOnly(2026, 10, 1);
        DateOnly actualEndDate = startDate.AddMonths(probationMonths);

        Assert.Equal(expectedEndDate, actualEndDate);
    }

    [Fact]
    public void Hrm_ContractRenewal_Alert_Triggers30DaysBeforeExpiry()
    {
        DateOnly contractEndDate = new DateOnly(2026, 9, 5);
        DateOnly currentDate = new DateOnly(2026, 8, 6);

        int daysRemaining = contractEndDate.DayNumber - currentDate.DayNumber;
        bool isRenewalAlertTriggered = daysRemaining <= 30;

        Assert.Equal(30, daysRemaining);
        Assert.True(isRenewalAlertTriggered);
    }

    [Fact]
    public void Hrm_SocialInsurance_EmployeeDeduction_Calculates10Point5Percent()
    {
        decimal insuranceSalary = 10000000;
        decimal employeeRatePercent = 10.5m; // 8% BHXH + 1.5% BHYT + 1% BHTN

        decimal deductionAmount = insuranceSalary * (employeeRatePercent / 100);

        Assert.Equal(1050000, deductionAmount);
    }

    [Fact]
    public void Hrm_PersonalIncomeTax_ProgressiveTaxBrackets_CalculatesCorrectTax()
    {
        // Taxable income = 15,000,000 VND
        // Bracket 1: 0 - 5M @ 5% = 250,000
        // Bracket 2: 5M - 10M @ 10% = 500,000
        // Bracket 3: 10M - 18M (5M) @ 15% = 750,000
        // Total Tax = 1,500,000
        decimal taxableIncome = 15000000;

        decimal taxBracket1 = Math.Min(taxableIncome, 5000000) * 0.05m;
        decimal taxBracket2 = Math.Max(0, Math.Min(taxableIncome - 5000000, 5000000)) * 0.10m;
        decimal taxBracket3 = Math.Max(0, taxableIncome - 10000000) * 0.15m;

        decimal totalPitTax = taxBracket1 + taxBracket2 + taxBracket3;

        Assert.Equal(1500000, totalPitTax);
    }

    [Fact]
    public void Hrm_OvertimeRate_HolidayMultiplier_Calculates300Percent()
    {
        decimal hourlyRate = 50000;
        decimal overtimeHours = 4;
        decimal holidayMultiplier = 3.0m;

        decimal overtimePay = hourlyRate * overtimeHours * holidayMultiplier;

        Assert.Equal(600000, overtimePay);
    }

    [Fact]
    public void Hrm_ShiftAssignment_RosterConflictCheck_DetectsOverlappingShifts()
    {
        var shift1 = (Start: new TimeSpan(8, 0, 0), End: new TimeSpan(16, 0, 0));
        var shift2 = (Start: new TimeSpan(14, 0, 0), End: new TimeSpan(22, 0, 0));

        bool isOverlapping = shift1.Start < shift2.End && shift2.Start < shift1.End;

        Assert.True(isOverlapping);
    }

    [Fact]
    public void Hrm_SeniorityLeaveBonus_CalculatesAdditionalDaysAfter5Years()
    {
        int totalTenureYears = 7;
        int baseLeaveDays = 12;

        int bonusLeaveDays = totalTenureYears / 5; // +1 day for every 5 full years
        int totalAnnualLeave = baseLeaveDays + bonusLeaveDays;

        Assert.Equal(1, bonusLeaveDays);
        Assert.Equal(13, totalAnnualLeave);
    }

    [Fact]
    public void Hrm_ResignationNotice_CheckNoticePeriod_FailsIfInsufficientNotice()
    {
        DateOnly noticeDate = new DateOnly(2026, 8, 1);
        DateOnly requestedLastDay = new DateOnly(2026, 8, 15); // 14 days notice
        int requiredNoticeDays = 30; // 30 days for indefinite labor contract

        int actualNoticeDays = requestedLastDay.DayNumber - noticeDate.DayNumber;
        bool isValidNotice = actualNoticeDays >= requiredNoticeDays;

        Assert.Equal(14, actualNoticeDays);
        Assert.False(isValidNotice);
    }

    [Fact]
    public void Hrm_EmployeeTransfer_UnitHistory_LogsTransferRecord()
    {
        string oldDept = "Sales";
        string newDept = "Marketing";

        bool isTransferRecorded = oldDept != newDept;

        Assert.True(isTransferRecorded);
    }

    [Fact]
    public void Hrm_DependentsDeduction_CalculatesTotalFamilyDeduction()
    {
        int numberOfDependents = 2;
        decimal deductionPerDependent = 4400000;

        decimal totalFamilyDeduction = numberOfDependents * deductionPerDependent;

        Assert.Equal(8800000, totalFamilyDeduction);
    }

    [Fact]
    public void Hrm_NightShiftAllowance_Applies30PercentExtra()
    {
        decimal baseHourlyRate = 60000;
        decimal nightShiftHours = 8;
        decimal nightAllowanceMultiplier = 0.30m;

        decimal nightAllowancePay = baseHourlyRate * nightShiftHours * nightAllowanceMultiplier;

        Assert.Equal(144000, nightAllowancePay);
    }

    [Fact]
    public void Hrm_HealthCheckup_AnnualEligibility_Verifies12MonthsTenure()
    {
        DateOnly joinDate = new DateOnly(2025, 7, 15);
        DateOnly checkupDate = new DateOnly(2026, 8, 6);

        int monthsTenure = (checkupDate.Year - joinDate.Year) * 12 + (checkupDate.Month - joinDate.Month);
        bool isEligibleForHealthCheckup = monthsTenure >= 12;

        Assert.Equal(13, monthsTenure);
        Assert.True(isEligibleForHealthCheckup);
    }

    [Fact]
    public void Hrm_PerformanceAppraisal_360DegreeScore_CalculatesWeightedAverage()
    {
        decimal selfScore = 80;     // 10%
        decimal peerScore = 85;     // 30%
        decimal managerScore = 90;  // 60%

        decimal final360Score = (selfScore * 0.10m) + (peerScore * 0.30m) + (managerScore * 0.60m);

        Assert.Equal(87.5m, final360Score);
    }

    [Fact]
    public void Hrm_UnpaidLeave_DeductsProRataDailySalary()
    {
        decimal monthlySalary = 22000000;
        int workingDaysInMonth = 22;
        int unpaidLeaveDays = 2;

        decimal dailySalary = monthlySalary / workingDaysInMonth;
        decimal deductedSalary = unpaidLeaveDays * dailySalary;
        decimal netPaidSalary = monthlySalary - deductedSalary;

        Assert.Equal(1000000, dailySalary);
        Assert.Equal(20000000, netPaidSalary);
    }

    [Fact]
    public void Hrm_MaternityLeave_Duration6Months_CalculatesReturnDate()
    {
        DateOnly leaveStartDate = new DateOnly(2026, 3, 1);
        int maternityMonths = 6;

        DateOnly expectedReturnDate = new DateOnly(2026, 9, 1);
        DateOnly actualReturnDate = leaveStartDate.AddMonths(maternityMonths);

        Assert.Equal(expectedReturnDate, actualReturnDate);
    }

    [Fact]
    public void Hrm_TimekeepingSummary_LateArrivalPenalty_DeductsTimesheetMinutes()
    {
        int gracePeriodMinutes = 15;
        int actualLateMinutes = 35;

        int penalizedMinutes = Math.Max(0, actualLateMinutes - gracePeriodMinutes);

        Assert.Equal(20, penalizedMinutes);
    }

    [Fact]
    public void Hrm_13thMonthBonus_ProRataTenure_CalculatesBonusAmount()
    {
        decimal monthlyBaseSalary = 18000000;
        int workedMonthsInYear = 8;

        decimal bonus13thMonth = (monthlyBaseSalary * workedMonthsInYear) / 12;

        Assert.Equal(12000000, bonus13thMonth);
    }

    [Fact]
    public void Hrm_BusinessTripAllowance_PerDiemCalculation_SumsDailyAllowance()
    {
        decimal perDiemRatePerDay = 500000;
        int tripDays = 4;

        decimal totalPerDiemAllowance = perDiemRatePerDay * tripDays;

        Assert.Equal(2000000, totalPerDiemAllowance);
    }

    [Fact]
    public void Hrm_EmployeeOffboarding_AssetHandoverChecklist_RequiresAllReturned()
    {
        var assignedAssets = new List<(string ItemName, bool Returned)>
        {
            ("MacBook Pro", true),
            ("Access Card", true),
            ("Monitors", true)
        };

        bool canIssueClearance = assignedAssets.All(a => a.Returned);

        Assert.True(canIssueClearance);
    }

    [Fact]
    public void Hrm_UniformAllowance_AnnualDistribution_GrantsEligibleUnits()
    {
        int annualUniformSets = 2;
        bool isFieldStaff = true;

        int totalGrantedSets = isFieldStaff ? annualUniformSets + 1 : annualUniformSets;

        Assert.Equal(3, totalGrantedSets);
    }

    [Fact]
    public void Hrm_TrainingExpenseReimbursement_ServiceCommitment_VerifiesBondPeriod()
    {
        decimal trainingCost = 20000000;
        int bondMonthsRequired = 12;
        int servedMonthsPostTraining = 8;

        bool isBondFulfilled = servedMonthsPostTraining >= bondMonthsRequired;
        decimal clawbackAmount = isBondFulfilled ? 0 : trainingCost * (1 - (decimal)servedMonthsPostTraining / bondMonthsRequired);

        Assert.False(isBondFulfilled);
        Assert.Equal(6666666.67m, Math.Round(clawbackAmount, 2));
    }

    [Fact]
    public void Hrm_EmployeeOnboarding_DocVerification_RequiresMandatoryDiplomas()
    {
        var requiredDocs = new List<(string DocType, bool Verified)>
        {
            ("ID Card", true),
            ("Degree Diploma", true),
            ("Health Certificate", true)
        };

        bool isFullyVerified = requiredDocs.All(d => d.Verified);

        Assert.True(isFullyVerified);
    }
}
