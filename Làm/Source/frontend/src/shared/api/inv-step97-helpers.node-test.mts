import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateInternalIssue,
  validateFefoPicking,
  validateAdjustmentIssue,
  validateTransferCreate,
} from './inv-step97-helpers.ts';

test('UC_INV_026: validateInternalIssue', () => {
  const valid = validateInternalIssue('WH-01', 'Dùng cho văn phòng');
  assert.equal(valid.canIssue, true);

  const noReason = validateInternalIssue('WH-01', '');
  assert.equal(noReason.canIssue, false);
  assert.match(noReason.error!, /lý do/);
});

test('UC_INV_029: validateFefoPicking', () => {
  const valid = validateFefoPicking(10, 50);
  assert.equal(valid.canFefoPick, true);

  const insufficient = validateFefoPicking(100, 50);
  assert.equal(insufficient.canFefoPick, false);
  assert.match(insufficient.reason!, /không đủ/);
});

test('UC_INV_030: validateAdjustmentIssue', () => {
  const valid = validateAdjustmentIssue('WH-01', 'Hàng hư hỏng quá hạn');
  assert.equal(valid.canIssue, true);

  const noReason = validateAdjustmentIssue('WH-01', '');
  assert.equal(noReason.canIssue, false);
});

test('UC_INV_031: validateTransferCreate', () => {
  const valid = validateTransferCreate('WH-A', 'WH-B');
  assert.equal(valid.isValid, true);

  const sameWh = validateTransferCreate('WH-A', 'WH-A');
  assert.equal(sameWh.isValid, false);
});
