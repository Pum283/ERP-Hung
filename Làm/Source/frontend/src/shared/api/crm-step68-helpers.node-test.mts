import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatActivityTypeBadge,
  formatConversionResult,
  validateMergeLeadRequest,
  formatConversionReportSummary,
} from './crm-step68-helpers.ts';

// ─── UC_CRM_056: formatActivityTypeBadge ───

test('formatActivityTypeBadge - returns correct labels and icons for activity types', () => {
  const call = formatActivityTypeBadge('Call');
  assert.equal(call.icon, '📞');
  assert.ok(call.label.includes('Cuộc gọi điện'));

  const meeting = formatActivityTypeBadge('Meeting');
  assert.equal(meeting.icon, '🤝');
});

// ─── UC_CRM_057: formatConversionResult ───

test('formatConversionResult - formats conversion success message', () => {
  const msg = formatConversionResult('ERP Enterprise Implementation', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890');
  assert.ok(msg.includes('Chuyển đổi thành công'));
  assert.ok(msg.includes('ERP Enterprise Implementation'));
  assert.ok(msg.includes('a1b2c3d4'));
});

// ─── UC_CRM_058: validateMergeLeadRequest ───

test('validateMergeLeadRequest - different IDs return isValid true', () => {
  const res = validateMergeLeadRequest('lead-id-1', 'lead-id-2');
  assert.equal(res.isValid, true);
});

test('validateMergeLeadRequest - same ID returns validation error', () => {
  const res = validateMergeLeadRequest('lead-id-1', 'lead-id-1');
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('không được là cùng một đối tượng'));
});

test('validateMergeLeadRequest - missing ID returns validation error', () => {
  const res = validateMergeLeadRequest('lead-id-1', '');
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Phải chọn đầy đủ'));
});

// ─── UC_CRM_059: formatConversionReportSummary ───

test('formatConversionReportSummary - formats summary string accurately', () => {
  const summary = formatConversionReportSummary({ totalLeads: 100, convertedLeads: 25, conversionRatePct: 25 });
  assert.ok(summary.includes('Tổng số lead: 100'));
  assert.ok(summary.includes('Đã chuyển đổi cơ hội: 25 (25%)'));
});
