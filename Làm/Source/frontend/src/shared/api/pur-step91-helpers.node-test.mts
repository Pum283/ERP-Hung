import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateVendorInvoiceCreate,
  formatThreeWayMatchStatus,
  validateApPushRequest,
  validatePurchaseReportFilter,
} from './pur-step91-helpers.ts';

test('UC_PUR_040: validateVendorInvoiceCreate', () => {
  const valid = validateVendorInvoiceCreate('VEND-01', 'INV-2026-001', 1000000);
  assert.equal(valid.isValid, true);

  const noNumber = validateVendorInvoiceCreate('VEND-01', '', 1000000);
  assert.equal(noNumber.isValid, false);
  assert.match(noNumber.error!, /Số hóa đơn/);
});

test('UC_PUR_041: formatThreeWayMatchStatus', () => {
  const matched = formatThreeWayMatchStatus('Matched');
  assert.equal(matched.badgeStyle, 'success');
  assert.match(matched.label, /Đã đối soát khớp/);

  const mismatch = formatThreeWayMatchStatus('Mismatch');
  assert.equal(mismatch.badgeStyle, 'danger');
});

test('UC_PUR_043: validateApPushRequest', () => {
  const valid = validateApPushRequest('Matched', 'Pending');
  assert.equal(valid.canPush, true);

  const unmatched = validateApPushRequest('Pending', 'Pending');
  assert.equal(unmatched.canPush, false);
  assert.match(unmatched.reason!, /Matched/);
});

test('UC_PUR_048: validatePurchaseReportFilter', () => {
  const valid = validatePurchaseReportFilter('2026-01-01', '2026-01-31');
  assert.equal(valid.isValid, true);

  const invalid = validatePurchaseReportFilter('2026-02-01', '2026-01-01');
  assert.equal(invalid.isValid, false);
});
