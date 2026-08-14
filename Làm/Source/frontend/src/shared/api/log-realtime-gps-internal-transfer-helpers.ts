export function getInternalTransferStatusBadge(status: string): { label: string; colorClass: string } {
  switch (status) {
    case 'Received':
      return { label: 'Đã Nhận Đủ', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'DiscrepancyReported':
      return { label: 'Lệch Số Lượng (Cần Đối Soát)', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    case 'InTransit':
    default:
      return { label: 'Đang Vận Chuyển', colorClass: 'bg-blue-100 text-blue-800 border-blue-300' };
  }
}

export function formatGpsCoordinates(lat: number, lng: number): string {
  return `${lat.toFixed(4)}° N, ${lng.toFixed(4)}° E`;
}
