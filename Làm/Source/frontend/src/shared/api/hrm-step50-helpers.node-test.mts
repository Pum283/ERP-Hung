import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLmsClassSessionInput,
  formatLmsAttendanceStatus,
  calculateClassAttendanceRate,
  formatLmsClassStatus,
} from './hrm-step50-helpers.ts';

// ─── UC_LMS_017: validateLmsClassSessionInput ───

test('validateLmsClassSessionInput - valid session returns valid', () => {
  const res = validateLmsClassSessionInput({
    topic: 'Buổi 1: Giới thiệu',
    sessionDate: '2026-09-10',
    startTime: '08:00',
    endTime: '11:30',
  });
  assert.equal(res.valid, true);
});

test('validateLmsClassSessionInput - end time before start time returns error', () => {
  const res = validateLmsClassSessionInput({
    topic: 'Buổi 1',
    sessionDate: '2026-09-10',
    startTime: '11:00',
    endTime: '08:00',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Giờ kết thúc phải sau'));
});

// ─── UC_LMS_019: formatLmsAttendanceStatus ───

test('formatLmsAttendanceStatus - returns label with icon', () => {
  assert.ok(formatLmsAttendanceStatus('Present').includes('Có mặt'));
  assert.ok(formatLmsAttendanceStatus('Absent').includes('Vắng mặt'));
  assert.ok(formatLmsAttendanceStatus('Late').includes('Đi trễ'));
});

// ─── UC_LMS_018: calculateClassAttendanceRate ───

test('calculateClassAttendanceRate - calculates rate correctly', () => {
  const res = calculateClassAttendanceRate(30, 27);
  assert.equal(res.attendanceRatePct, 90);
  assert.equal(res.isGood, true);
});

// ─── UC_LMS_022: formatLmsClassStatus ───

test('formatLmsClassStatus - returns class status label', () => {
  assert.ok(formatLmsClassStatus('Open').includes('Đang mở ghi danh'));
  assert.ok(formatLmsClassStatus('Closed').includes('Đã kết thúc'));
});
