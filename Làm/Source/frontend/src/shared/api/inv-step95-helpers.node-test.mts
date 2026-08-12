import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateNegativeStockSetting,
  validatePurchaseReceiptCreate,
  validateProductionReceiptCreate,
  validateStocktakeAdjustment,
} from './inv-step95-helpers.ts';

test('UC_INV_016: validateNegativeStockSetting', () => {
  const allow = validateNegativeStockSetting(true);
  assert.match(allow.policyText, /Chấp nhận tồn âm/);

  const forbid = validateNegativeStockSetting(false);
  assert.match(forbid.policyText, /Không cho phép tồn âm/);
});

test('UC_INV_017: validatePurchaseReceiptCreate', () => {
  const valid = validatePurchaseReceiptCreate('WH-01', 'GRN-01');
  assert.equal(valid.canCreate, true);

  const noGrn = validatePurchaseReceiptCreate('WH-01', '');
  assert.equal(noGrn.canCreate, false);
  assert.match(noGrn.error!, /GRN/);
});

test('UC_INV_018: validateProductionReceiptCreate', () => {
  const valid = validateProductionReceiptCreate('WH-01', 3);
  assert.equal(valid.canCreate, true);

  const emptyLines = validateProductionReceiptCreate('WH-01', 0);
  assert.equal(emptyLines.canCreate, false);
});

test('UC_INV_019: validateStocktakeAdjustment', () => {
  const surplus = validateStocktakeAdjustment(105, 100);
  assert.equal(surplus.varianceQty, 5);
  assert.equal(surplus.adjustmentType, 'Increase');

  const shortage = validateStocktakeAdjustment(95, 100);
  assert.equal(shortage.varianceQty, -5);
  assert.equal(shortage.adjustmentType, 'Decrease');

  const match = validateStocktakeAdjustment(100, 100);
  assert.equal(match.varianceQty, 0);
  assert.equal(match.adjustmentType, 'None');
});
