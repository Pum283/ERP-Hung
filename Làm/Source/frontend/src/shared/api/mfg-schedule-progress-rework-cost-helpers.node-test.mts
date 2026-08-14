import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatDefectRate,
  formatUnitCost,
} from './mfg-schedule-progress-rework-cost-helpers.ts';

test('formatDefectRate - calculates and formats defect rate', () => {
  assert.equal(formatDefectRate(95, 5), '5.0% Lỗi');
  assert.equal(formatDefectRate(0, 0), '0.0%');
});

test('formatUnitCost - formats unit cost with VND suffix', () => {
  assert.equal(formatUnitCost(150000), '150.000 đ/SP');
});
