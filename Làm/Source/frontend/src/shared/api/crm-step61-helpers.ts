// crm-step61-helpers.ts
// Frontend helpers cho Bước 61:
//   UC_CRM_018 — Gắn sản phẩm / đối tượng mục tiêu (formatCampaignRoiSummary)
//   UC_CRM_019 — Ghi nhận chi phí quảng cáo (formatExpenseTypeBadge, validateExpenseInput)
//   UC_CRM_020 — Gắn ngân sách & theo dõi (calculateBudgetBurnRate)
//   UC_CRM_021 — Đánh giá hậu chiến dịch (formatCampaignRoiSummary)

export function formatExpenseTypeBadge(expenseType?: string): { label: string; icon: string } {
  switch ((expenseType || '').trim().toLowerCase()) {
    case 'ads':
      return { label: 'Quảng cáo Trực tuyến (Ads)', icon: '🎯' };
    case 'media':
      return { label: 'Truyền thông & Báo chí', icon: '📰' };
    case 'event':
      return { label: 'Sự kiện / Workshop', icon: '🎤' };
    case 'agency':
      return { label: 'Chi phí Agency', icon: '🏢' };
    default:
      return { label: 'Chi phí khác', icon: '💸' };
  }
}

export function validateExpenseInput(amount: number, expenseType: string, description?: string): { isValid: boolean; errors: string[] } {
  const errors: string[] = [];

  if (isNaN(amount) || amount <= 0) {
    errors.push('Số tiền chi phí phải là số dương lớn hơn 0.');
  }

  const validTypes = ['Ads', 'Media', 'Event', 'Agency', 'Other'];
  if (!validTypes.includes((expenseType || '').trim())) {
    errors.push('Loại chi phí phải thuộc danh mục: Ads, Media, Event, Agency, Other.');
  }

  if (description && description.length > 500) {
    errors.push('Mô tả chi phí tối đa 500 ký tự.');
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
}

export function calculateBudgetBurnRate(budgetAmount: number, spentAmount: number): { burnRatePct: number; isOverBudget: boolean; statusLabel: string } {
  if (isNaN(budgetAmount) || budgetAmount <= 0) {
    return { burnRatePct: 0, isOverBudget: false, statusLabel: 'Chưa thiết lập ngân sách' };
  }

  const burnRatePct = Math.round(((spentAmount || 0) / budgetAmount) * 10000) / 100;
  const isOverBudget = burnRatePct > 100;
  const statusLabel = isOverBudget
    ? `🚨 Vượt ngân sách! (${burnRatePct}%)`
    : `📊 Giải ngân: ${burnRatePct}% (${(spentAmount || 0).toLocaleString('vi-VN')} / ${budgetAmount.toLocaleString('vi-VN')} VNĐ)`;

  return { burnRatePct, isOverBudget, statusLabel };
}

export function formatCampaignRoiSummary(spent: number, revenue: number, roi: number): string {
  const roiText = roi >= 0 ? `+${roi}%` : `${roi}%`;
  return `📈 Đánh giá ROI: Chi ${spent.toLocaleString('vi-VN')} VNĐ ➔ Thu ${revenue.toLocaleString('vi-VN')} VNĐ | ROI: ${roiText}`;
}
