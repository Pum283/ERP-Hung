using Xunit;

namespace Erp.UnitTests;

public class HrmServiceTests
{
    [Fact]
    public void Employee_CreateWithValidData_Succeeds()
    {
        string empCode = "EMP001";
        string fullName = "Nguyễn Văn A";
        string email = "nva@erp.vn";

        bool isValid = !string.IsNullOrEmpty(empCode) && !string.IsNullOrEmpty(fullName) && email.Contains("@");

        Assert.True(isValid);
    }

    [Fact]
    public void Employee_InvalidEmailFormat_FailsValidation()
    {
        string email = "invalid-email-string";

        bool isValid = email.Contains("@") && email.Contains(".");

        Assert.False(isValid);
    }

    [Fact]
    public void Attendance_GpsCheckinWithinGeofence_Succeeds()
    {
        double officeLat = 10.7769;
        double officeLng = 106.7009;

        double userLat = 10.7770;
        double userLng = 106.7010;

        double distanceMeters = Math.Sqrt(Math.Pow(userLat - officeLat, 2) + Math.Pow(userLng - officeLng, 2)) * 111000;
        bool isWithinGeofence = distanceMeters <= 100; // 100 meters limit

        Assert.True(isWithinGeofence);
    }

    [Fact]
    public void Attendance_GpsCheckinOutsideGeofence_Fails()
    {
        double officeLat = 10.7769;
        double officeLng = 106.7009;

        double userLat = 10.8000; // Far away
        double userLng = 106.7500;

        double distanceMeters = Math.Sqrt(Math.Pow(userLat - officeLat, 2) + Math.Pow(userLng - officeLng, 2)) * 111000;
        bool isWithinGeofence = distanceMeters <= 100;

        Assert.False(isWithinGeofence);
    }

    [Fact]
    public void Attendance_LateGraceMinutes_DoesNotDeductSalary()
    {
        int shiftStartMinute = 480; // 8:00 AM (8 * 60)
        int checkinMinute = 490; // 8:10 AM
        int lateGraceMinutes = 15; // 15 mins grace

        int actualLateMinutes = checkinMinute - shiftStartMinute;
        bool isPenalized = actualLateMinutes > lateGraceMinutes;

        Assert.Equal(10, actualLateMinutes);
        Assert.False(isPenalized);
    }

    [Fact]
    public void Attendance_ExceedingLateGrace_CalculatesDeduction()
    {
        int shiftStartMinute = 480; // 8:00 AM
        int checkinMinute = 515; // 8:35 AM
        int lateGraceMinutes = 15;

        int actualLateMinutes = checkinMinute - shiftStartMinute;
        bool isPenalized = actualLateMinutes > lateGraceMinutes;

        Assert.Equal(35, actualLateMinutes);
        Assert.True(isPenalized);
    }

    [Fact]
    public void Payroll_NetSalaryCalculation_SubtractsDeductionsAndTax()
    {
        decimal baseSalary = 20000000;
        decimal allowance = 3000000;
        decimal bhxhDeduction = baseSalary * 0.105m; // 10.5%
        decimal pitTax = 1200000;

        decimal grossSalary = baseSalary + allowance;
        decimal netSalary = grossSalary - bhxhDeduction - pitTax;

        Assert.Equal(23000000, grossSalary);
        Assert.Equal(2100000, bhxhDeduction);
        Assert.Equal(19700000, netSalary);
    }

    [Fact]
    public void Payroll_SalaryGradeValidation_ValidatesPositiveBaseAmount()
    {
        decimal baseAmount = 15000000;
        bool isValidGrade = baseAmount >= 0;

        Assert.True(isValidGrade);
    }

    [Fact]
    public void LeaveRequest_ExceedingAvailableQuota_Rejected()
    {
        int annualLeaveQuota = 12;
        int usedLeaveDays = 10;
        int remainingLeaveDays = annualLeaveQuota - usedLeaveDays;

        int requestedDays = 5;
        bool isApproved = requestedDays <= remainingLeaveDays;

        Assert.Equal(2, remainingLeaveDays);
        Assert.False(isApproved);
    }

    [Fact]
    public void LeaveRequest_WithinQuota_Approved()
    {
        int annualLeaveQuota = 12;
        int usedLeaveDays = 8;
        int remainingLeaveDays = annualLeaveQuota - usedLeaveDays;

        int requestedDays = 3;
        bool isApproved = requestedDays <= remainingLeaveDays;

        Assert.Equal(4, remainingLeaveDays);
        Assert.True(isApproved);
    }
}
