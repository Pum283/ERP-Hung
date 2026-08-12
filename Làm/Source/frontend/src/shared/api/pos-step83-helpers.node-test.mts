import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateCashVariance,
  formatVarianceNotice,
  validateShiftReportPrint,
  calculateBomMaterialRequirement,
  formatStockAlertBadge,
} from './pos-step83-helpers.ts';

test('UC_POS_047: calculateCashVariance - balanced, over, short', () => {
  const balanced = calculateCashVariance(500000, 500000);
  assert.equal(balanced.isBalanced, true);
  assert.equal(balanced.variance, 0);

  const over = calculateCashVariance(500000, 550000);
  assert.equal(over.isBalanced, false);
  assert.equal(over.status, 'Over');
  assert.equal(over.variance, 50000);

  const short = calculateCashVariance(500000, 480000);
  assert.equal(short.isBalanced, false);
  assert.equal(short.status, 'Short');
  assert.equal(short.variance, -20000);
});

test('UC_POS_047: formatVarianceNotice', () => {
  const notice1 = formatVarianceNotice(0);
  assert.match(notice1, /khớp 100%/);

  const notice2 = formatVarianceNotice(30000);
  assert.match(notice2, /Thừa tiền/);

  const notice3 = formatVarianceNotice(-15000);
  assert.match(notice3, /Thiếu tiền/);
});

test('UC_POS_048: validateShiftReportPrint', () => {
  const validClosed = validateShiftReportPrint('Closed');
  assert.equal(validClosed.canPrint, true);

  const validOpen = validateShiftReportPrint('Open');
  assert.equal(validOpen.canPrint, true);

  const invalid = validateShiftReportPrint('Cancelled');
  assert.equal(invalid.canPrint, false);
});

test('UC_POS_054: calculateBomMaterialRequirement', () => {
  const req = calculateBomMaterialRequirement(5, 0.05);
  assert.equal(req, 0.25);

  const zero = calculateBomMaterialRequirement(0, 0.05);
  assert.equal(zero, 0);
});

test('UC_POS_055: formatStockAlertBadge', () => {
  const out = formatStockAlertBadge('OutOfStock', 'CF01', 0);
  assert.equal(out.badgeStyle, 'danger');
  assert.match(out.label, /Hết hàng/);

  const below = formatStockAlertBadge('BelowMin', 'CF02', 2);
  assert.equal(below.badgeStyle, 'warning');
  assert.match(below.label, /Dưới mức tối thiểu/);
});
