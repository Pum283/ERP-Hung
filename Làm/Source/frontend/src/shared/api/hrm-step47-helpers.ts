// hrm-step47-helpers.ts
// Frontend helpers cho Bước 47:
//   UC_HRM_185 — Báo cáo quỹ phép (calculateLeaveFundUsageRate)
//   UC_HRM_186 — Báo cáo chi phí nhân sự (formatHRCostSummaryRow)
//   UC_HRM_187 — Báo cáo định biên vs thực tế (calculateHeadcountVariance)
//   UC_LMS_001 — Danh mục chương trình đào tạo (validateLmsProgramInput & formatLmsProgramStatus)

export function calculateLeaveFundUsageRate(entitled: number, used: number): { usageRatePct: number; isOverLimit: boolean } {
  if (isNaN(entitled) || entitled <= 0) return { usageRatePct: 0, isOverLimit: false };
  const usageRatePct = Math.round(((used || 0) / entitled) * 10000) / 100;
  return { usageRatePct, isOverLimit: usageRatePct > 100 };
}

export interface HeadcountVarianceResult {
  variance: number;
  fulfillmentPct: number;
  statusLabel: string;
}

export function calculateHeadcountVariance(target: number, current: number): HeadcountVarianceResult {
  const variance = (current || 0) - (target || 0);
  const fulfillmentPct = target > 0 ? Math.round(((current || 0) / target) * 10000) / 100 : 0;

  let statusLabel = '🎯 Đạt định biên';
  if (variance < 0) statusLabel = `⚠️ Thiếu ${Math.abs(variance)} nhân sự`;
  else if (variance > 0) statusLabel = `📈 Vượt định biên +${variance} nhân sự`;

  return { variance, fulfillmentPct, statusLabel };
}

export interface LmsProgramInput {
  code: string;
  name: string;
  status: string;
}

export function validateLmsProgramInput(p: LmsProgramInput): { valid: boolean; error?: string } {
  if (!p.code?.trim() || p.code.trim().length > 40)
    return { valid: false, error: 'Mã chương trình đào tạo phải từ 1 đến 40 ký tự.' };

  if (!p.name?.trim() || p.name.trim().length > 200)
    return { valid: false, error: 'Tên chương trình đào tạo phải từ 1 đến 200 ký tự.' };

  const validStatuses = ['Active', 'Inactive'];
  if (p.status && !validStatuses.includes(p.status))
    return { valid: false, error: 'Trạng thái chương trình đào tạo phải là Active hoặc Inactive.' };

  return { valid: true };
}

export function formatLmsProgramStatus(status: string): string {
  switch (status) {
    case 'Active':
      return '🟢 Đang hoạt động';
    case 'Inactive':
      return '🔴 Ngưng áp dụng';
    default:
      return status || 'Khác';
  }
}
