import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePoSendRequest,
  validatePoRevision,
  formatPartialReceivingStatus,
  validatePoCloseOrCancel,
} from './pur-step89-helpers.ts';

test('UC_PUR_028: validatePoSendRequest', () => {
  const valid = validatePoSendRequest('Approved');
  assert.equal(valid.canSend, true);

  const draft = validatePoSendRequest('Draft');
  assert.equal(draft.canSend, false);
  assert.match(draft.error!, /Approved/);
});

test('UC_PUR_030: validatePoRevision', () => {
  const validSent = validatePoRevision('Sent');
  assert.equal(validSent.canRevise, true);

  const closed = validatePoRevision('Closed');
  assert.equal(closed.canRevise, false);
});

test('UC_PUR_031: formatPartialReceivingStatus', () => {
  const partial = formatPartialReceivingStatus(100, 40);
  assert.equal(partial.percentComplete, 40);
  assert.match(partial.statusText, /40%/);

  const full = formatPartialReceivingStatus(100, 100);
  assert.equal(full.percentComplete, 100);
  assert.match(full.statusText, /100%/);
});

test('UC_PUR_032: validatePoCloseOrCancel', () => {
  const cancelReceived = validatePoCloseOrCancel('Sent', 10, 'Cancel');
  assert.equal(cancelReceived.canExecute, false);
  assert.match(cancelReceived.error!, /không thể Hủy/);

  const closeReceived = validatePoCloseOrCancel('Sent', 10, 'Close');
  assert.equal(closeReceived.canExecute, true);
});
