import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePickCompletion,
  validateWaybillPrint,
  validateDeliveryCancellation,
  validateDriverAssignment,
} from './log-step105-helpers.ts';

test('UC_LOG_009: validatePickCompletion', () => {
  const complete = validatePickCompletion(10, 10);
  assert.equal(complete.isComplete, true);

  const overflow = validatePickCompletion(12, 10);
  assert.equal(overflow.isComplete, false);
  assert.match(overflow.error!, /vượt quá/);
});

test('UC_LOG_011: validateWaybillPrint', () => {
  const valid = validateWaybillPrint('Ready');
  assert.equal(valid.canPrint, true);

  const draft = validateWaybillPrint('Draft');
  assert.equal(draft.canPrint, false);
  assert.match(draft.reason!, /Dự thảo/);
});

test('UC_LOG_012: validateDeliveryCancellation', () => {
  const valid = validateDeliveryCancellation('InTransit');
  assert.equal(valid.canCancel, true);

  const delivered = validateDeliveryCancellation('Delivered');
  assert.equal(delivered.canCancel, false);
  assert.match(delivered.reason!, /hoàn thành/);
});

test('UC_LOG_013: validateDriverAssignment', () => {
  const valid = validateDriverAssignment('Tài xế Minh', undefined);
  assert.equal(valid.hasAssignee, true);

  const empty = validateDriverAssignment('', '');
  assert.equal(empty.hasAssignee, false);
  assert.match(empty.message!, /Chưa chọn/);
});
