import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatYieldPercentage,
  formatBatchQuantity,
} from './mfg-quarantine-yield-batch-param-helpers.ts';

test('formatYieldPercentage - formats pass rate percentage', () => {
  assert.equal(formatYieldPercentage(97.55), '97.5% Đạt Chuẩn');
});

test('formatBatchQuantity - formats actual and planned quantity', () => {
  assert.equal(formatBatchQuantity(280, 300), '280 / 300 SP');
});
