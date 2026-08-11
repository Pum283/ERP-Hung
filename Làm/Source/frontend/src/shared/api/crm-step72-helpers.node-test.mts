import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatPriceListBindingNotice,
  validateDiscountApprovalRequest,
  formatQuoteDispatchNotice,
  formatQuoteRevisionBadge,
} from './crm-step72-helpers.ts';

// ─── UC_CRM_072: formatPriceListBindingNotice ───

test('formatPriceListBindingNotice - custom and default price list notice', () => {
  const custom = formatPriceListBindingNotice('Bảng giá VIP 2026');
  assert.ok(custom.includes('Bảng giá VIP 2026'));

  const def = formatPriceListBindingNotice();
  assert.ok(def.includes('bảng giá mặc định'));
});

// ─── UC_CRM_073: validateDiscountApprovalRequest ───

test('validateDiscountApprovalRequest - discount <= 15 does not require approval', () => {
  const res = validateDiscountApprovalRequest(10);
  assert.equal(res.isValid, true);
  assert.equal(res.requiresApproval, false);
});

test('validateDiscountApprovalRequest - discount > 15 without reason returns validation error', () => {
  const res = validateDiscountApprovalRequest(20, '');
  assert.equal(res.isValid, false);
  assert.equal(res.requiresApproval, true);
  assert.ok(res.error?.includes('bắt buộc nhập lý do'));
});

test('validateDiscountApprovalRequest - discount > 15 with reason returns valid', () => {
  const res = validateDiscountApprovalRequest(20, 'Dự án trọng điểm');
  assert.equal(res.isValid, true);
  assert.equal(res.requiresApproval, true);
});

// ─── UC_CRM_074: formatQuoteDispatchNotice ───

test('formatQuoteDispatchNotice - formats email and zalo dispatch notice', () => {
  const email = formatQuoteDispatchNotice('Email', 'client@growth.vn');
  assert.equal(email.icon, 'email');
  assert.ok(email.title.includes('client@growth.vn'));

  const zalo = formatQuoteDispatchNotice('Zalo');
  assert.equal(zalo.icon, 'zalo');
  assert.ok(zalo.title.includes('Zalo OA'));
});

// ─── UC_CRM_075: formatQuoteRevisionBadge ───

test('formatQuoteRevisionBadge - formats version number badge string', () => {
  const badge = formatQuoteRevisionBadge(3);
  assert.equal(badge, 'v3.0 (Phiên bản 3)');
});
