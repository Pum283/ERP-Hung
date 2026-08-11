// hrm-step31-helpers.ts
// Frontend helpers cho Bước 31:
//   UC_HRM_109 — Check-in đầu ca (method in ["App", "Qr", "Fingerprint", "DeviceSync", "Manual"])
//   UC_HRM_110 — Check-out cuối ca (punch request validation)
//   UC_HRM_111 — Xem lịch sử chấm cá nhân (date range validation: from <= to, span <= 366 days)
//   UC_HRM_112 — Bảng chấm công theo đơn vị (formatAttendanceStatus & filterDepartmentBoard)

export interface PunchInput {
  method: string;
  latitude?: number;
  longitude?: number;
  note?: string;
}

export function validatePunchRequest(input: PunchInput): { valid: boolean; error?: string } {
  const method = (input.method ?? '').trim();
  if (!method)
    return { valid: false, error: 'Vui lòng chọn phương thức chấm công.' };

  const validMethods = new Set(['App', 'Qr', 'Fingerprint', 'DeviceSync', 'Manual']);
  if (!validMethods.has(method))
    return { valid: false, error: 'Phương thức chấm công không hợp lệ.' };

  if (input.latitude !== undefined && input.latitude !== null) {
    if (isNaN(input.latitude) || input.latitude < -90 || input.latitude > 90)
      return { valid: false, error: 'Tọa độ vĩ độ không hợp lệ.' };
  }

  if (input.longitude !== undefined && input.longitude !== null) {
    if (isNaN(input.longitude) || input.longitude < -180 || input.longitude > 180)
      return { valid: false, error: 'Tọa độ kinh độ không hợp lệ.' };
  }

  return { valid: true };
}

export function validateDateRangeFilter(from?: string, to?: string): { valid: boolean; error?: string } {
  if (!from || !to) return { valid: true };

  const f = new Date(from);
  const t = new Date(to);
  if (t < f)
    return { valid: false, error: 'Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.' };

  const spanDays = Math.ceil((t.getTime() - f.getTime()) / (1000 * 3600 * 24));
  if (spanDays > 366)
    return { valid: false, error: 'Khoảng thời gian xem tối đa 1 năm (366 ngày).' };

  return { valid: true };
}

export function formatAttendanceStatus(status: string): string {
  switch (status) {
    case 'Open':
      return 'Đang làm việc';
    case 'Closed':
      return 'Đã hoàn thành';
    case 'Missing':
      return 'Thiếu chấm công';
    case 'Adjusted':
      return 'Đã điều chỉnh';
    default:
      return status || 'Khác';
  }
}

export interface AttendanceRecordItem {
  id: string;
  orgUnitId: string;
  workDate: string;
  status: string;
}

export function filterDepartmentBoard(
  records: AttendanceRecordItem[],
  orgUnitId?: string,
  from?: string,
  to?: string
): AttendanceRecordItem[] {
  if (!records || records.length === 0) return [];
  return records.filter((r) => {
    if (orgUnitId && r.orgUnitId !== orgUnitId) return false;
    if (from && r.workDate < from) return false;
    if (to && r.workDate > to) return false;
    return true;
  });
}
