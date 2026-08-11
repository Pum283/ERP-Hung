// hrm-step38-helpers.ts
// Frontend helpers cho Bước 38:
//   UC_HRM_143 — Báo cáo khen thưởng – kỷ luật (filterOffboardingCases)
//   UC_HRM_144 — Tạo đơn nghỉ việc (validateOffboardingCreateRequest)
//   UC_HRM_145 — Cấu hình / kiểm tra báo trước (validateNoticePeriodConfig & calculateNoticeDaysLeft)
//   UC_HRM_146 — Duyệt đơn nghỉ việc (formatNoticeStatus)

export interface OffboardingCreateInput {
  employeeId: string;
  requestDate: string;
  lastWorkingDay: string;
  reasonCode?: string;
  reasonDetail?: string;
}

export function validateOffboardingCreateRequest(input: OffboardingCreateInput): { valid: boolean; error?: string } {
  if (!input.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên làm thủ tục nghỉ việc.' };

  if (!input.requestDate?.trim() || !input.lastWorkingDay?.trim())
    return { valid: false, error: 'Vui lòng nhập đầy đủ ngày nộp đơn và ngày làm việc cuối cùng.' };

  if (input.lastWorkingDay < input.requestDate)
    return { valid: false, error: 'Ngày làm việc cuối cùng phải lớn hơn hoặc bằng ngày nộp đơn.' };

  return { valid: true };
}

export function validateNoticePeriodConfig(noticeDays: number): { valid: boolean; error?: string } {
  if (isNaN(noticeDays) || noticeDays < 0 || noticeDays > 365)
    return { valid: false, error: 'Số ngày quy định báo trước phải từ 0 đến 365 ngày.' };

  return { valid: true };
}

export function calculateNoticeDaysLeft(requestDateIso: string, lastWorkingDayIso: string): number {
  if (!requestDateIso || !lastWorkingDayIso) return 0;
  const d1 = new Date(requestDateIso).getTime();
  const d2 = new Date(lastWorkingDayIso).getTime();
  const diffDays = Math.ceil((d2 - d1) / (1000 * 3600 * 24));
  return Math.max(0, diffDays);
}

export function formatNoticeStatus(noticeSatisfied: boolean, actualDays: number, requiredDays: number): string {
  if (noticeSatisfied) {
    return `✅ Đảm bảo báo trước (${actualDays}/${requiredDays} ngày)`;
  }
  return `⚠️ Khắc phục báo trước thiếu (${actualDays}/${requiredDays} ngày)`;
}
