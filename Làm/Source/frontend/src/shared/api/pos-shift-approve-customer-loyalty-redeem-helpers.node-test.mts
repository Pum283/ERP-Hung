import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculatePosEarnedLoyaltyPoints,
  calculatePosRedeemPointsDiscount,
} from './pos-shift-approve-customer-loyalty-redeem-helpers.ts';

test('calculatePosEarnedLoyaltyPoints - calculates earned points from order total', () => {
  assert.equal(calculatePosEarnedLoyaltyPoints(150000), 15);
  assert.equal(calculatePosEarnedLoyaltyPoints(45000), 4);
});

test('calculatePosRedeemPointsDiscount - calculates discount amount from points redeemed', () => {
  assert.equal(calculatePosRedeemPointsDiscount(50), 50000);
  assert.equal(calculatePosRedeemPointsDiscount(100), 100000);
});
