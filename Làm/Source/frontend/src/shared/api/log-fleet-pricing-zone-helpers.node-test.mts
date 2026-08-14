import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatVehiclePayload,
  formatEstimatedTransitTime,
} from './log-fleet-pricing-zone-helpers.ts';

test('formatVehiclePayload - formats kg to tons if >= 1000', () => {
  assert.equal(formatVehiclePayload(2500), '2.5 Tấn');
  assert.equal(formatVehiclePayload(500), '500 Kg');
});

test('formatEstimatedTransitTime - formats transit hours correctly', () => {
  assert.equal(formatEstimatedTransitTime(4), '4 Giờ');
});
