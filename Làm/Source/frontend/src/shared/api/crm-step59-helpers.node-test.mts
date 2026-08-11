import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatCustomerStatusBadge,
  validateContactInput,
  formatPrimaryContactBadge,
  formatAuditTrailSummary,
} from './crm-step59-helpers.ts';

// ─── UC_CRM_010 & UC_CRM_013: formatCustomerStatusBadge ───

test('formatCustomerStatusBadge - active and blacklisted statuses return correct metadata', () => {
  const active = formatCustomerStatusBadge('Active');
  assert.equal(active.isBlacklisted, false);
  assert.ok(active.label.includes('Hoạt động'));

  const blacklisted = formatCustomerStatusBadge('Blacklisted');
  assert.equal(blacklisted.isBlacklisted, true);
  assert.ok(blacklisted.label.includes('Blacklist'));
});

// ─── UC_CRM_011: validateContactInput ───

test('validateContactInput - valid contact returns no errors', () => {
  const res = validateContactInput({ fullName: 'Trần Văn B', phone: '0901112233', email: 'b@erp.vn' });
  assert.equal(res.isValid, true);
  assert.equal(res.errors.length, 0);
});

test('validateContactInput - empty name returns validation error', () => {
  const res = validateContactInput({ fullName: '   ' });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('Họ tên người liên hệ')));
});

test('validateContactInput - invalid email returns validation error', () => {
  const res = validateContactInput({ fullName: 'Lê Văn C', email: 'invalid-email' });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('email')));
});

// ─── UC_CRM_011: formatPrimaryContactBadge ───

test('formatPrimaryContactBadge - formats primary and secondary contact labels', () => {
  assert.ok(formatPrimaryContactBadge(true).includes('Liên hệ chính'));
  assert.ok(formatPrimaryContactBadge(false).includes('Người liên hệ'));
});

// ─── UC_CRM_012: formatAuditTrailSummary ───

test('formatAuditTrailSummary - formats 360 summary string accurately', () => {
  const summary = formatAuditTrailSummary(5, 3, 'Active');
  assert.ok(summary.includes('Active'));
  assert.ok(summary.includes('Người liên hệ: 3'));
  assert.ok(summary.includes('5 sự kiện'));
});
