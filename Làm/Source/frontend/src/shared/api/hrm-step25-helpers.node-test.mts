import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateShiftSwap,
  validateShiftCancel,
  filterPersonalRoster,
  type ShiftItem,
} from './hrm-step25-helpers.ts';

// ─── UC_HRM_084: validateShiftSwap ───

test('validateShiftSwap - valid different IDs returns valid', () => {
  const res = validateShiftSwap('assign-1', 'assign-2');
  assert.equal(res.valid, true);
});

test('validateShiftSwap - empty ID returns error', () => {
  const res = validateShiftSwap('assign-1', '   ');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chọn đủ 2 lịch ca'));
});

test('validateShiftSwap - same ID returns error', () => {
  const res = validateShiftSwap('assign-1', 'assign-1');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('tự đổi ca'));
});

// ─── UC_HRM_085: validateShiftCancel ───

test('validateShiftCancel - valid assignmentId returns valid', () => {
  const res = validateShiftCancel('assign-100');
  assert.equal(res.valid, true);
});

test('validateShiftCancel - empty assignmentId returns error', () => {
  const res = validateShiftCancel('   ');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chọn lịch ca'));
});

// ─── UC_HRM_086 & UC_HRM_087: filterPersonalRoster ───

test('filterPersonalRoster - excludes cancelled by default', () => {
  const items: ShiftItem[] = [
    { id: '1', workDate: '2026-08-11', status: 'Scheduled' },
    { id: '2', workDate: '2026-08-12', status: 'Cancelled' },
  ];
  const filtered = filterPersonalRoster(items);
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].id, '1');
});

test('filterPersonalRoster - includes cancelled when flag is true', () => {
  const items: ShiftItem[] = [
    { id: '1', workDate: '2026-08-11', status: 'Scheduled' },
    { id: '2', workDate: '2026-08-12', status: 'Cancelled' },
  ];
  const filtered = filterPersonalRoster(items, undefined, undefined, true);
  assert.equal(filtered.length, 2);
});

test('filterPersonalRoster - filters by date range correctly', () => {
  const items: ShiftItem[] = [
    { id: '1', workDate: '2026-08-01', status: 'Scheduled' },
    { id: '2', workDate: '2026-08-11', status: 'Scheduled' },
    { id: '3', workDate: '2026-08-20', status: 'Scheduled' },
  ];
  const filtered = filterPersonalRoster(items, '2026-08-05', '2026-08-15');
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].id, '2');
});
