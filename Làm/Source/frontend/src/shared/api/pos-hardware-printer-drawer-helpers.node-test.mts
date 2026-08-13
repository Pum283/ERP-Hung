import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePrinterConfigForm,
  validateCashDrawerConfigForm,
} from './pos-hardware-printer-drawer-helpers.ts';

test('validatePrinterConfigForm - validates printer name and IP address', () => {
  assert.equal(validatePrinterConfigForm('', '192.168.1.200').isValid, false);
  assert.equal(validatePrinterConfigForm('Máy in Bếp', '').isValid, false);
  assert.equal(validatePrinterConfigForm('Máy in Bếp', '192.168.1.200').isValid, true);
});

test('validateCashDrawerConfigForm - validates drawer name and pulse command', () => {
  assert.equal(validateCashDrawerConfigForm('', '1B700019FA').isValid, false);
  assert.equal(validateCashDrawerConfigForm('Ngăn kéo', '').isValid, false);
  assert.equal(validateCashDrawerConfigForm('Ngăn kéo', '1B700019FA').isValid, true);
});
