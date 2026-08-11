// crm-step64-helpers.ts
// Frontend helpers cho Bước 64:
//   UC_CRM_031 — Dashboard marketing
//   UC_CRM_032 — Tạo chương trình khuyến mại (formatDiscountTypeBadge, validatePromotionInput)
//   UC_CRM_033 — Cấu hình điều kiện khuyến mại (formatConditionTypeLabel)
//   UC_CRM_034 — Sinh mã voucher (generateVoucherCodePreview)

export function formatDiscountTypeBadge(discountType?: string, discountValue?: number): { label: string; icon: string } {
  const type = (discountType || '').trim().toLowerCase();
  const valStr = discountValue ? discountValue.toLocaleString('vi-VN') : '0';

  switch (type) {
    case 'percent':
      return { label: `🏷️ Giảm ${valStr}%`, icon: '🏷️' };
    case 'amount':
      return { label: `💵 Giảm ${valStr} VNĐ`, icon: '💵' };
    case 'sameprice':
      return { label: `🎯 Đồng giá ${valStr} VNĐ`, icon: '🎯' };
    case 'gift':
      return { label: '🎁 Quà tặng kèm', icon: '🎁' };
    default:
      return { label: '🔖 Khuyến mại', icon: '🔖' };
  }
}

export function validatePromotionInput(input: {
  code: string;
  name: string;
  discountType: string;
  discountValue: number;
  startDate?: string;
  endDate?: string;
}): { isValid: boolean; errors: string[] } {
  const errors: string[] = [];

  const code = (input.code || '').trim();
  if (!code || code.length > 40) {
    errors.push('Mã khuyến mại là bắt buộc và tối đa 40 ký tự.');
  }

  const name = (input.name || '').trim();
  if (!name || name.length > 200) {
    errors.push('Tên khuyến mại là bắt buộc và tối đa 200 ký tự.');
  }

  const validTypes = ['Percent', 'Amount', 'SamePrice', 'Gift'];
  if (!validTypes.includes((input.discountType || '').trim())) {
    errors.push('Loại giảm giá phải thuộc: Percent, Amount, SamePrice, Gift.');
  }

  if (input.discountType !== 'Gift' && (isNaN(input.discountValue) || input.discountValue <= 0)) {
    errors.push('Giá trị giảm giá phải là số dương > 0.');
  }

  if (input.startDate && input.endDate && new Date(input.endDate) < new Date(input.startDate)) {
    errors.push('Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.');
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
}

export function formatConditionTypeLabel(conditionType?: string, value?: string): string {
  const type = (conditionType || '').trim();
  const val = (value || '').trim();

  switch (type) {
    case 'MinOrderValue':
      return `💰 Đơn hàng tối thiểu: ${Number(val || 0).toLocaleString('vi-VN')} VNĐ`;
    case 'MinQty':
      return `📦 Số lượng mua tối thiểu: ${val} sản phẩm`;
    case 'ProductCategory':
      return `🏷️ Áp dụng cho Danh mục: ${val}`;
    case 'Product':
      return `🛒 Áp dụng cho Sản phẩm: ${val}`;
    case 'CustomerSegment':
      return `👑 Phân tệp khách hàng: ${val}`;
    default:
      return `📌 Điều kiện: ${type} = ${val}`;
  }
}

export function generateVoucherCodePreview(prefix: string, startIndex: number, count: number): string[] {
  const list: string[] = [];
  const p = (prefix || 'VCH-').trim();
  const start = isNaN(startIndex) || startIndex < 1 ? 1 : startIndex;
  const num = isNaN(count) || count < 1 ? 1 : Math.min(count, 5);

  for (let i = 0; i < num; i++) {
    const seq = (start + i).toString().padStart(6, '0');
    list.push(`${p}${seq}`);
  }
  return list;
}
