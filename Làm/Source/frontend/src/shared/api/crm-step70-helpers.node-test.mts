import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatRevenueForecastSummary,
  calculateOpportunityLineTotal,
  validateCompetitorInfo,
  formatQuoteFromOppResult,
} from './crm-step70-helpers.ts';

// ─── UC_CRM_064: formatRevenueForecastSummary ───

test('formatRevenueForecastSummary - formats estimated and weighted forecast values', () => {
  const summary = formatRevenueForecastSummary(500000000, 300000000);
  assert.ok(summary.includes('Doanh thu dự kiến'));
  assert.ok(summary.includes('Giá trị gia trọng'));
});

// ─── UC_CRM_065: calculateOpportunityLineTotal ───

test('calculateOpportunityLineTotal - valid quantity and price calculates total', () => {
  const res = calculateOpportunityLineTotal(3, 50000000);
  assert.equal(res.isValid, true);
  assert.equal(res.lineAmount, 150000000);
});

test('calculateOpportunityLineTotal - zero or negative quantity returns validation error', () => {
  const res = calculateOpportunityLineTotal(0, 10000);
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Số lượng phải lớn hơn 0'));
});

test('calculateOpportunityLineTotal - negative price returns validation error', () => {
  const res = calculateOpportunityLineTotal(2, -5000);
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Đơn giá không được âm'));
});

// ─── UC_CRM_066: validateCompetitorInfo ───

test('validateCompetitorInfo - valid competitor info returns isValid true', () => {
  const res = validateCompetitorInfo({ competitorName: 'Đối thủ ABC', negotiationNotes: 'Cạnh tranh về tiến độ' });
  assert.equal(res.isValid, true);
});

// ─── UC_CRM_067: formatQuoteFromOppResult ───

test('formatQuoteFromOppResult - formats success quote creation message', () => {
  const msg = formatQuoteFromOppResult('BG-2026-001', 'quote-id-123');
  assert.ok(msg.includes('Đã tạo thành công Báo giá mới'));
  assert.ok(msg.includes('BG-2026-001'));
});
