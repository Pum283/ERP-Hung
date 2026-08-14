import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatOnTimeRate,
  formatWeightTons,
} from './log-driver-productivity-cost-helpers.ts';

test('formatOnTimeRate - formats rate to 1 decimal place with %', () => {
  assert.equal(formatOnTimeRate(98.54), '98.5%');
});

test('formatWeightTons - formats kg to tons with 1 decimal place', () => {
  assert.equal(formatWeightTons(18500), '18.5 Tấn');
});
