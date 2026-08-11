import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatDiscountTypeBadge,
  validatePromotionInput,
  formatConditionTypeLabel,
  generateVoucherCodePreview,
} from './crm-step64-helpers.ts';

// ─── UC_CRM_032: formatDiscountTypeBadge ───

test('formatDiscountTypeBadge - percent and amount return formatted labels', () => {
  const percent = formatDiscountTypeBadge('Percent', 20);
  assert.ok(percent.label.includes('20%'));
  assert.ok(percent.icon.includes('🏷️'));

  const amount = formatDiscountTypeBadge('Amount', 100000);
  assert.ok(amount.label.includes('100.000 VNĐ'));
});

// ─── UC_CRM_032: validatePromotionInput ───

test('validatePromotionInput - valid promotion input returns no errors', () => {
  const res = validatePromotionInput({
    code: 'KM_TET',
    name: 'Khuyến mại tết',
    discountType: 'Percent',
    discountValue: 15,
    startDate: '2026-01-01',
    endDate: '2026-02-01',
  });
  assert.equal(res.isValid, true);
  assert.equal(res.errors.length, 0);
});

test('validatePromotionInput - end date before start date returns date error', () => {
  const res = validatePromotionInput({
    code: 'KM_BAD',
    name: 'Khuyến mại sai ngày',
    discountType: 'Amount',
    discountValue: 50000,
    startDate: '2026-02-01',
    endDate: '2026-01-01',
  });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('Ngày kết thúc')));
});

// ─── UC_CRM_033: formatConditionTypeLabel ───

test('formatConditionTypeLabel - formats MinOrderValue and CustomerSegment labels correctly', () => {
  const minVal = formatConditionTypeLabel('MinOrderValue', '500000');
  assert.ok(minVal.includes('500.000 VNĐ'));

  const segment = formatConditionTypeLabel('CustomerSegment', 'VIP');
  assert.ok(segment.includes('VIP'));
});

// ─── UC_CRM_034: generateVoucherCodePreview ───

test('generateVoucherCodePreview - generates sequential padded voucher codes', () => {
  const preview = generateVoucherCodePreview('VCH2026-', 1, 3);
  assert.equal(preview.length, 3);
  assert.equal(preview[0], 'VCH2026-000001');
  assert.equal(preview[1], 'VCH2026-000002');
  assert.equal(preview[2], 'VCH2026-000003');
});
