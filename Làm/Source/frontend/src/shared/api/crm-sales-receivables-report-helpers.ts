export function evaluateReceivableDebtRiskLevel(overduePercent: number): { label: string; badgeClass: string } {
  if (overduePercent > 40) {
    return { label: 'Rủi ro cao (Nợ xấu > 40%)', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300 font-bold' };
  }
  if (overduePercent > 20) {
    return { label: 'Cảnh báo nợ quá hạn (20-40%)', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300 font-semibold' };
  }
  return { label: 'Công nợ an toàn (< 20%)', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300 font-semibold' };
}

export function validateReportExportForm(reportName: string, recipientEmails: string): { isValid: boolean; error?: string } {
  if (!reportName || !reportName.trim()) {
    return { isValid: false, error: 'Tên báo cáo không được để trống.' };
  }
  if (!recipientEmails || !recipientEmails.includes('@')) {
    return { isValid: false, error: 'Vui lòng nhập địa chỉ email hợp lệ để nhận báo cáo.' };
  }
  return { isValid: true };
}
