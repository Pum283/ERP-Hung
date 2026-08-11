import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatOrderStatusBadge,
  getAvailableStatusTransitions,
  formatStockHoldNotice,
  validateStockHoldEligibility,
  validateSplitRequest,
  formatSplitOrderNotice,
  formatMergeOrderNotice,
  validateCancelRequest,
  formatCancelConfirmation,
} from './crm-step74-helpers.ts';

// ─── UC_CRM_081: Cập nhật trạng thái đơn ───

test('UC_CRM_081 — formatOrderStatusBadge — returns correct badge for Confirmed', () => {
  const badge = formatOrderStatusBadge('Confirmed');
  assert.equal(badge.label, 'Đã xác nhận');
  assert.equal(badge.icon, '✅');
  assert.ok(badge.color.startsWith('#'));
});

test('UC_CRM_081 — formatOrderStatusBadge — unknown status returns fallback', () => {
  const badge = formatOrderStatusBadge('UnknownXYZ');
  assert.equal(badge.label, 'UnknownXYZ');
  assert.equal(badge.icon, '❓');
});

test('UC_CRM_081 — getAvailableStatusTransitions — Confirmed can transition to Holding/Released/Delivered', () => {
  const transitions = getAvailableStatusTransitions('Confirmed');
  assert.ok(transitions.includes('Holding'));
  assert.ok(transitions.includes('Released'));
  assert.ok(transitions.includes('Delivered'));
  assert.ok(!transitions.includes('Draft'));
});

test('UC_CRM_081 — getAvailableStatusTransitions — Cancelled returns empty', () => {
  const transitions = getAvailableStatusTransitions('Cancelled');
  assert.equal(transitions.length, 0);
});

// ─── UC_CRM_082: Giữ tồn khi duyệt đơn ───

test('UC_CRM_082 — validateStockHoldEligibility — eligible for valid order', () => {
  const result = validateStockHoldEligibility('Confirmed', 'None', 3);
  assert.equal(result.eligible, true);
});

test('UC_CRM_082 — validateStockHoldEligibility — not eligible for Cancelled order', () => {
  const result = validateStockHoldEligibility('Cancelled', 'None', 3);
  assert.equal(result.eligible, false);
  assert.ok(result.reason?.includes('Cancelled'));
});

test('UC_CRM_082 — formatStockHoldNotice — Held status shows success message', () => {
  const notice = formatStockHoldNotice('Held', 'SO-001');
  assert.ok(notice.message.includes('giữ tồn'));
  assert.equal(notice.canHold, false);
});

// ─── UC_CRM_083: Tách / gộp đơn ───

test('UC_CRM_083 — validateSplitRequest — valid split with lines', () => {
  const result = validateSplitRequest(['line1', 'line2'], 5, 'Confirmed');
  assert.equal(result.isValid, true);
});

test('UC_CRM_083 — validateSplitRequest — splitting all lines rejected', () => {
  const result = validateSplitRequest(['l1', 'l2', 'l3'], 3, 'Confirmed');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('tất cả'));
});

test('UC_CRM_083 — formatSplitOrderNotice — formats correctly', () => {
  const notice = formatSplitOrderNotice('SO-001', 'SO-001-S1', 2);
  assert.ok(notice.includes('SO-001'));
  assert.ok(notice.includes('SO-001-S1'));
  assert.ok(notice.includes('2 dòng'));
});

test('UC_CRM_083 — formatMergeOrderNotice — formats merge message', () => {
  const notice = formatMergeOrderNotice('SO-001', 'SO-002');
  assert.ok(notice.includes('SO-001'));
  assert.ok(notice.includes('SO-002'));
  assert.ok(notice.includes('đã bị hủy'));
});

// ─── UC_CRM_084: Hủy đơn có kiểm soát ───

test('UC_CRM_084 — validateCancelRequest — valid cancel with reason', () => {
  const result = validateCancelRequest('Khách yêu cầu hủy', 'Confirmed');
  assert.equal(result.isValid, true);
});

test('UC_CRM_084 — validateCancelRequest — empty reason rejected', () => {
  const result = validateCancelRequest('', 'Confirmed');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('lý do'));
});

test('UC_CRM_084 — validateCancelRequest — Delivered order cannot cancel', () => {
  const result = validateCancelRequest('Lý do test', 'Delivered');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('đã giao'));
});

test('UC_CRM_084 — formatCancelConfirmation — formats confirmation', () => {
  const text = formatCancelConfirmation('SO-005', 'Hết hàng');
  assert.ok(text.includes('SO-005'));
  assert.ok(text.includes('Hết hàng'));
  assert.ok(text.includes('❌'));
});
