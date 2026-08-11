// hrm-step25-helpers.ts
// Frontend helpers cho Bước 25:
//   UC_HRM_084 — Đổi ca giữa nhân viên (assignmentAId != assignmentBId)
//   UC_HRM_085 — Hủy lịch ca (assignmentId required)
//   UC_HRM_086 — Xem lịch ca theo đơn vị (orgUnitId & date range filter)
//   UC_HRM_087 — Xem lịch ca cá nhân trên APP (personal roster helper)

export function validateShiftSwap(assignmentAId: string, assignmentBId: string): { valid: boolean; error?: string } {
  const a = (assignmentAId ?? '').trim();
  const b = (assignmentBId ?? '').trim();

  if (!a || !b)
    return { valid: false, error: 'Vui lòng chọn đủ 2 lịch ca để thực hiện đổi ca.' };

  if (a === b)
    return { valid: false, error: 'Không thể tự đổi ca với chính ca làm việc đó.' };

  return { valid: true };
}

export function validateShiftCancel(assignmentId: string): { valid: boolean; error?: string } {
  if (!assignmentId?.trim())
    return { valid: false, error: 'Vui lòng chọn lịch ca cần hủy.' };

  return { valid: true };
}

export interface ShiftItem {
  id: string;
  workDate: string;
  status: string;
}

export function filterPersonalRoster(items: ShiftItem[], from?: string, to?: string, includeCancelled = false): ShiftItem[] {
  if (!items || items.length === 0) return [];

  return items.filter((x) => {
    if (!includeCancelled && x.status === 'Cancelled') return false;
    if (from && x.workDate < from) return false;
    if (to && x.workDate > to) return false;
    return true;
  });
}
