// hrm-step36-helpers.ts
// Frontend helpers cho Bước 36:
//   UC_HRM_134 — Hủy đơn nghỉ (validateLeaveCancelRequest)
//   UC_HRM_136 — Lịch nghỉ theo đơn vị (filterLeaveCalendar)
//   UC_HRM_137 — Import nghỉ lễ / ngày nghỉ (validateHolidayInput & validateHolidayImportBatch)
//   UC_HRM_138 — Báo cáo nghỉ / quỹ phép (formatHolidayStatus)

export function validateLeaveCancelRequest(requestId: string, status: string): { valid: boolean; error?: string } {
  if (!requestId?.trim())
    return { valid: false, error: 'Mã đơn nghỉ không hợp lệ.' };

  if (status === 'Cancelled' || status === 'Rejected')
    return { valid: false, error: 'Đơn nghỉ đã bị hủy hoặc bị từ chối trước đó.' };

  return { valid: true };
}

export interface LeaveCalendarEntry {
  requestId: string;
  employeeId: string;
  employeeName: string;
  orgUnitId: string;
  fromDate: string;
  toDate: string;
  status: string;
}

export function filterLeaveCalendar(
  items: LeaveCalendarEntry[],
  orgUnitId?: string,
  statusFilter?: string
): LeaveCalendarEntry[] {
  if (!items || items.length === 0) return [];
  return items.filter((item) => {
    if (orgUnitId && item.orgUnitId !== orgUnitId) return false;
    if (statusFilter && item.status !== statusFilter) return false;
    return true;
  });
}

export interface HolidayInput {
  date: string;
  name: string;
  isPaid: boolean;
}

export function validateHolidayInput(input: HolidayInput): { valid: boolean; error?: string } {
  if (!input.date?.trim())
    return { valid: false, error: 'Vui lòng chọn ngày nghỉ lễ.' };

  const name = (input.name ?? '').trim();
  if (name.length < 1 || name.length > 200)
    return { valid: false, error: 'Tên ngày nghỉ lễ phải từ 1 đến 200 ký tự.' };

  return { valid: true };
}

export function validateHolidayImportBatch(items: HolidayInput[]): { valid: boolean; error?: string } {
  if (!items || items.length === 0)
    return { valid: false, error: 'Danh sách ngày nghỉ import không được để trống.' };

  for (let i = 0; i < items.length; i++) {
    const check = validateHolidayInput(items[i]);
    if (!check.valid) {
      return { valid: false, error: `Dòng ${i + 1}: ${check.error}` };
    }
  }

  return { valid: true };
}

export function formatHolidayStatus(isPaid: boolean): string {
  return isPaid ? '🎉 Nghỉ lễ hưởng nguyên lương' : '🏖️ Nghỉ lễ không hưởng lương';
}
