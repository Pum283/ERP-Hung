import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLeaveCancelRequest,
  filterLeaveCalendar,
  validateHolidayInput,
  validateHolidayImportBatch,
  formatHolidayStatus,
  type LeaveCalendarEntry,
  type HolidayInput,
} from './hrm-step36-helpers.ts';

// ─── UC_HRM_134: validateLeaveCancelRequest ───

test('validateLeaveCancelRequest - draft status returns valid', () => {
  const res = validateLeaveCancelRequest('req-1', 'Draft');
  assert.equal(res.valid, true);
});

test('validateLeaveCancelRequest - cancelled status returns error', () => {
  const res = validateLeaveCancelRequest('req-1', 'Cancelled');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('hủy hoặc bị từ chối'));
});

// ─── UC_HRM_136: filterLeaveCalendar ───

test('filterLeaveCalendar - filters items by orgUnitId and status', () => {
  const items: LeaveCalendarEntry[] = [
    { requestId: '1', employeeId: 'e1', employeeName: 'A', orgUnitId: 'ou-1', fromDate: '2026-09-01', toDate: '2026-09-02', status: 'Approved' },
    { requestId: '2', employeeId: 'e2', employeeName: 'B', orgUnitId: 'ou-2', fromDate: '2026-09-01', toDate: '2026-09-02', status: 'Pending' },
  ];

  const filtered = filterLeaveCalendar(items, 'ou-1', 'Approved');
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].employeeName, 'A');
});

// ─── UC_HRM_137: validateHolidayInput & validateHolidayImportBatch ───

test('validateHolidayInput - valid input returns valid', () => {
  const res = validateHolidayInput({ date: '2026-09-02', name: 'Quốc Khánh', isPaid: true });
  assert.equal(res.valid, true);
});

test('validateHolidayInput - empty name returns error', () => {
  const res = validateHolidayInput({ date: '2026-09-02', name: '', isPaid: true });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 200'));
});

test('validateHolidayImportBatch - valid batch returns valid', () => {
  const batch: HolidayInput[] = [
    { date: '2026-01-01', name: 'Tết Dương Lịch', isPaid: true },
    { date: '2026-04-30', name: '30 Tháng 4', isPaid: true },
  ];
  const res = validateHolidayImportBatch(batch);
  assert.equal(res.valid, true);
});

test('validateHolidayImportBatch - empty batch returns error', () => {
  const res = validateHolidayImportBatch([]);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không được để trống'));
});

// ─── UC_HRM_138: formatHolidayStatus ───

test('formatHolidayStatus - formats paid/unpaid status correctly', () => {
  assert.ok(formatHolidayStatus(true).includes('nguyên lương'));
  assert.ok(formatHolidayStatus(false).includes('không hưởng lương'));
});
