import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateAdjustCreateRequest,
  validateEvidenceStorageKey,
  formatOvertimeHours,
  filterMissingAlertsByEmployee,
  type MissingAlertItem,
} from './hrm-step33-helpers.ts';

// ─── UC_HRM_120: validateAdjustCreateRequest ───

test('validateAdjustCreateRequest - valid input returns valid', () => {
  const res = validateAdjustCreateRequest({
    employeeId: 'emp-1',
    workDate: '2026-08-10',
    reason: 'Xin điều chỉnh do quên check-in',
    evidenceStorageKey: 'evidence/proof.png',
  });
  assert.equal(res.valid, true);
});

test('validateAdjustCreateRequest - short reason returns error', () => {
  const res = validateAdjustCreateRequest({
    employeeId: 'emp-1',
    workDate: '2026-08-10',
    reason: 'Lý',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('3 đến 500'));
});

// ─── UC_HRM_121: validateEvidenceStorageKey ───

test('validateEvidenceStorageKey - valid key returns valid', () => {
  const res = validateEvidenceStorageKey('evidence/2026/proof_1.jpg');
  assert.equal(res.valid, true);
});

test('validateEvidenceStorageKey - invalid characters returns error', () => {
  const res = validateEvidenceStorageKey('evidence/<invalid>|key.png');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ký tự cấm'));
});

// ─── UC_HRM_119: formatOvertimeHours ───

test('formatOvertimeHours - formats minutes into human readable string', () => {
  assert.equal(formatOvertimeHours(0), '0 giờ');
  assert.equal(formatOvertimeHours(150), '2 giờ 30 phút');
  assert.equal(formatOvertimeHours(120), '2 giờ');
  assert.equal(formatOvertimeHours(45), '45 phút');
});

// ─── UC_HRM_117: filterMissingAlertsByEmployee ───

test('filterMissingAlertsByEmployee - filters alerts by keyword', () => {
  const alerts: MissingAlertItem[] = [
    { employeeId: '1', employeeCode: 'EMP_01', employeeName: 'Lê Văn A', alertType: 'MissingCheckIn' },
    { employeeId: '2', employeeCode: 'EMP_02', employeeName: 'Phạm Thị B', alertType: 'MissingCheckout' },
  ];

  const res = filterMissingAlertsByEmployee(alerts, 'Lê Văn');
  assert.equal(res.length, 1);
  assert.equal(res[0].employeeCode, 'EMP_01');
});
