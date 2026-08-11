import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatWinLossStageNotice,
  formatWinRateReportSummary,
  validateQuoteHeaderInput,
  calculateQuoteLineSummary,
} from './crm-step71-helpers.ts';

// ─── UC_CRM_068: formatWinLossStageNotice ───

test('formatWinLossStageNotice - Won and Lost notices formatted correctly', () => {
  const won = formatWinLossStageNotice('Won');
  assert.equal(won.color, 'green');
  assert.ok(won.title.includes('Closed-Won'));

  const lost = formatWinLossStageNotice('Lost', 'Khách chọn đối thủ');
  assert.equal(lost.color, 'red');
  assert.ok(lost.title.includes('Lý do: Khách chọn đối thủ'));
});

// ─── UC_CRM_069: formatWinRateReportSummary ───

test('formatWinRateReportSummary - formats win rate summary text accurately', () => {
  const summary = formatWinRateReportSummary(10, 7, 3, 70);
  assert.ok(summary.includes('70%'));
  assert.ok(summary.includes('7/10'));
});

// ─── UC_CRM_070: validateQuoteHeaderInput ───

test('validateQuoteHeaderInput - valid dates return isValid true', () => {
  const res = validateQuoteHeaderInput({ quoteDate: '2026-08-01', validUntil: '2026-08-30' });
  assert.equal(res.isValid, true);
});

test('validateQuoteHeaderInput - validUntil before quoteDate returns validation error', () => {
  const res = validateQuoteHeaderInput({ quoteDate: '2026-08-15', validUntil: '2026-08-01' });
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('không được nhỏ hơn'));
});

// ─── UC_CRM_071: calculateQuoteLineSummary ───

test('calculateQuoteLineSummary - calculates gross, discount and net amounts accurately', () => {
  const res = calculateQuoteLineSummary(5, 20000000, 10);
  assert.equal(res.isValid, true);
  assert.equal(res.grossAmount, 100000000);
  assert.equal(res.discountAmount, 10000000);
  assert.equal(res.netAmount, 90000000);
});

test('calculateQuoteLineSummary - invalid discount percent returns validation error', () => {
  const res = calculateQuoteLineSummary(1, 50000, 120);
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('từ 0% đến 100%'));
});
