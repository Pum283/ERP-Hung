import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateTransferReceipt,
  validateLotExpiryInput,
  validateSalesIssue,
  validateProductionIssue,
} from './inv-step96-helpers.ts';

test('UC_INV_020: validateTransferReceipt', () => {
  const valid = validateTransferReceipt('WH-K1', 'WH-K2');
  assert.equal(valid.canReceipt, true);

  const sameWh = validateTransferReceipt('WH-K1', 'WH-K1');
  assert.equal(sameWh.canReceipt, false);
  assert.match(sameWh.error!, /trùng nhau/);
});

test('UC_INV_022: validateLotExpiryInput', () => {
  const valid = validateLotExpiryInput('LOT-2026-08', '2026-12-31');
  assert.equal(valid.isValid, true);

  const invalidDate = validateLotExpiryInput('LOT-2026-08', 'invalid-date');
  assert.equal(invalidDate.isValid, false);
});

test('UC_INV_024: validateSalesIssue', () => {
  const valid = validateSalesIssue('WH-01', 5);
  assert.equal(valid.canIssue, true);

  const empty = validateSalesIssue('WH-01', 0);
  assert.equal(empty.canIssue, false);
});

test('UC_INV_025: validateProductionIssue', () => {
  const valid = validateProductionIssue('WH-01', 2);
  assert.equal(valid.canIssue, true);

  const noWh = validateProductionIssue('', 2);
  assert.equal(noWh.canIssue, false);
});
