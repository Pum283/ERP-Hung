import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatOeePercentage,
  formatMixingRatio,
} from './mfg-pack-blend-oee-helpers.ts';

test('formatOeePercentage - formats OEE with percentage and suffix', () => {
  assert.equal(formatOeePercentage(79.77), '79.8% OEE');
});

test('formatMixingRatio - formats ratio with tolerance range', () => {
  assert.equal(formatMixingRatio(25, 0.5), '25.0% (±0.5%)');
});
