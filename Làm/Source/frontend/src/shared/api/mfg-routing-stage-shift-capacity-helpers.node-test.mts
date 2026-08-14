import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatCycleTime,
  formatEfficiencyPercentage,
} from './mfg-routing-stage-shift-capacity-helpers.ts';

test('formatCycleTime - formats minutes per item', () => {
  assert.equal(formatCycleTime(15), '15 Phút / SP');
});

test('formatEfficiencyPercentage - formats efficiency percentage', () => {
  assert.equal(formatEfficiencyPercentage(85), '85% Hiệu Suất');
});
