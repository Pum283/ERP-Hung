import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateSplitBillSelection,
  validateKitchenNoteLength,
} from './pos-promo-report-bill-order-ops-helpers.ts';

test('validateSplitBillSelection - checks selected items array', () => {
  assert.equal(validateSplitBillSelection([]).isValid, false);
  assert.equal(validateSplitBillSelection(['item-1', 'item-2']).isValid, true);
});

test('validateKitchenNoteLength - checks max note length', () => {
  const longStr = 'a'.repeat(250);
  assert.equal(validateKitchenNoteLength(longStr).isValid, false);
  assert.equal(validateKitchenNoteLength('Ít đường, không đá').isValid, true);
});
