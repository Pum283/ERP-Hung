// hrm-step27-helpers.ts
// Frontend helpers cho Bước 27:
//   UC_HRM_093 — Đề xuất nhu cầu điều động (From != To, RequestedHeadcount 1-1000, Reason 3-500 chars)
//   UC_HRM_094 — Nhận lệnh điều động trên APP (orderId required, status == "Issued")
//   UC_HRM_095 — Theo dõi nhân sự điều động (active tracking filter)
//   UC_HRM_096 — Gắn nhãn công điều động khi chấm (attendanceTag validation)

export interface MobilizationRequestForm {
  fromOrgUnitId: string;
  toOrgUnitId: string;
  startDate: string;
  requestedHeadcount: number;
  reason: string;
}

export function validateMobilizationRequest(form: MobilizationRequestForm): { valid: boolean; error?: string } {
  if (!form.fromOrgUnitId?.trim() || !form.toOrgUnitId?.trim())
    return { valid: false, error: 'Vui lòng chọn đơn vị nguồn và đơn vị đích.' };

  if (form.fromOrgUnitId.trim() === form.toOrgUnitId.trim())
    return { valid: false, error: 'Đơn vị nguồn và đơn vị đích phải khác nhau.' };

  if (!form.startDate?.trim())
    return { valid: false, error: 'Vui lòng chọn ngày bắt đầu dự kiến.' };

  if (isNaN(form.requestedHeadcount) || form.requestedHeadcount < 1 || form.requestedHeadcount > 1000)
    return { valid: false, error: 'Số lượng nhân sự đề xuất phải từ 1 đến 1,000 người.' };

  const reason = (form.reason ?? '').trim();
  if (reason.length < 3 || reason.length > 500)
    return { valid: false, error: 'Lý do đề xuất điều động từ 3 đến 500 ký tự.' };

  return { valid: true };
}

export function validateOrderAcknowledge(orderId: string, currentStatus?: string): { valid: boolean; error?: string } {
  if (!orderId?.trim())
    return { valid: false, error: 'Vui lòng chọn lệnh điều động cần nhận.' };

  if (currentStatus && currentStatus !== 'Issued')
    return { valid: false, error: 'Chỉ nhận lệnh điều động đang ở trạng thái Được phát hành (Issued).' };

  return { valid: true };
}

export interface MobilizationItem {
  id: string;
  kind: string;
  status: string;
}

export function filterActiveTracking(items: MobilizationItem[]): MobilizationItem[] {
  if (!items || items.length === 0) return [];
  const activeStatuses = new Set(['Issued', 'Acknowledged', 'Active']);
  return items.filter((x) => x.kind === 'Order' && activeStatuses.has(x.status));
}

export function validateAttendanceTag(orderId: string, attendanceTagged: boolean): { valid: boolean; error?: string } {
  if (!orderId?.trim())
    return { valid: false, error: 'Vui lòng chọn lệnh điều động.' };

  if (typeof attendanceTagged !== 'boolean')
    return { valid: false, error: 'Cơ chế gắn nhãn chấm công không hợp lệ.' };

  return { valid: true };
}
