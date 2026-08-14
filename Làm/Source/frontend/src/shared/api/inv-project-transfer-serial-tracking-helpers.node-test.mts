import test from 'node:test';
import assert from 'node:assert/strict';
import {
  getTransferApprovalStatusBadge,
  formatSerialLifecycleSummary,
} from './inv-project-transfer-serial-tracking-helpers.ts';

test('getTransferApprovalStatusBadge - maps status correctly to labels and badge classes', () => {
  const approved = getTransferApprovalStatusBadge('Approved');
  assert.equal(approved.label, 'Đã Phê Duyệt');
  assert.match(approved.colorClass, /bg-emerald/);

  const pending = getTransferApprovalStatusBadge('PendingApproval');
  assert.equal(pending.label, 'Chờ Ban Giám Đốc Duyệt');
  assert.match(pending.colorClass, /bg-amber/);
});

test('formatSerialLifecycleSummary - formats event count and location string', () => {
  const s = formatSerialLifecycleSummary(3, 'Kho Chi Nhánh Hà Nội');
  assert.equal(s, 'Đã qua 3 chặng luân chuyển (Hiện tại: Kho Chi Nhánh Hà Nội)');
});
