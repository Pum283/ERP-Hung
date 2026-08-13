export interface StatusBadgeResult {
  label: string;
  badgeClass: string;
}

export function evaluateCommissionPeriodStatusBadge(status: string): StatusBadgeResult {
  switch (status?.toLowerCase()) {
    case 'syncedtohrmfin':
      return { label: 'Đã đồng bộ HRM/FIN', badgeClass: 'bg-brand-muted text-brand-strong border-brand/30 font-semibold' };
    case 'approved':
      return { label: 'Đã duyệt chi trả', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300 font-semibold' };
    case 'calculated':
      return { label: 'Đã tính toán', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300 font-semibold' };
    default:
      return { label: 'Bản nháp (Draft)', badgeClass: 'bg-slate-100 text-slate-800 border-slate-300 font-semibold' };
  }
}

export function formatLeaderboardRankBadge(rank: number): { label: string; badgeClass: string } {
  if (rank === 1) return { label: '🥇 Hạng 1 (Quán quân)', badgeClass: 'bg-amber-100 text-amber-900 border-amber-400 font-bold' };
  if (rank === 2) return { label: '🥈 Hạng 2 (Á quân 1)', badgeClass: 'bg-slate-200 text-slate-800 border-slate-400 font-bold' };
  if (rank === 3) return { label: '🥉 Hạng 3 (Á quân 2)', badgeClass: 'bg-amber-800/10 text-amber-900 border-amber-600/30 font-bold' };
  return { label: `Top ${rank}`, badgeClass: 'bg-slate-100 text-slate-700 border-slate-200 font-medium' };
}

export function validateCommissionPeriodForm(periodCode: string, startDateStr: string, endDateStr: string): { isValid: boolean; error?: string } {
  if (!periodCode || !periodCode.trim()) {
    return { isValid: false, error: 'Mã kỳ tính hoa hồng không được để trống.' };
  }
  if (!startDateStr || !endDateStr) {
    return { isValid: false, error: 'Ngày bắt đầu và ngày kết thúc kỳ không được để trống.' };
  }
  const start = new Date(startDateStr);
  const end = new Date(endDateStr);
  if (start > end) {
    return { isValid: false, error: 'Ngày bắt đầu kỳ phải nhỏ hơn hoặc bằng ngày kết thúc.' };
  }
  return { isValid: true };
}
