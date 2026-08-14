export function getLotTraceDirectionLabel(direction: string): { label: string; colorClass: string } {
  if (direction === 'Forward') {
    return { label: 'Truy Vết Xuôi (NCC ➔ SX ➔ Khách Hàng)', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
  }
  return { label: 'Truy Vết Ngược (Khách Hàng ➔ SX ➔ NCC)', colorClass: 'bg-blue-100 text-blue-800 border-blue-300' };
}

export function getStocktakeLockStatusPill(isLocked: boolean): { label: string; colorClass: string } {
  if (isLocked) {
    return { label: '🔒 ĐANG KHÓA GIAO DỊCH (KIỂM KÊ)', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
  }
  return { label: '🔓 ĐANG MỞ (GIAO DỊCH BÌNH THƯỜNG)', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
}
