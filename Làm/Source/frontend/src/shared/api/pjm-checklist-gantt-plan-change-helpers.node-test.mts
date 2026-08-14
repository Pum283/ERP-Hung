import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatProgressPercent,
  formatGanttStatusBadge,
} from './pjm-checklist-gantt-plan-change-helpers.ts';

test('formatProgressPercent - formats progress percentage', () => {
  assert.equal(formatProgressPercent(75.0), '75% Hoàn Thành');
});

test('formatGanttStatusBadge - returns styling class', () => {
  assert.equal(formatGanttStatusBadge('Completed'), 'bg-emerald-100 text-emerald-800 border-emerald-300');
  assert.equal(formatGanttStatusBadge('InProgress'), 'bg-blue-100 text-blue-800 border-blue-300');
});
