import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateReturnOrderRequest,
  formatOrderReturnNotice,
  validateLinkContractRequest,
  formatContractLinkNotice,
  validateOrderPayment,
  formatOrderPaymentSummary,
  validatePushToWarehouseEligibility,
  formatWarehousePushNotice,
} from './crm-step75-helpers.ts';

// ─── UC_CRM_085: Trả hàng / điều chỉnh đơn ───

test('UC_CRM_085 — validateReturnOrderRequest — valid return request', () => {
  const result = validateReturnOrderRequest('Sản phẩm móp méo khi vận chuyển', 'Confirmed');
  assert.equal(result.isValid, true);
  assert.equal(result.error, undefined);
});

test('UC_CRM_085 — validateReturnOrderRequest — empty reason rejected', () => {
  const result = validateReturnOrderRequest('   ', 'Confirmed');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('lý do'));
});

test('UC_CRM_085 — validateReturnOrderRequest — Cancelled order cannot return', () => {
  const result = validateReturnOrderRequest('Lý do hợp lệ', 'Cancelled');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('hủy'));
});

test('UC_CRM_085 — formatOrderReturnNotice — formats notice string correctly', () => {
  const notice = formatOrderReturnNotice('SO-100', 'Lỗi linh kiện');
  assert.ok(notice.includes('SO-100'));
  assert.ok(notice.includes('Lỗi linh kiện'));
  assert.ok(notice.includes('↩️'));
});

// ─── UC_CRM_086: Gắn hợp đồng ───

test('UC_CRM_086 — validateLinkContractRequest — valid contract ID passes', () => {
  const result = validateLinkContractRequest('contract-uuid-1234');
  assert.equal(result.isValid, true);
});

test('UC_CRM_086 — validateLinkContractRequest — missing contract ID rejected', () => {
  const result = validateLinkContractRequest('');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('hợp đồng'));
});

test('UC_CRM_086 — formatContractLinkNotice — formats with contract code', () => {
  const notice = formatContractLinkNotice('SO-200', 'HD-2026-088');
  assert.ok(notice.includes('SO-200'));
  assert.ok(notice.includes('HD-2026-088'));
  assert.ok(notice.includes('📜'));
});

// ─── UC_CRM_087: Theo dõi thanh toán ───

test('UC_CRM_087 — validateOrderPayment — valid payment within remaining amount', () => {
  const result = validateOrderPayment(5000000, 10000000, 'Transfer');
  assert.equal(result.isValid, true);
});

test('UC_CRM_087 — validateOrderPayment — payment exceeds remaining rejected', () => {
  const result = validateOrderPayment(15000000, 10000000, 'Cash');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('vượt quá'));
});

test('UC_CRM_087 — validateOrderPayment — invalid payment method rejected', () => {
  const result = validateOrderPayment(1000000, 10000000, 'CryptoCurrency');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('Phương thức'));
});

test('UC_CRM_087 — formatOrderPaymentSummary — calculates partial payment status & percent', () => {
  const summary = formatOrderPaymentSummary(5000000, 10000000);
  assert.equal(summary.percent, 50);
  assert.equal(summary.status, 'Partial');
  assert.ok(summary.paidText.includes('5.000.000'));
});

test('UC_CRM_087 — formatOrderPaymentSummary — calculates full payment status', () => {
  const summary = formatOrderPaymentSummary(10000000, 10000000);
  assert.equal(summary.percent, 100);
  assert.equal(summary.status, 'Paid');
});

// ─── UC_CRM_088: Đẩy đơn sang kho / giao vận ───

test('UC_CRM_088 — validatePushToWarehouseEligibility — Confirmed order with lines passes', () => {
  const result = validatePushToWarehouseEligibility('Confirmed', 2);
  assert.equal(result.canPush, true);
});

test('UC_CRM_088 — validatePushToWarehouseEligibility — Draft order rejected', () => {
  const result = validatePushToWarehouseEligibility('Draft', 2);
  assert.equal(result.canPush, false);
  assert.ok(result.reason?.includes('xác nhận'));
});

test('UC_CRM_088 — validatePushToWarehouseEligibility — zero lines rejected', () => {
  const result = validatePushToWarehouseEligibility('Confirmed', 0);
  assert.equal(result.canPush, false);
  assert.ok(result.reason?.includes('chưa có dòng sản phẩm'));
});

test('UC_CRM_088 — formatWarehousePushNotice — Pushed status returns success label', () => {
  const notice = formatWarehousePushNotice('SO-300', 'Pushed');
  assert.equal(notice.style, 'success');
  assert.ok(notice.label.includes('đã đẩy kho'));
});
