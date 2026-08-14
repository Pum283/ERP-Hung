import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatWarrantyApprovalRate,
  formatClaimAmount,
} from './fsm-warranty-claim-report-helpers.ts';

test('formatWarrantyApprovalRate - formats warranty approval rate', () => {
  assert.equal(formatWarrantyApprovalRate(91.4), '91.4% Duyệt Bảo Hành');
});

test('formatClaimAmount - formats warranty covered expense amount', () => {
  assert.equal(formatClaimAmount(155000000), '155.000.000 đ Chi Phí');
});
