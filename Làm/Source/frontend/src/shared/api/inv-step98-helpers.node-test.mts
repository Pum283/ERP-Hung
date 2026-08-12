import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateTransferShipment,
  formatTransferStatusBadge,
  validateCentralWarehouseDistribution,
  validateReservationCreate,
} from './inv-step98-helpers.ts';

test('UC_INV_033: validateTransferShipment', () => {
  const valid = validateTransferShipment('Draft');
  assert.equal(valid.canShip, true);

  const completed = validateTransferShipment('Completed');
  assert.equal(completed.canShip, false);
  assert.match(completed.reason!, /Dự thảo/);
});

test('UC_INV_035: formatTransferStatusBadge', () => {
  const inTransit = formatTransferStatusBadge('InTransit');
  assert.equal(inTransit.badgeStyle, 'warning');
  assert.match(inTransit.label, /Đang vận chuyển/);

  const completed = formatTransferStatusBadge('Completed');
  assert.equal(completed.badgeStyle, 'success');
});

test('UC_INV_036: validateCentralWarehouseDistribution', () => {
  const central = validateCentralWarehouseDistribution('KHO-TONG');
  assert.equal(central.isCentralWarehouse, true);

  const branch = validateCentralWarehouseDistribution('KHO-CHINHANH-01');
  assert.equal(branch.isCentralWarehouse, false);
});

test('UC_INV_037: validateReservationCreate', () => {
  const valid = validateReservationCreate('WH-01', 'SKU-01', 10);
  assert.equal(valid.isValid, true);

  const invalidQty = validateReservationCreate('WH-01', 'SKU-01', 0);
  assert.equal(invalidQty.isValid, false);
});
