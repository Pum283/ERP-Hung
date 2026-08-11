// hrm-step32-helpers.ts
// Frontend helpers cho Bước 32:
//   UC_HRM_113 — Bảng chấm công toàn công ty (filterCompanyWideBoard with searchKeyword & orgUnit filter)
//   UC_HRM_114 — Cảnh báo thiếu chấm realtime (formatMissingAlertMessage for MissingCheckIn & MissingCheckout)
//   UC_HRM_115 — Tự tính phút đi trễ (calculateLateMinutes)
//   UC_HRM_116 — Tự trừ công do đi trễ (calculatePenaltyDeduction)

export function calculateLateMinutes(scheduledStartIso: string, actualCheckInIso: string): number {
  if (!scheduledStartIso || !actualCheckInIso) return 0;
  const start = new Date(scheduledStartIso).getTime();
  const checkIn = new Date(actualCheckInIso).getTime();
  if (isNaN(start) || isNaN(checkIn)) return 0;
  if (checkIn <= start) return 0;
  return Math.ceil((checkIn - start) / (1000 * 60));
}

export interface PenaltyDeductionResult {
  deductedWorkUnit: number;
  workUnit: number;
}

export function calculatePenaltyDeduction(
  lateMinutes: number,
  graceMinutes: number,
  deductEveryMinutes: number,
  deductWorkUnit: number
): PenaltyDeductionResult {
  if (lateMinutes <= graceMinutes || deductEveryMinutes <= 0) {
    return { deductedWorkUnit: 0, workUnit: 1.0 };
  }

  const excessLate = lateMinutes - graceMinutes;
  const blocks = Math.ceil(excessLate / deductEveryMinutes);
  const deducted = Math.min(1.0, Math.round(blocks * deductWorkUnit * 100) / 100);
  const workUnit = Math.max(0.0, Math.round((1.0 - deducted) * 100) / 100);

  return { deductedWorkUnit: deducted, workUnit };
}

export interface CompanyBoardItem {
  id: string;
  employeeCode: string;
  employeeName: string;
  orgUnitId: string;
  orgUnitName: string;
  workDate: string;
  status: string;
}

export function filterCompanyWideBoard(
  records: CompanyBoardItem[],
  searchKeyword?: string,
  orgUnitId?: string,
  from?: string,
  to?: string
): CompanyBoardItem[] {
  if (!records || records.length === 0) return [];
  const kw = (searchKeyword ?? '').trim().toLowerCase();

  return records.filter((r) => {
    if (orgUnitId && r.orgUnitId !== orgUnitId) return false;
    if (from && r.workDate < from) return false;
    if (to && r.workDate > to) return false;
    if (kw) {
      const matchCode = r.employeeCode.toLowerCase().includes(kw);
      const matchName = r.employeeName.toLowerCase().includes(kw);
      const matchOu = r.orgUnitName.toLowerCase().includes(kw);
      if (!matchCode && !matchName && !matchOu) return false;
    }
    return true;
  });
}

export function formatMissingAlertMessage(alertType: string, employeeName: string, workDate: string): string {
  const dateStr = workDate || 'hôm nay';
  if (alertType === 'MissingCheckIn') {
    return `⚠️ Cảnh báo: Nhân viên ${employeeName} chưa check-in ca làm việc ngày ${dateStr}.`;
  }
  if (alertType === 'MissingCheckout') {
    return `⚠️ Cảnh báo: Nhân viên ${employeeName} chưa check-out (quá giờ quy định) ngày ${dateStr}.`;
  }
  return `⚠️ Cảnh báo bất thường chấm công ngày ${dateStr} cho ${employeeName}.`;
}
