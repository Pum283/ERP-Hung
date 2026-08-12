import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatPeriodSummaryTitle,
  formatStockCardRow,
  validateStockLevelThreshold,
  formatDashboardWidgetStats,
} from './inv-step103-helpers.ts';

test('UC_INV_064: formatPeriodSummaryTitle', () => {
  const title = formatPeriodSummaryTitle('2026-08-01', '2026-08-31');
  assert.match(title.title, /2026-08-01/);
  assert.match(title.title, /2026-08-31/);
});

test('UC_INV_065: formatStockCardRow', () => {
  const row = formatStockCardRow('2026-08-12', 'NK-001', 50, 0, 150);
  assert.match(row.formattedRowText, /NK-001/);
  assert.match(row.formattedRowText, /Tồn cuối: 150/);
});

test('UC_INV_067: validateStockLevelThreshold', () => {
  const low = validateStockLevelThreshold(5, 10, 100);
  assert.equal(low.status, 'LOW');
  assert.match(low.message!, /tối thiểu/);

  const high = validateStockLevelThreshold(120, 10, 100);
  assert.equal(high.status, 'HIGH');

  const ok = validateStockLevelThreshold(50, 10, 100);
  assert.equal(ok.status, 'OK');
});

test('UC_INV_069: formatDashboardWidgetStats', () => {
  const stats = formatDashboardWidgetStats(1200, 15, 5);
  assert.equal(stats.warningCount, 20);
  assert.match(stats.widgetTitle, /20 cảnh báo/);
});
