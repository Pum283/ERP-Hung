// crm-step68-helpers.ts
// Frontend helpers cho Bước 68:
//   UC_CRM_056 — Nhật ký chăm sóc lead (formatActivityTypeBadge)
//   UC_CRM_057 — Chuyển lead thành cơ hội (formatConversionResult)
//   UC_CRM_058 — Gộp lead trùng (validateMergeLeadRequest)
//   UC_CRM_059 — Báo cáo chuyển đổi lead (formatConversionReportSummary)

export function formatActivityTypeBadge(activityType?: string): { label: string; icon: string } {
  switch ((activityType || '').trim()) {
    case 'Call':
      return { label: 'Cuộc gọi điện', icon: '📞' };
    case 'Email':
      return { label: 'Email trao đổi', icon: '✉️' };
    case 'Meeting':
      return { label: 'Cuộc họp trực tiếp / Online', icon: '🤝' };
    case 'Note':
      return { label: 'Ghi chú nhanh', icon: '📝' };
    default:
      return { label: 'Nhật ký tương tác', icon: '📌' };
  }
}

export function formatConversionResult(opportunityName: string, opportunityId: string): string {
  return `🎉 Chuyển đổi thành công! Cơ hội mới: "${opportunityName}" (ID: ${opportunityId.substring(0, 8)}...)`;
}

export function validateMergeLeadRequest(targetLeadId: string, sourceLeadId: string): { isValid: boolean; error?: string } {
  if (!targetLeadId || !sourceLeadId) {
    return { isValid: false, error: 'Phải chọn đầy đủ Lead gốc và Lead trùng cần gộp.' };
  }

  if (targetLeadId.trim() === sourceLeadId.trim()) {
    return { isValid: false, error: 'Lead gốc và Lead trùng không được là cùng một đối tượng.' };
  }

  return { isValid: true };
}

export function formatConversionReportSummary(report: { totalLeads: number; convertedLeads: number; conversionRatePct: number }): string {
  const total = report.totalLeads || 0;
  const converted = report.convertedLeads || 0;
  const rate = report.conversionRatePct || (total > 0 ? Math.round((converted / total) * 100) : 0);

  return `📊 Tổng số lead: ${total} | Đã chuyển đổi cơ hội: ${converted} (${rate}%)`;
}
