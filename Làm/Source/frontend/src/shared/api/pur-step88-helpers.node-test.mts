import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePrRejection,
  formatPrStatusBadge,
  validatePoFromPrCreation,
  validatePoLimitApproval,
} from './pur-step88-helpers.ts';

test('UC_PUR_018: validatePrRejection', () => {
  const valid = validatePrRejection('Submitted', 'Vượt ngân sách tháng');
  assert.equal(valid.canReject, true);

  const noReason = validatePrRejection('Submitted', '');
  assert.equal(noReason.canReject, false);
  assert.match(noReason.error!, /lý do/);

  const draft = validatePrRejection('Draft', 'Lý do');
  assert.equal(draft.canReject, false);
});

test('UC_PUR_019: formatPrStatusBadge', () => {
  const approved = formatPrStatusBadge('Approved');
  assert.equal(approved.badgeStyle, 'success');
  assert.match(approved.label, /Đã duyệt/);

  const rejected = formatPrStatusBadge('Rejected');
  assert.equal(rejected.badgeStyle, 'danger');
});

test('UC_PUR_026: validatePoFromPrCreation', () => {
  const valid = validatePoFromPrCreation('Approved', 'VEND-01');
  assert.equal(valid.canCreate, true);

  const unapproved = validatePoFromPrCreation('Submitted', 'VEND-01');
  assert.equal(unapproved.canCreate, false);
  assert.match(unapproved.error!, /Approved/);

  const noVendor = validatePoFromPrCreation('Approved', '');
  assert.equal(noVendor.canCreate, false);
});

test('UC_PUR_027: validatePoLimitApproval', () => {
  const within = validatePoLimitApproval(50000000, 100000000);
  assert.equal(within.isWithinLimit, true);

  const exceed = validatePoLimitApproval(150000000, 100000000);
  assert.equal(exceed.isWithinLimit, false);
  assert.match(exceed.error!, /vượt quá hạn mức/);
});
