import test from 'node:test';
import assert from 'node:assert/strict';
import {
  getTripStatusBadge,
  formatShiftTiming,
} from './log-shift-trip-pod-reschedule-helpers.ts';

test('getTripStatusBadge - returns matching badge for trip statuses', () => {
  const inTransit = getTripStatusBadge('InTransit');
  assert.equal(inTransit.label, 'Đang Giao Hàng');
  assert.match(inTransit.colorClass, /bg-blue/);

  const completed = getTripStatusBadge('Completed');
  assert.equal(completed.label, 'Đã Hoàn Tất Chuyến');
  assert.match(completed.colorClass, /bg-emerald/);
});

test('formatShiftTiming - formats start and end time', () => {
  assert.equal(formatShiftTiming('08:00', '12:00'), '08:00 - 12:00');
});
