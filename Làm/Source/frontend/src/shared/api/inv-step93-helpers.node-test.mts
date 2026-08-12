import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateUnitConversion,
  validateLotSerialRequirement,
  validateCostingMethod,
  validateSkuStatusChange,
} from './inv-step93-helpers.ts';

test('UC_INV_003: validateUnitConversion', () => {
  const valid = validateUnitConversion('UOM-THUNG', 'UOM-CAI', 24);
  assert.equal(valid.isValid, true);

  const sameUom = validateUnitConversion('UOM-CAI', 'UOM-CAI', 1);
  assert.equal(sameUom.isValid, false);
  assert.match(sameUom.error!, /trùng nhau/);

  const invalidFactor = validateUnitConversion('UOM-THUNG', 'UOM-CAI', 0);
  assert.equal(invalidFactor.isValid, false);
});

test('UC_INV_004: validateLotSerialRequirement', () => {
  const lotAndExpiry = validateLotSerialRequirement(true, false, true);
  assert.match(lotAndExpiry.flagsSummary, /Theo dõi Lô/);
  assert.match(lotAndExpiry.flagsSummary, /Hạn sử dụng/);

  const normal = validateLotSerialRequirement(false, false, false);
  assert.equal(normal.flagsSummary, 'Hàng thông thường');
});

test('UC_INV_005: validateCostingMethod', () => {
  const fifo = validateCostingMethod('FIFO');
  assert.equal(fifo.isValid, true);

  const invalid = validateCostingMethod('INVALID');
  assert.equal(invalid.isValid, false);
  assert.match(invalid.error!, /MovingAverage, FIFO/);
});

test('UC_INV_007: validateSkuStatusChange', () => {
  const valid = validateSkuStatusChange('Active', 'Inactive');
  assert.equal(valid.canChange, true);

  const same = validateSkuStatusChange('Inactive', 'Inactive');
  assert.equal(same.canChange, false);
});
