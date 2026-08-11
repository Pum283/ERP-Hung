import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateWorkShiftTemplate,
  validateSingleShiftAssign,
  validateShiftAssignRange,
} from './hrm-step24-helpers.ts';

// ─── UC_HRM_081: validateWorkShiftTemplate ───

test('validateWorkShiftTemplate - valid shift returns valid', () => {
  const res = validateWorkShiftTemplate({ code: 'SH_HC8', name: 'Ca Hành Chính 8h', breakMinutes: 60 });
  assert.equal(res.valid, true);
});

test('validateWorkShiftTemplate - empty code returns error', () => {
  const res = validateWorkShiftTemplate({ code: '   ', name: 'Ca Hành Chính 8h', breakMinutes: 60 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Mã ca'));
});

test('validateWorkShiftTemplate - code over 40 chars returns error', () => {
  const res = validateWorkShiftTemplate({ code: 'C'.repeat(41), name: 'Ca Hành Chính 8h', breakMinutes: 60 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('40 ký tự'));
});

test('validateWorkShiftTemplate - empty name returns error', () => {
  const res = validateWorkShiftTemplate({ code: 'SH_HC8', name: '   ', breakMinutes: 60 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Tên ca'));
});

test('validateWorkShiftTemplate - invalid breakMinutes returns error', () => {
  const res = validateWorkShiftTemplate({ code: 'SH_HC8', name: 'Ca HC', breakMinutes: 700 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('600 phút'));
});

// ─── UC_HRM_082: validateSingleShiftAssign ───

test('validateSingleShiftAssign - valid assignment returns valid', () => {
  const res = validateSingleShiftAssign({ employeeId: 'emp-1', workShiftId: 'shift-1', workDate: '2026-08-15' });
  assert.equal(res.valid, true);
});

test('validateSingleShiftAssign - empty employeeId returns error', () => {
  const res = validateSingleShiftAssign({ employeeId: '   ', workShiftId: 'shift-1', workDate: '2026-08-15' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('nhân viên'));
});

test('validateSingleShiftAssign - empty workShiftId returns error', () => {
  const res = validateSingleShiftAssign({ employeeId: 'emp-1', workShiftId: '   ', workDate: '2026-08-15' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ca làm việc'));
});

test('validateSingleShiftAssign - empty workDate returns error', () => {
  const res = validateSingleShiftAssign({ employeeId: 'emp-1', workShiftId: 'shift-1', workDate: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ngày làm việc'));
});

// ─── UC_HRM_083: validateShiftAssignRange ───

test('validateShiftAssignRange - valid 7 days range returns valid', () => {
  const res = validateShiftAssignRange({
    employeeIds: ['emp-1', 'emp-2'],
    workShiftId: 'shift-1',
    from: '2026-08-01',
    to: '2026-08-07',
  });
  assert.equal(res.valid, true);
});

test('validateShiftAssignRange - empty employeeIds returns error', () => {
  const res = validateShiftAssignRange({
    employeeIds: [],
    workShiftId: 'shift-1',
    from: '2026-08-01',
    to: '2026-08-07',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ít nhất một nhân viên'));
});

test('validateShiftAssignRange - to < from returns error', () => {
  const res = validateShiftAssignRange({
    employeeIds: ['emp-1'],
    workShiftId: 'shift-1',
    from: '2026-08-10',
    to: '2026-08-01',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn hoặc bằng'));
});

test('validateShiftAssignRange - range over 62 days returns error', () => {
  const res = validateShiftAssignRange({
    employeeIds: ['emp-1'],
    workShiftId: 'shift-1',
    from: '2026-08-01',
    to: '2026-10-15', // 75 days
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('62 ngày'));
});
