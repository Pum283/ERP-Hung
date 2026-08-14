import test from 'node:test';
import assert from 'node:assert/strict';
import {
  getRequisitionStatusBadge,
  getSlowMovingRiskLevelBadge,
} from './inv-material-requisition-approval-slow-moving-helpers.ts';

test('getRequisitionStatusBadge - maps status correctly to labels and badge classes', () => {
  const approved = getRequisitionStatusBadge('Approved');
  assert.equal(approved.label, 'Đã Phê Duyệt');
  assert.match(approved.colorClass, /bg-emerald/);

  const converted = getRequisitionStatusBadge('ConvertedToIssue');
  assert.equal(converted.label, 'Đã Xuất Kho Cấp Phát');
  assert.match(converted.colorClass, /bg-blue/);
});

test('getSlowMovingRiskLevelBadge - maps slow moving risk level correctly', () => {
  const high = getSlowMovingRiskLevelBadge('HighRisk');
  assert.equal(high.label, 'Nguy Cơ Cao (>180 ngày)');
  assert.match(high.colorClass, /bg-rose/);
});
