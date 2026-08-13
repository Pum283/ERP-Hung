import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculatePosCashRounding,
  formatComboDiscountSavings,
} from './pos-pricing-rounding-combo-helpers.ts';

test('calculatePosCashRounding - rounds payment total to nearest 500 VND', () => {
  const r1 = calculatePosCashRounding(123400, 500);
  assert.equal(r1.roundedTotalVnd, 123500);
  assert.equal(r1.roundingDifferenceVnd, 100);

  const r2 = calculatePosCashRounding(123100, 500);
  assert.equal(r2.roundedTotalVnd, 123000);
  assert.equal(r2.roundingDifferenceVnd, -100);
});

test('formatComboDiscountSavings - calculates combo discount savings', () => {
  const savings = formatComboDiscountSavings(45000, 60000);
  assert.equal(savings.savingsVnd, 15000);
  assert.equal(savings.savingsPercent, 25);
});
