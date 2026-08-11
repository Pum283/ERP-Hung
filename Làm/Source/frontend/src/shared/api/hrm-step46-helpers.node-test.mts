import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculatePayrollCompareDelta,
  formatHeadcountMovementSummary,
  calculateAttendanceSummary,
  formatRecruitFunnelStage,
} from './hrm-step46-helpers.ts';

// ─── UC_HRM_176: calculatePayrollCompareDelta ───

test('calculatePayrollCompareDelta - calculates percentage delta correctly', () => {
  const delta = calculatePayrollCompareDelta(120000000, 100000000, 105000000, 90000000);
  assert.equal(delta.grossDiff, 20000000);
  assert.equal(delta.grossDiffPct, 20);
  assert.equal(delta.netDiff, 15000000);
  assert.equal(delta.netDiffPct, 16.67);
});

test('calculatePayrollCompareDelta - handles zero previous pay', () => {
  const delta = calculatePayrollCompareDelta(100000000, 0, 90000000, 0);
  assert.equal(delta.grossDiff, 100000000);
  assert.equal(delta.grossDiffPct, 0);
});

// ─── UC_HRM_182: formatHeadcountMovementSummary ───

test('formatHeadcountMovementSummary - formats movement summary string', () => {
  const summary = formatHeadcountMovementSummary(5, 2);
  assert.ok(summary.includes('Tuyển mới: 5'));
  assert.ok(summary.includes('Nghỉ việc: 2'));
  assert.ok(summary.includes('+3'));
});

// ─── UC_HRM_183: calculateAttendanceSummary ───

test('calculateAttendanceSummary - aggregates work days and OT hours', () => {
  const rows = [
    { workUnits: 22, otMinutes: 120, lateMinutes: 30, lateCount: 2 },
    { workUnits: 20, otMinutes: 90, lateMinutes: 15, lateCount: 1 },
  ];
  const summary = calculateAttendanceSummary(rows);
  assert.equal(summary.totalWorkDays, 42);
  assert.equal(summary.totalOtHours, 3.5);
  assert.equal(summary.totalLateMinutes, 45);
  assert.equal(summary.totalLateCount, 3);
});

// ─── UC_HRM_184: formatRecruitFunnelStage ───

test('formatRecruitFunnelStage - returns stage icon and label', () => {
  assert.ok(formatRecruitFunnelStage('Interviewing').includes('Đang phỏng vấn'));
  assert.ok(formatRecruitFunnelStage('Hired').includes('Đã nhận việc'));
});
