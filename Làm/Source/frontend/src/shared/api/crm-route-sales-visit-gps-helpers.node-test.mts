import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateGpsDistanceKm,
  formatVisitFrequencyLabel,
  validateGpsCoordinates,
} from './crm-route-sales-visit-gps-helpers.ts';

test('calculateGpsDistanceKm - calculates distance correctly between coordinates', () => {
  // District 1 HCM to District 3 HCM ~ 2.5 km
  const dist = calculateGpsDistanceKm(10.7769, 106.7009, 10.782, 106.687);
  assert.equal(dist > 0, true);
  assert.equal(dist < 10, true);
});

test('formatVisitFrequencyLabel - formats frequency strings', () => {
  assert.equal(formatVisitFrequencyLabel('weekly').includes('Hàng tuần'), true);
  assert.equal(formatVisitFrequencyLabel('biweekly').includes('2 tuần'), true);
  assert.equal(formatVisitFrequencyLabel('monthly').includes('Hàng tháng'), true);
});

test('validateGpsCoordinates - checks valid and invalid lat/lng', () => {
  assert.equal(validateGpsCoordinates(10.7769, 106.7009).isValid, true);
  assert.equal(validateGpsCoordinates(100, 106.7009).isValid, false);
  assert.equal(validateGpsCoordinates(10.7769, 200).isValid, false);
});
