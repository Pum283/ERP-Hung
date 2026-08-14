import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatPercentageBreakdown,
  getPurposeColorIndicator,
} from './inv-dispatch-purpose-report-helpers.ts';

test('formatPercentageBreakdown - formats number to 1 decimal place with %', () => {
  assert.equal(formatPercentageBreakdown(56.666), '56.7%');
  assert.equal(formatPercentageBreakdown(20), '20.0%');
});

test('getPurposeColorIndicator - returns matching color style class based on dispatch purpose', () => {
  assert.match(getPurposeColorIndicator('Xuất Bán Hàng'), /text-blue/);
  assert.match(getPurposeColorIndicator('Xuất Dự Án'), /text-purple/);
  assert.match(getPurposeColorIndicator('Khác'), /text-slate/);
});
