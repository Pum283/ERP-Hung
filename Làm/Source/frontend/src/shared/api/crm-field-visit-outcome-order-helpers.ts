export interface OutcomeBadgeResult {
  label: string;
  badgeClass: string;
}

export function evaluateOutcomeStatusBadge(status: string): OutcomeBadgeResult {
  switch (status?.toLowerCase()) {
    case 'successful':
      return { label: 'Thành công', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'partial':
      return { label: 'Đạt một phần', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300' };
    case 'followuprequired':
      return { label: 'Cần theo dõi thêm', badgeClass: 'bg-blue-100 text-blue-800 border-blue-300' };
    case 'unsuccessful':
      return { label: 'Không thành công', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    default:
      return { label: 'Đã hoàn thành', badgeClass: 'bg-slate-100 text-slate-800 border-slate-300' };
  }
}

export function calculateOnSiteOrderTotal(items: { qty: number; price: number }[]): number {
  if (!items || items.length === 0) return 0;
  return items.reduce((acc, item) => acc + (item.qty || 0) * (item.price || 0), 0);
}

export function validateDemandEntry(category: string, qty: number): { isValid: boolean; error?: string } {
  if (!category || !category.trim()) {
    return { isValid: false, error: 'Vui lòng nhập nhóm sản phẩm/dịch vụ quan tâm.' };
  }
  if (isNaN(qty) || qty <= 0) {
    return { isValid: false, error: 'Số lượng dự kiến nhu cầu phải lớn hơn 0.' };
  }
  return { isValid: true };
}
