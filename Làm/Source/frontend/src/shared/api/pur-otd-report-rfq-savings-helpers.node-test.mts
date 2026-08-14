import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateOtdRating,
  calculateRfqNegotiationSavings,
} from './pur-otd-report-rfq-savings-helpers.ts';

test('calculateOtdRating - classifies vendor OTD performance correctly', () => {
  assert.equal(calculateOtdRating(96), 'Excellent');
  assert.equal(calculateOtdRating(88), 'Good');
  assert.equal(calculateOtdRating(75), 'Poor');
});

test('calculateRfqNegotiationSavings - calculates RFQ savings amount and percentage', () => {
  const res = calculateRfqNegotiationSavings(300000000, 240000000);
  assert.equal(res.savingsAmount, 60000000);
  assert.equal(res.savingsPercentage, 20);
});
