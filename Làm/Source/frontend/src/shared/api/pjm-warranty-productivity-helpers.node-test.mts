import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatWarrantyPeriod,
  formatUtilizationPercent,
} from './pjm-warranty-productivity-helpers.ts';

test('formatWarrantyPeriod - formats warranty month duration', () => {
  assert.equal(formatWarrantyPeriod(24), '24 Tháng Bảo Hành');
});

test('formatUtilizationPercent - formats resource utilization rate', () => {
  assert.equal(formatUtilizationPercent(92.0), '92.0% Hiệu Suất Nguồn Lực');
});
