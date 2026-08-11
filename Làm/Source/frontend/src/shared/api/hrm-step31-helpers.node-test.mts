import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePunchRequest,
  validateDateRangeFilter,
  formatAttendanceStatus,
  filterDepartmentBoard,
  type AttendanceRecordItem,
} from './hrm-step31-helpers.ts';

// ─── UC_HRM_109 & 110: validatePunchRequest ───

test('validatePunchRequest - valid App method returns valid', () => {
  const res = validatePunchRequest({ method: 'App', latitude: 10.76, longitude: 106.66 });
  assert.equal(res.valid, true);
});

test('validatePunchRequest - empty method returns error', () => {
  const res = validatePunchRequest({ method: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('phương thức'));
});

test('validatePunchRequest - invalid method returns error', () => {
  const res = validatePunchRequest({ method: 'NFC_CARD' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không hợp lệ'));
});

test('validatePunchRequest - invalid latitude returns error', () => {
  const res = validatePunchRequest({ method: 'App', latitude: 100 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('vĩ độ'));
});

// ─── UC_HRM_111: validateDateRangeFilter ───

test('validateDateRangeFilter - valid range returns valid', () => {
  const res = validateDateRangeFilter('2026-08-01', '2026-08-31');
  assert.equal(res.valid, true);
});

test('validateDateRangeFilter - to < from returns error', () => {
  const res = validateDateRangeFilter('2026-08-31', '2026-08-01');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn hoặc bằng'));
});

test('validateDateRangeFilter - range over 366 days returns error', () => {
  const res = validateDateRangeFilter('2025-01-01', '2026-08-01');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 năm'));
});

// ─── UC_HRM_112: formatAttendanceStatus & filterDepartmentBoard ───

test('formatAttendanceStatus - maps statuses to Vietnamese labels', () => {
  assert.equal(formatAttendanceStatus('Open'), 'Đang làm việc');
  assert.equal(formatAttendanceStatus('Closed'), 'Đã hoàn thành');
  assert.equal(formatAttendanceStatus('Missing'), 'Thiếu chấm công');
});

test('filterDepartmentBoard - filters records by orgUnitId and date range', () => {
  const records: AttendanceRecordItem[] = [
    { id: '1', orgUnitId: 'org-1', workDate: '2026-08-01', status: 'Open' },
    { id: '2', orgUnitId: 'org-1', workDate: '2026-08-05', status: 'Closed' },
    { id: '3', orgUnitId: 'org-2', workDate: '2026-08-05', status: 'Closed' },
  ];

  const filtered = filterDepartmentBoard(records, 'org-1', '2026-08-01', '2026-08-03');
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].id, '1');
});
