export function evaluateOfflineSyncBadgeStatus(syncStatus: string): { label: string; badgeClass: string } {
  switch (syncStatus?.toLowerCase()) {
    case 'synced':
      return { label: 'Đồng bộ hoàn tất', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300 font-semibold' };
    case 'syncing':
      return { label: 'Đang tiến hành đồng bộ...', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300 font-semibold' };
    case 'syncerror':
      return { label: 'Lỗi đồng bộ đệm', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300 font-bold' };
    default:
      return { label: 'Chờ đồng bộ (Pending)', badgeClass: 'bg-slate-100 text-slate-800 border-slate-300 font-semibold' };
  }
}

export function validateScannerConfigForm(scannerName: string): { isValid: boolean; error?: string } {
  if (!scannerName || !scannerName.trim()) {
    return { isValid: false, error: 'Tên thiết bị đầu quét mã vạch không được để trống.' };
  }
  return { isValid: true };
}
