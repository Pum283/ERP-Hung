// hrm-step46-helpers.ts
// Frontend helpers cho Bước 46:
//   UC_HRM_176 — So sánh lương kỳ này / kỳ trước (calculatePayrollCompareDelta)
//   UC_HRM_182 — Dashboard headcount & biến động (formatHeadcountMovementSummary)
//   UC_HRM_183 — Báo cáo công / OT / đi trễ (calculateAttendanceSummary)
//   UC_HRM_184 — Báo cáo tuyển dụng funnel (formatRecruitFunnelStage)

export interface PayrollCompareDelta {
  grossDiff: number;
  grossDiffPct: number;
  netDiff: number;
  netDiffPct: number;
}

export function calculatePayrollCompareDelta(grossCur: number, grossPrev: number, netCur: number, netPrev: number): PayrollCompareDelta {
  const grossDiff = grossCur - grossPrev;
  const grossDiffPct = grossPrev > 0 ? Math.round((grossDiff / grossPrev) * 10000) / 100 : 0;

  const netDiff = netCur - netPrev;
  const netDiffPct = netPrev > 0 ? Math.round((netDiff / netPrev) * 10000) / 100 : 0;

  return { grossDiff, grossDiffPct, netDiff, netDiffPct };
}

export function formatHeadcountMovementSummary(hired: number, resigned: number): string {
  const netChange = hired - resigned;
  const sign = netChange > 0 ? '+' : '';
  return `📈 Tuyển mới: ${hired} | 📉 Nghỉ việc: ${resigned} | 🔄 Biến động ròng: ${sign}${netChange} nhân sự`;
}

export interface AttendanceReportSummary {
  totalWorkDays: number;
  totalOtHours: number;
  totalLateMinutes: number;
  totalLateCount: number;
}

export function calculateAttendanceSummary(rows: { workUnits: number; otMinutes: number; lateMinutes: number; lateCount: number }[]): AttendanceReportSummary {
  const totalWorkDays = rows.reduce((sum, r) => sum + r.workUnits, 0);
  const totalOtMinutes = rows.reduce((sum, r) => sum + r.otMinutes, 0);
  const totalLateMinutes = rows.reduce((sum, r) => sum + r.lateMinutes, 0);
  const totalLateCount = rows.reduce((sum, r) => sum + r.lateCount, 0);

  return {
    totalWorkDays,
    totalOtHours: Math.round((totalOtMinutes / 60) * 10) / 10,
    totalLateMinutes,
    totalLateCount,
  };
}

export function formatRecruitFunnelStage(status: string): string {
  switch (status) {
    case 'Applied':
      return '📥 Hố sơ mới ứng tuyển';
    case 'Screening':
      return '🔍 Sàng lọc hồ sơ';
    case 'Interviewing':
      return '🎙️ Đang phỏng vấn';
    case 'Offered':
      return '✉️ Gửi thư mời làm việc';
    case 'Hired':
      return '🎉 Đã nhận việc';
    case 'Rejected':
      return '❌ Từ chối / Không đạt';
    default:
      return status || 'Khác';
  }
}
