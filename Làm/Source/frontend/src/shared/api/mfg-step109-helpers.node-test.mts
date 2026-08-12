import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatCodReportSummary,
  calculateLogisticsDashboardKpi,
  validateFinishedGoodItem,
  validateRawMaterialItem,
} from './mfg-step109-helpers.ts';

test('UC_LOG_038: formatCodReportSummary', () => {
  const summary = formatCodReportSummary(500000, 1000000, 800000);
  assert.match(summary.summaryText, /Chờ thu:/);
  assert.match(summary.summaryText, /Đã thu:/);
});

test('UC_LOG_039: calculateLogisticsDashboardKpi', () => {
  const kpi = calculateLogisticsDashboardKpi(80, 15, 5);
  assert.equal(kpi.totalOrders, 100);
  assert.equal(kpi.successRatePct, 80);
});

test('UC_MFG_001: validateFinishedGoodItem', () => {
  const validFg = validateFinishedGoodItem('FG-001', 'Áo Nam', 'FG');
  assert.equal(validFg.isValid, true);

  const invalidType = validateFinishedGoodItem('RM-001', 'Vải Kaki', 'RM');
  assert.equal(invalidType.isValid, false);
});

test('UC_MFG_002: validateRawMaterialItem', () => {
  const validRm = validateRawMaterialItem('RM-101', 'Chỉ May High-Polymer', 15000);
  assert.equal(validRm.isValid, true);

  const negativeCost = validateRawMaterialItem('RM-102', 'Nút Áo', -500);
  assert.equal(negativeCost.isValid, false);
});
