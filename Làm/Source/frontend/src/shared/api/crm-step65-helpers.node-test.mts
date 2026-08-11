import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatVoucherStatusBadge,
  validateVoucherRedeemRequest,
  calculateDiscountPreview,
  formatVoucherReportSummary,
} from './crm-step65-helpers.ts';

// ─── UC_CRM_035: formatVoucherStatusBadge ───

test('formatVoucherStatusBadge - available and used vouchers return correct badge state', () => {
  const active = formatVoucherStatusBadge('Active', 0, 1);
  assert.equal(active.isAvailable, true);
  assert.ok(active.label.includes('Khả dụng'));

  const used = formatVoucherStatusBadge('Active', 1, 1);
  assert.equal(used.isAvailable, false);
  assert.ok(used.label.includes('hết lượt'));
});

// ─── UC_CRM_035: validateVoucherRedeemRequest ───

test('validateVoucherRedeemRequest - empty code returns error', () => {
  const res = validateVoucherRedeemRequest('');
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('không được để trống'));
});

test('validateVoucherRedeemRequest - valid code returns isValid true', () => {
  const res = validateVoucherRedeemRequest('VCH2026-001');
  assert.equal(res.isValid, true);
});

// ─── UC_CRM_037: calculateDiscountPreview ───

test('calculateDiscountPreview - percentage discount respects max discount cap', () => {
  // 20% of 1.000.000 = 200.000, max discount = 100.000
  const discount = calculateDiscountPreview('Percentage', 20, 1000000, 100000);
  assert.equal(discount, 100000);
});

test('calculateDiscountPreview - fixed amount discount capped at subTotal', () => {
  const discount = calculateDiscountPreview('FixedAmount', 500000, 300000);
  assert.equal(discount, 300000);
});

// ─── UC_CRM_038: formatVoucherReportSummary ───

test('formatVoucherReportSummary - formats usage percentage summary', () => {
  const summary = formatVoucherReportSummary(100, 25);
  assert.ok(summary.includes('25 / 100 voucher'));
  assert.ok(summary.includes('25% tỷ lệ sử dụng'));
});
