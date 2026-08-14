export function getTransferApprovalStatusBadge(status: string): { label: string; colorClass: string } {
  switch (status) {
    case 'Approved':
      return { label: 'Đã Phê Duyệt', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'Rejected':
      return { label: 'Từ Chối Điều Chuyển', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    case 'PendingApproval':
    default:
      return { label: 'Chờ Ban Giám Đốc Duyệt', colorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
}

export function formatSerialLifecycleSummary(eventsCount: number, lastLocation: string): string {
  const loc = lastLocation?.trim() || 'Kho Tổng';
  return `Đã qua ${eventsCount} chặng luân chuyển (Hiện tại: ${loc})`;
}
