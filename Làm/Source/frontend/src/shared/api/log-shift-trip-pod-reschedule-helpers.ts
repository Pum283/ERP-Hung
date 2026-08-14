export function getTripStatusBadge(status: string): { label: string; colorClass: string } {
  switch (status) {
    case 'InTransit':
      return { label: 'Đang Giao Hàng', colorClass: 'bg-blue-100 text-blue-800 border-blue-300' };
    case 'Completed':
      return { label: 'Đã Hoàn Tất Chuyến', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'Planned':
    default:
      return { label: 'Lên Kế Hoạch Chuyến', colorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
}

export function formatShiftTiming(start: string, end: string): string {
  return `${start} - ${end}`;
}
