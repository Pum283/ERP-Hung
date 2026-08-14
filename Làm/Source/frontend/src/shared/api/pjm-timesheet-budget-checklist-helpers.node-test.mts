import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatTotalHours,
  formatOverrunPercent,
} from './pjm-timesheet-budget-checklist-helpers.ts';

test('formatTotalHours - formats standard and overtime hours', () => {
  assert.equal(formatTotalHours(8, 2), '8h (OT: +2h)');
});

test('formatOverrunPercent - formats overrun percentage', () => {
  assert.equal(formatOverrunPercent(6.0), '+6.0% Vượt Ngân Sách');
});
