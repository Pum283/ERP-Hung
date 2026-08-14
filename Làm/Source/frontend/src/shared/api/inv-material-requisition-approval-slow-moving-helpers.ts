export function getRequisitionStatusBadge(status: string): { label: string; colorClass: string } {
  switch (status) {
    case 'Approved':
      return { label: 'Đã Phê Duyệt', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'ConvertedToIssue':
      return { label: 'Đã Xuất Kho Cấp Phát', colorClass: 'bg-blue-100 text-blue-800 border-blue-300' };
    case 'Rejected':
      return { label: 'Từ Chối Cấp Hàng', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    case 'Submitted':
    default:
      return { label: 'Chờ Quản Lý Duyệt', colorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
}

export function getSlowMovingRiskLevelBadge(risk: string): { label: string; colorClass: string } {
  switch (risk) {
    case 'HighRisk':
      return { label: 'Nguy Cơ Cao (>180 ngày)', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    case 'MediumRisk':
      return { label: 'Cần Lưu Ý (90-180 ngày)', colorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
    case 'LowRisk':
    default:
      return { label: 'Bình Thường (<90 ngày)', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
  }
}
