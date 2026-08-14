import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateEan13BarcodeFormat,
  parseQrPayload,
} from './inv-product-image-barcode-qr-helpers.ts';

test('validateEan13BarcodeFormat - validates 13-digit EAN-13 barcode strings', () => {
  assert.equal(validateEan13BarcodeFormat('8935000123456'), true);
  assert.equal(validateEan13BarcodeFormat('12345'), false);
  assert.equal(validateEan13BarcodeFormat('ABC1234567890'), false);
});

test('parseQrPayload - parses product QR code payload structure', () => {
  const qr = 'ERP-PROD|p-100|SKU-MILK-1L|BC:8935000123456';
  const parsed = parseQrPayload(qr);

  assert.equal(parsed.isProductQr, true);
  assert.equal(parsed.productId, 'p-100');
  assert.equal(parsed.productCode, 'SKU-MILK-1L');
  assert.equal(parsed.barcode, '8935000123456');
});
