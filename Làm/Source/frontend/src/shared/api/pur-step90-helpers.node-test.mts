import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePoPrintRequest,
  validateGrnCreationFromPo,
  validateGrnItemInspection,
  validateGrnInventoryPush,
} from './pur-step90-helpers.ts';

test('UC_PUR_033: validatePoPrintRequest', () => {
  const valid = validatePoPrintRequest('Sent');
  assert.equal(valid.canPrint, true);

  const draft = validatePoPrintRequest('Draft');
  assert.equal(draft.canPrint, false);
  assert.match(draft.reason!, /Nháp/);
});

test('UC_PUR_034: validateGrnCreationFromPo', () => {
  const valid = validateGrnCreationFromPo('Sent', 50);
  assert.equal(valid.canCreate, true);

  const invalidStatus = validateGrnCreationFromPo('Draft', 50);
  assert.equal(invalidStatus.canCreate, false);
});

test('UC_PUR_035: validateGrnItemInspection', () => {
  const valid = validateGrnItemInspection(45, 5, 50);
  assert.equal(valid.isValid, true);

  const mismatch = validateGrnItemInspection(40, 5, 50);
  assert.equal(mismatch.isValid, false);
  assert.match(mismatch.error!, /phải bằng tổng/);

  const negative = validateGrnItemInspection(-1, 5, 4);
  assert.equal(negative.isValid, false);
});

test('UC_PUR_037: validateGrnInventoryPush', () => {
  const valid = validateGrnInventoryPush('Posted');
  assert.equal(valid.canPush, true);

  const draft = validateGrnInventoryPush('Draft');
  assert.equal(draft.canPush, false);
  assert.match(draft.reason!, /Posted/);
});
