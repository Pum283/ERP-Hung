// crm-step60-helpers.ts
// Frontend helpers cho Bước 60:
//   UC_CRM_014 — Import / export khách hàng (generateCustomerCsvExportFileName, validateCsvImportLine)
//   UC_CRM_015 — Tìm kiếm khách đa tiêu chí (validateCsvImportLine)
//   UC_CRM_016 — Tạo campaign marketing (formatCampaignChannelBadge)
//   UC_CRM_017 — Quản lý nhóm quảng cáo (formatUtmSummary)

export function generateCustomerCsvExportFileName(tenantCode = 'ERP'): string {
  const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '');
  return `${tenantCode}_DanhSach_KhachHang_${dateStr}.csv`;
}

export function validateCsvImportLine(csvLine: string, lineNumber: number): { isValid: boolean; error?: string; parsedCols?: string[] } {
  if (!csvLine || !csvLine.trim()) {
    return { isValid: false, error: `Dòng ${lineNumber}: Dòng trống.` };
  }

  const cols = csvLine.split(',').map(c => c.trim().replace(/^"|"$/g, ''));
  if (cols.length < 3) {
    return { isValid: false, error: `Dòng ${lineNumber}: Thiếu cột bắt buộc (cần ít nhất Mã, Loại KH, Tên hiển thị).` };
  }

  const code = cols[0];
  if (!code || code.length > 40) {
    return { isValid: false, error: `Dòng ${lineNumber}: Mã khách hàng không hợp lệ (1-40 ký tự).` };
  }

  return { isValid: true, parsedCols: cols };
}

export function formatCampaignChannelBadge(channel?: string): { label: string; icon: string } {
  switch ((channel || '').trim().toLowerCase()) {
    case 'email':
      return { label: 'Email Marketing', icon: '📧' };
    case 'social':
      return { label: 'Mạng xã hội (Social)', icon: '🌐' };
    case 'sem':
      return { label: 'Tìm kiếm trả phí (SEM)', icon: '🔍' };
    case 'event':
      return { label: 'Sự kiện / Hội thảo', icon: '🎪' };
    default:
      return { label: 'Kênh khác', icon: '📢' };
  }
}

export function formatUtmSummary(utmSource?: string, utmMedium?: string, utmCampaign?: string): string {
  const source = utmSource || 'Direct';
  const medium = utmMedium || 'None';
  const campaign = utmCampaign || 'Organic';
  return `🔗 UTM: ${source} / ${medium} (${campaign})`;
}
