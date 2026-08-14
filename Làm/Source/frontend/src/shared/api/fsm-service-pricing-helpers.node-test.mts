import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatHourlyRate,
  formatTravelFee,
} from './fsm-service-pricing-helpers.ts';

test('formatHourlyRate - formats rate per hour with VND', () => {
  assert.equal(formatHourlyRate(250000), '250.000 đ / Giờ');
});

test('formatTravelFee - formats travel fee per trip with VND', () => {
  assert.equal(formatTravelFee(150000), '150.000 đ / Lượt di chuyển');
});
