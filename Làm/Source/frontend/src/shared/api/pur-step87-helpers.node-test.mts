import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateVendorContact,
  validateVendorProductMapping,
  validatePurchaseRequestCreate,
  validatePrApproval,
} from './pur-step87-helpers.ts';

test('UC_PUR_003: validateVendorContact', () => {
  const valid = validateVendorContact('Nguyễn Văn A', 'a@vendor.com', '0901234567');
  assert.equal(valid.isValid, true);

  const emptyName = validateVendorContact('', 'a@vendor.com');
  assert.equal(emptyName.isValid, false);

  const invalidEmail = validateVendorContact('Nguyễn Văn A', 'invalid-email');
  assert.equal(invalidEmail.isValid, false);
  assert.match(invalidEmail.error!, /Email/);
});

test('UC_PUR_009: validateVendorProductMapping', () => {
  const valid = validateVendorProductMapping('VEND-01', 'SKU-100', 50000);
  assert.equal(valid.isValid, true);

  const invalidPrice = validateVendorProductMapping('VEND-01', 'SKU-100', 0);
  assert.equal(invalidPrice.isValid, false);
  assert.match(invalidPrice.error!, /Đơn giá mua/);
});

test('UC_PUR_014: validatePurchaseRequestCreate', () => {
  const valid = validatePurchaseRequestCreate('DEPT-IT', 2);
  assert.equal(valid.canCreate, true);

  const noDept = validatePurchaseRequestCreate('', 2);
  assert.equal(noDept.canCreate, false);

  const emptyLines = validatePurchaseRequestCreate('DEPT-IT', 0);
  assert.equal(emptyLines.canCreate, false);
  assert.match(emptyLines.reason!, /ít nhất 1 dòng/);
});

test('UC_PUR_017: validatePrApproval', () => {
  const valid = validatePrApproval('Submitted');
  assert.equal(valid.canApprove, true);

  const draft = validatePrApproval('Draft');
  assert.equal(draft.canApprove, false);
  assert.match(draft.reason!, /Submitted/);
});
