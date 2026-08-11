import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateHoldSaleRequest,
  formatSaleHoldBadge,
  validateCashPayment,
  formatCashChange,
  validateTransferPayment,
  formatTransferNotice,
  validateCardWalletPayment,
  formatPaymentMethodBadge,
} from './pos-step80-helpers.ts';

// ─── UC_POS_032: Tạm tính / giữ đơn ───

test('UC_POS_032 — validateHoldSaleRequest — valid request passes', () => {
  const result = validateHoldSaleRequest(3, 'Open');
  assert.equal(result.canHold, true);
});

test('UC_POS_032 — validateHoldSaleRequest — empty order rejected', () => {
  const result = validateHoldSaleRequest(0, 'Open');
  assert.equal(result.canHold, false);
  assert.ok(result.reason?.includes('Đơn rỗng'));
});

test('UC_POS_032 — formatSaleHoldBadge — Held status format', () => {
  const badge = formatSaleHoldBadge('Held', 'Khách chờ người thân');
  assert.equal(badge.style, 'warning');
  assert.ok(badge.label.includes('Đang giữ đơn'));
});

// ─── UC_POS_033: Thanh toán tiền mặt ───

test('UC_POS_033 — validateCashPayment — calculates correct change', () => {
  const result = validateCashPayment(500000, 350000);
  assert.equal(result.isValid, true);
  assert.equal(result.change, 150000);
});

test('UC_POS_033 — validateCashPayment — insufficient cash rejected', () => {
  const result = validateCashPayment(200000, 350000);
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('Thiếu 150.000'));
});

test('UC_POS_033 — formatCashChange — formats cash & change text', () => {
  const text = formatCashChange(500000, 150000);
  assert.ok(text.includes('500.000 VNĐ'));
  assert.ok(text.includes('150.000 VNĐ'));
});

// ─── UC_POS_034: Thanh toán chuyển khoản / QR ───

test('UC_POS_034 — validateTransferPayment — valid amount passes', () => {
  const result = validateTransferPayment(200000, 200000);
  assert.equal(result.isValid, true);
});

test('UC_POS_034 — formatTransferNotice — formats VietQR text', () => {
  const text = formatTransferNotice('VietQR', 'FT260811001');
  assert.ok(text.includes('VietQR'));
  assert.ok(text.includes('FT260811001'));
});

// ─── UC_POS_035: Thanh toán thẻ / ví điện tử ───

test('UC_POS_035 — validateCardWalletPayment — valid card payment passes', () => {
  const result = validateCardWalletPayment('Card', 100000, 100000);
  assert.equal(result.isValid, true);
});

test('UC_POS_035 — formatPaymentMethodBadge — Card returns card icon', () => {
  const badge = formatPaymentMethodBadge('Card');
  assert.equal(badge.icon, '💳');
  assert.equal(badge.label, 'Thẻ ngân hàng');
});
