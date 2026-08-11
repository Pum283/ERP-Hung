// hrm-step33-helpers.ts
// Frontend helpers cho Bước 33:
//   UC_HRM_117 — Đánh dấu quên chấm (filterMissingAlertsByEmployee)
//   UC_HRM_119 — Xử lý công OT tự động (formatOvertimeHours)
//   UC_HRM_120 — Tạo phiếu xin điều chỉnh công (validateAdjustCreateRequest)
//   UC_HRM_121 — Đính kèm lý do / bằng chứng (validateEvidenceStorageKey)

export interface AdjustCreateInput {
  employeeId: string;
  workDate: string;
  reason: string;
  evidenceStorageKey?: string;
  submit?: boolean;
}

export function validateAdjustCreateRequest(input: AdjustCreateInput): { valid: boolean; error?: string } {
  if (!input.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên.' };

  if (!input.workDate?.trim())
    return { valid: false, error: 'Vui lòng chọn ngày công điều chỉnh.' };

  const reason = (input.reason ?? '').trim();
  if (reason.length < 3 || reason.length > 500)
    return { valid: false, error: 'Lý do điều chỉnh công phải từ 3 đến 500 ký tự.' };

  if (input.evidenceStorageKey !== undefined && input.evidenceStorageKey !== null) {
    const keyVal = validateEvidenceStorageKey(input.evidenceStorageKey);
    if (!keyVal.valid) return keyVal;
  }

  return { valid: true };
}

export function validateEvidenceStorageKey(storageKey: string): { valid: boolean; error?: string } {
  const key = (storageKey ?? '').trim();
  if (!key) return { valid: true };

  if (key.length > 250)
    return { valid: false, error: 'Đường dẫn bằng chứng (Storage Key) tối đa 250 ký tự.' };

  if (/[\<\>\:\"\\\|\?\*]/.test(key))
    return { valid: false, error: 'Đường dẫn bằng chứng chứa ký tự cấm không hợp lệ.' };

  return { valid: true };
}

export function formatOvertimeHours(otMinutes: number): string {
  if (isNaN(otMinutes) || otMinutes <= 0) return '0 giờ';
  const hours = Math.floor(otMinutes / 60);
  const mins = otMinutes % 60;
  if (hours > 0 && mins > 0) return `${hours} giờ ${mins} phút`;
  if (hours > 0) return `${hours} giờ`;
  return `${mins} phút`;
}

export interface MissingAlertItem {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  alertType: string;
}

export function filterMissingAlertsByEmployee(alerts: MissingAlertItem[], searchKeyword?: string): MissingAlertItem[] {
  if (!alerts || alerts.length === 0) return [];
  const kw = (searchKeyword ?? '').trim().toLowerCase();
  if (!kw) return alerts;

  return alerts.filter(
    (a) => a.employeeCode.toLowerCase().includes(kw) || a.employeeName.toLowerCase().includes(kw)
  );
}
