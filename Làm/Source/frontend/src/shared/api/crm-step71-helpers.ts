// crm-step71-helpers.ts
// Frontend helpers cho Bước 71:
//   UC_CRM_068 — Đóng thắng / thua (formatWinLossStageNotice)
//   UC_CRM_069 — Báo cáo win-rate (formatWinRateReportSummary)
//   UC_CRM_070 — Tạo báo giá từ cơ hội (validateQuoteHeaderInput)
//   UC_CRM_071 — Thêm dòng sản phẩm / dịch vụ (calculateQuoteLineSummary)

export function formatWinLossStageNotice(stage: string, lostReason?: string): { title: string; color: string; isClosed: boolean } {
  const st = (stage || '').trim();
  if (st === 'Won') {
    return { title: '🎉 ĐỐNG THẮNG: Đã chốt hợp đồng thành công (Closed-Won)!', color: 'green', isClosed: true };
  }
  if (st === 'Lost') {
    const reasonNotice = lostReason ? ` (Lý do: ${lostReason})` : '';
    return { title: `❌ ĐỐNG THUA: Cơ hội thất bại (Closed-Lost)${reasonNotice}`, color: 'red', isClosed: true };
  }
  return { title: '💼 Cơ hội đang trong tiến trình đàm phán.', color: 'blue', isClosed: false };
}

export function formatWinRateReportSummary(total: number, won: number, lost: number, winRatePercent: number): string {
  const t = total || 0;
  const w = won || 0;
  const l = lost || 0;
  const rate = winRatePercent || (t > 0 ? Math.round((w / t) * 100) : 0);

  return `📊 Tỷ lệ thắng (Win-Rate): ${rate}% | Đã thắng: ${w}/${t} | Thất bại: ${l}`;
}

export function validateQuoteHeaderInput(input: { quoteDate?: string | Date; validUntil?: string | Date }): { isValid: boolean; error?: string } {
  if (!input.quoteDate) {
    return { isValid: false, error: 'Ngày lập báo giá là bắt buộc.' };
  }

  if (input.validUntil) {
    const qDate = new Date(input.quoteDate).getTime();
    const vDate = new Date(input.validUntil).getTime();
    if (vDate < qDate) {
      return { isValid: false, error: 'Ngày hết hạn hiệu lực không được nhỏ hơn ngày lập báo giá.' };
    }
  }

  return { isValid: true };
}

export function calculateQuoteLineSummary(quantity: number, unitPrice: number, discountPercent: number = 0): { grossAmount: number; discountAmount: number; netAmount: number; isValid: boolean; error?: string } {
  if (quantity <= 0) {
    return { grossAmount: 0, discountAmount: 0, netAmount: 0, isValid: false, error: 'Số lượng phải lớn hơn 0.' };
  }
  if (unitPrice < 0) {
    return { grossAmount: 0, discountAmount: 0, netAmount: 0, isValid: false, error: 'Đơn giá không được nhỏ hơn 0.' };
  }
  if (discountPercent < 0 || discountPercent > 100) {
    return { grossAmount: 0, discountAmount: 0, netAmount: 0, isValid: false, error: 'Phần trăm chiết khấu phải từ 0% đến 100%.' };
  }

  const grossAmount = Math.round(quantity * unitPrice * 100) / 100;
  const discountAmount = Math.round((grossAmount * discountPercent / 100) * 100) / 100;
  const netAmount = grossAmount - discountAmount;

  return { grossAmount, discountAmount, netAmount, isValid: true };
}
