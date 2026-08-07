/** Pure helpers — CRM auto-intake (UC_CRM_050) + HRM device sync (UC_HRM_118). */

export function canAutoIntake(name: string, phone?: string | null, email?: string | null): boolean {
  if (!name.trim()) return false;
  return !!(phone?.trim() || email?.trim());
}

export function formatAutoIntakeFlash(opts: {
  code: string;
  intakeChannel?: string | null;
  ownerName?: string | null;
  isReintake?: boolean;
}): string {
  if (opts.isReintake) return `Re-intake lead ${opts.code} (trùng SĐT/Email — đã ghi hoạt động).`;
  const owner = opts.ownerName ? ` · owner ${opts.ownerName}` : " · chưa phân bổ";
  return `Auto-intake ${opts.code}${owner}`;
}

export type DeviceSyncSummary = {
  synced: number;
  skippedUnknownEmployee: number;
  skippedLocked: number;
  skippedDuplicate: number;
  skippedInvalidType: number;
  total: number;
};

export function formatDeviceSyncFlash(r: DeviceSyncSummary): string {
  const parts = [`${r.synced}/${r.total} áp dụng`];
  if (r.skippedUnknownEmployee) parts.push(`${r.skippedUnknownEmployee} NV lạ`);
  if (r.skippedLocked) parts.push(`${r.skippedLocked} kỳ khóa`);
  if (r.skippedDuplicate) parts.push(`${r.skippedDuplicate} trùng`);
  if (r.skippedInvalidType) parts.push(`${r.skippedInvalidType} loại lỗi`);
  return `Đồng bộ máy: ${parts.join(" · ")}`;
}
