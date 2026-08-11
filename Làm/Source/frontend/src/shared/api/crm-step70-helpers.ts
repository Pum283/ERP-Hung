// crm-step70-helpers.ts
// Frontend helpers cho Bước 70:
//   UC_CRM_064 — Dự báo doanh thu (formatRevenueForecastSummary)
//   UC_CRM_065 — Gắn sản phẩm / giá trị ước tính (calculateOpportunityLineTotal)
//   UC_CRM_066 — Đối thủ / ghi chú đàm phán (validateCompetitorInfo)
//   UC_CRM_067 — Chuyển cơ hội sang báo giá (formatQuoteFromOppResult)

export function formatRevenueForecastSummary(totalEstimatedValue: number, weightedForecastValue: number): string {
  const est = (totalEstimatedValue || 0).toLocaleString('vi-VN');
  const w = (weightedForecastValue || 0).toLocaleString('vi-VN');
  return `📈 Doanh thu dự kiến: ${est} VNĐ | Giá trị gia trọng: ${w} VNĐ`;
}

export function calculateOpportunityLineTotal(quantity: number, unitPrice: number): { lineAmount: number; isValid: boolean; error?: string } {
  if (quantity <= 0) {
    return { lineAmount: 0, isValid: false, error: 'Số lượng phải lớn hơn 0.' };
  }
  if (unitPrice < 0) {
    return { lineAmount: 0, isValid: false, error: 'Đơn giá không được âm.' };
  }

  const lineAmount = Math.round(quantity * unitPrice * 100) / 100;
  return { lineAmount, isValid: true };
}

export function validateCompetitorInfo(input: { competitorName?: string; negotiationNotes?: string }): { isValid: boolean; error?: string } {
  const comp = (input.competitorName || '').trim();
  const notes = (input.negotiationNotes || '').trim();

  if (comp.length > 200) {
    return { isValid: false, error: 'Tên đối thủ tối đa 200 ký tự.' };
  }

  if (notes.length > 2000) {
    return { isValid: false, error: 'Ghi chú đàm phán tối đa 2000 ký tự.' };
  }

  return { isValid: true };
}

export function formatQuoteFromOppResult(quoteCode: string, quoteId: string): string {
  return `📜 Đã tạo thành công Báo giá mới "${quoteCode}" từ Cơ hội bán hàng.`;
}
