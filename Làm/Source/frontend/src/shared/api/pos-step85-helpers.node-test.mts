import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateCancelDiscountRates,
  formatCancelDiscountNotice,
  calculateCostVariance,
  formatTopProductRank,
  formatStoreCompareRow,
} from './pos-step85-helpers.ts';

test('UC_POS_064: calculateCancelDiscountRates', () => {
  const res = calculateCancelDiscountRates(10000000, 500000, 1000000);
  assert.equal(res.cancelRatePct, 5);
  assert.equal(res.discountRatePct, 10);

  const zeroGross = calculateCancelDiscountRates(0, 500000, 1000000);
  assert.equal(zeroGross.cancelRatePct, 0);
  assert.equal(zeroGross.discountRatePct, 0);
});

test('UC_POS_064: formatCancelDiscountNotice', () => {
  const notice = formatCancelDiscountNotice(3.5, 12.0);
  assert.match(notice, /3.5%/);
  assert.match(notice, /12%/);
  assert.match(notice, /Normal Cancel/);
});

test('UC_POS_065: calculateCostVariance', () => {
  const within = calculateCostVariance(100000, 103000, 5);
  assert.equal(within.variance, 3000);
  assert.equal(within.variancePct, 3);
  assert.equal(within.isWithinTolerance, true);

  const exceeded = calculateCostVariance(100000, 110000, 5);
  assert.equal(exceeded.variance, 10000);
  assert.equal(exceeded.variancePct, 10);
  assert.equal(exceeded.isWithinTolerance, false);
});

test('UC_POS_066: formatTopProductRank', () => {
  const rank1 = formatTopProductRank(1, 'Cà phê Espresso', 150, 4500000);
  assert.match(rank1, /🥇/);
  assert.match(rank1, /Cà phê Espresso/);
  assert.match(rank1, /150/);

  const rank4 = formatTopProductRank(4, 'Bánh Mì Kẹp', 80, 2400000);
  assert.match(rank4, /#4/);
});

test('UC_POS_067: formatStoreCompareRow', () => {
  const row = formatStoreCompareRow('CH Quận 1', 120, 15000000, 35.5);
  assert.match(row, /CH Quận 1/);
  assert.match(row, /120 đơn/);
  assert.match(row, /15.000.000/);
  assert.match(row, /35.5%/);
});
