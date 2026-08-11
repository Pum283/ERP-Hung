import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatExpenseTypeBadge,
  validateExpenseInput,
  calculateBudgetBurnRate,
  formatCampaignRoiSummary,
} from './crm-step61-helpers.ts';

// ─── UC_CRM_019: formatExpenseTypeBadge ───

test('formatExpenseTypeBadge - returns correct labels and icons for expense types', () => {
  assert.ok(formatExpenseTypeBadge('Ads').label.includes('Quảng cáo'));
  assert.ok(formatExpenseTypeBadge('Agency').label.includes('Agency'));
  assert.ok(formatExpenseTypeBadge('Other').icon.includes('💸'));
});

// ─── UC_CRM_019: validateExpenseInput ───

test('validateExpenseInput - valid expense returns no errors', () => {
  const res = validateExpenseInput(5000000, 'Ads', 'Chi phí chạy Google Ads');
  assert.equal(res.isValid, true);
  assert.equal(res.errors.length, 0);
});

test('validateExpenseInput - zero or negative amount returns validation error', () => {
  const res = validateExpenseInput(0, 'Media');
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('lớn hơn 0')));
});

test('validateExpenseInput - invalid expense type returns error', () => {
  const res = validateExpenseInput(1000000, 'InvalidType');
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('Loại chi phí')));
});

// ─── UC_CRM_020: calculateBudgetBurnRate ───

test('calculateBudgetBurnRate - calculates burn rate and detects over-budget state', () => {
  const normal = calculateBudgetBurnRate(100000000, 50000000);
  assert.equal(normal.burnRatePct, 50);
  assert.equal(normal.isOverBudget, false);
  assert.ok(normal.statusLabel.includes('50%'));

  const over = calculateBudgetBurnRate(100000000, 120000000);
  assert.equal(over.isOverBudget, true);
  assert.ok(over.statusLabel.includes('Vượt ngân sách'));
});

// ─── UC_CRM_021: formatCampaignRoiSummary ───

test('formatCampaignRoiSummary - formats ROI summary string with positive ROI percentage', () => {
  const summary = formatCampaignRoiSummary(20000000, 50000000, 150);
  assert.ok(summary.includes('Chi 20.000.000'));
  assert.ok(summary.includes('Thu 50.000.000'));
  assert.ok(summary.includes('+150%'));
});
