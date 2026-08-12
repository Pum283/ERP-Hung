import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatStocktakeSummary,
  calculateInventoryValuation,
  validateFinJournalPosting,
  formatValuationReportTitle,
} from './inv-step102-helpers.ts';

test('UC_INV_055: formatStocktakeSummary', () => {
  const summary = formatStocktakeSummary(50, 2, 1);
  assert.match(summary.summary, /Tổng SKU đếm: 50/);
  assert.match(summary.summary, /Thừa: 2/);
  assert.match(summary.summary, /Thiếu: 1/);
});

test('UC_INV_060: calculateInventoryValuation', () => {
  const { totalValue } = calculateInventoryValuation(100, 15000);
  assert.equal(totalValue, 1500000);
});

test('UC_INV_062: validateFinJournalPosting', () => {
  const valid = validateFinJournalPosting('Posted');
  assert.equal(valid.canPostToFin, true);

  const draft = validateFinJournalPosting('Draft');
  assert.equal(draft.canPostToFin, false);
  assert.match(draft.reason!, /Posted/);
});

test('UC_INV_063: formatValuationReportTitle', () => {
  const title = formatValuationReportTitle('Kho Chính', '2026-08-12');
  assert.match(title.reportTitle, /Kho Chính/);
  assert.match(title.reportTitle, /2026-08-12/);
});
