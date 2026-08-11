// crm-step69-helpers.ts
// Frontend helpers cho Bước 69:
//   UC_CRM_060 — Import lead Excel/CSV (validateLeadCsvImport, formatLeadImportSummary)
//   UC_CRM_061 — Báo cáo chuyển đổi lead (formatLeadImportSummary)
//   UC_CRM_062 — Tạo cơ hội từ lead/khách (validateOpportunityInput)
//   UC_CRM_063 — Pipeline cơ hội theo giai đoạn (formatOpportunityStageBadge)

export function validateLeadCsvImport(csvContent: string): { isValid: boolean; error?: string; lineCount: number } {
  const content = (csvContent || '').trim();
  if (!content) {
    return { isValid: false, error: 'Nội dung file CSV không được để trống.', lineCount: 0 };
  }

  const lines = content.split('\n').map(l => l.trim()).filter(l => l.length > 0);
  if (lines.length < 2) {
    return { isValid: false, error: 'File CSV phải chứa ít nhất 1 dòng tiêu đề và 1 dòng dữ liệu.', lineCount: lines.length };
  }

  const header = lines[0].toLowerCase();
  if (!header.includes('name') && !header.includes('tên')) {
    return { isValid: false, error: 'File CSV thiếu cột bắt buộc "Name" hoặc "Tên".', lineCount: lines.length };
  }

  return { isValid: true, lineCount: lines.length - 1 };
}

export function formatLeadImportSummary(created: number, skipped: number, errors: string[]): string {
  const total = created + skipped;
  const errNotice = errors.length > 0 ? ` (Cảnh báo: ${errors.length} lỗi)` : '';
  return `📥 Import thành công ${created}/${total} lead${errNotice}`;
}

export function validateOpportunityInput(input: { name: string; estimatedValue?: number; probabilityPercent?: number }): { isValid: boolean; error?: string } {
  const name = (input.name || '').trim();
  if (!name || name.length > 200) {
    return { isValid: false, error: 'Tên cơ hội bán hàng là bắt buộc và tối đa 200 ký tự.' };
  }

  if (input.estimatedValue !== undefined && input.estimatedValue < 0) {
    return { isValid: false, error: 'Giá trị ước tính không được nhỏ hơn 0.' };
  }

  if (input.probabilityPercent !== undefined && (input.probabilityPercent < 0 || input.probabilityPercent > 100)) {
    return { isValid: false, error: 'Xác suất thành công phải từ 0% đến 100%.' };
  }

  return { isValid: true };
}

export function formatOpportunityStageBadge(stage?: string): { label: string; color: string; probability: number } {
  switch ((stage || '').trim()) {
    case 'Prospecting':
      return { label: '🔍 Tìm hiểu nhu cầu (Prospecting)', color: 'blue', probability: 20 };
    case 'Qualification':
      return { label: '🎯 Đánh giá khả thi (Qualification)', color: 'cyan', probability: 40 };
    case 'NeedsAnalysis':
      return { label: '📋 Phân tích giải pháp', color: 'indigo', probability: 50 };
    case 'ValueProposition':
      return { label: '💡 Trình bày giải pháp giá trị', color: 'purple', probability: 60 };
    case 'Proposal':
      return { label: '📄 Đã gửi Báo giá / Đề xuất', color: 'orange', probability: 75 };
    case 'Negotiation':
      return { label: '🤝 Thương lượng / Đàm phán', color: 'amber', probability: 90 };
    case 'ClosedWon':
      return { label: '🏆 Đóng thắng (Closed-Won)', color: 'green', probability: 100 };
    case 'ClosedLost':
      return { label: '❌ Đóng thua (Closed-Lost)', color: 'red', probability: 0 };
    default:
      return { label: '💼 Đang tư vấn', color: 'gray', probability: 30 };
  }
}
