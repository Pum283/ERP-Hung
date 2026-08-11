import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateMobilizationRequest,
  validateOrderAcknowledge,
  filterActiveTracking,
  validateAttendanceTag,
  type MobilizationItem,
} from './hrm-step27-helpers.ts';

// ─── UC_HRM_093: validateMobilizationRequest ───

test('validateMobilizationRequest - valid form returns valid', () => {
  const res = validateMobilizationRequest({
    fromOrgUnitId: 'org-1',
    toOrgUnitId: 'org-2',
    startDate: '2026-09-01',
    requestedHeadcount: 5,
    reason: 'Đề xuất 5 nhân sự hỗ trợ công trường mới',
  });
  assert.equal(res.valid, true);
});

test('validateMobilizationRequest - same org units returns error', () => {
  const res = validateMobilizationRequest({
    fromOrgUnitId: 'org-1',
    toOrgUnitId: 'org-1',
    startDate: '2026-09-01',
    requestedHeadcount: 5,
    reason: 'Đề xuất nhân sự trùng đơn vị',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('phải khác nhau'));
});

test('validateMobilizationRequest - zero headcount returns error', () => {
  const res = validateMobilizationRequest({
    fromOrgUnitId: 'org-1',
    toOrgUnitId: 'org-2',
    startDate: '2026-09-01',
    requestedHeadcount: 0,
    reason: 'Đề xuất 0 người',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 1,000'));
});

test('validateMobilizationRequest - short reason returns error', () => {
  const res = validateMobilizationRequest({
    fromOrgUnitId: 'org-1',
    toOrgUnitId: 'org-2',
    startDate: '2026-09-01',
    requestedHeadcount: 5,
    reason: 'OK',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('3 đến 500'));
});

// ─── UC_HRM_094: validateOrderAcknowledge ───

test('validateOrderAcknowledge - Issued status returns valid', () => {
  const res = validateOrderAcknowledge('order-1', 'Issued');
  assert.equal(res.valid, true);
});

test('validateOrderAcknowledge - Draft status returns error', () => {
  const res = validateOrderAcknowledge('order-1', 'Draft');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Issued'));
});

test('validateOrderAcknowledge - empty orderId returns error', () => {
  const res = validateOrderAcknowledge('   ', 'Issued');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chọn lệnh'));
});

// ─── UC_HRM_095: filterActiveTracking ───

test('filterActiveTracking - returns only Orders in active statuses', () => {
  const items: MobilizationItem[] = [
    { id: '1', kind: 'Order', status: 'Issued' },
    { id: '2', kind: 'Order', status: 'Acknowledged' },
    { id: '3', kind: 'Order', status: 'Active' },
    { id: '4', kind: 'Order', status: 'Completed' },
    { id: '5', kind: 'Request', status: 'Approved' },
  ];
  const filtered = filterActiveTracking(items);
  assert.equal(filtered.length, 3);
  assert.deepEqual(filtered.map(x => x.id), ['1', '2', '3']);
});

// ─── UC_HRM_096: validateAttendanceTag ───

test('validateAttendanceTag - valid orderId & boolean returns valid', () => {
  const res = validateAttendanceTag('order-10', true);
  assert.equal(res.valid, true);
});

test('validateAttendanceTag - empty orderId returns error', () => {
  const res = validateAttendanceTag('   ', true);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chọn lệnh'));
});
