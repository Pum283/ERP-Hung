import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatScrapPercentage,
  formatGrossRequirement,
} from './mfg-scrap-bom-demand-mrp-helpers.ts';

test('formatScrapPercentage - formats percentage with plus prefix and label', () => {
  assert.equal(formatScrapPercentage(5), '+5.0% Hao Hụt');
});

test('formatGrossRequirement - calculates gross quantity including scrap allowance', () => {
  assert.equal(formatGrossRequirement(100, 5), 105);
  assert.equal(formatGrossRequirement(200, 3.5), 207);
});
