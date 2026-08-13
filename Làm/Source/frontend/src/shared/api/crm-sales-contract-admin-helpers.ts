export interface ValidityBadgeResult {
  label: string;
  badgeClass: string;
}

export function evaluateContractValidityStatus(endDateStr: string): ValidityBadgeResult {
  if (!endDateStr) return { label: 'Không xác định', badgeClass: 'bg-slate-100 text-slate-800' };

  const endDate = new Date(endDateStr);
  const now = new Date();
  const diffDays = Math.ceil((endDate.getTime() - now.getTime()) / (1000 * 3600 * 24));

  if (diffDays < 0) {
    return { label: 'Đã hết hạn', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300' };
  }
  if (diffDays <= 30) {
    return { label: `Sắp hết hạn (${diffDays} ngày)`, badgeClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
  return { label: 'Còn hiệu lực', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
}

export function formatContractFileSize(bytes: number): string {
  if (!bytes || bytes <= 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
}

export function validateContractForm(code: string, value: number, customer: string): { isValid: boolean; error?: string } {
  if (!code || !code.trim()) {
    return { isValid: false, error: 'Mã hợp đồng không được để trống.' };
  }
  if (isNaN(value) || value <= 0) {
    return { isValid: false, error: 'Giá trị hợp đồng phải lớn hơn 0.' };
  }
  if (!customer || !customer.trim()) {
    return { isValid: false, error: 'Vui lòng chọn khách hàng đứng tên hợp đồng.' };
  }
  return { isValid: true };
}
