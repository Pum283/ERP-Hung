import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateRewardDisciplineInput,
  validateDecisionAttach,
  filterRewardDisciplineHistory,
  formatRewardDisciplineStatus,
  type DecisionHistoryEntry,
} from './hrm-step37-helpers.ts';

// ─── UC_HRM_139 & 140: validateRewardDisciplineInput ───

test('validateRewardDisciplineInput - valid reward input returns valid', () => {
  const res = validateRewardDisciplineInput({
    employeeId: 'emp-1',
    kind: 'Reward',
    title: 'Khen thưởng sáng kiến',
    decisionDate: '2026-08-01',
    payrollImpactAmount: 1000000,
    payrollImpactKind: 'Bonus',
  });
  assert.equal(res.valid, true);
});

test('validateRewardDisciplineInput - empty title returns error', () => {
  const res = validateRewardDisciplineInput({
    employeeId: 'emp-1',
    kind: 'Reward',
    title: '',
    decisionDate: '2026-08-01',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('tiêu đề'));
});

test('validateRewardDisciplineInput - invalid kind returns error', () => {
  const res = validateRewardDisciplineInput({
    employeeId: 'emp-1',
    kind: 'Other',
    title: 'Tiêu đề',
    decisionDate: '2026-08-01',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Khen thưởng'));
});

// ─── UC_HRM_143: validateDecisionAttach & filterRewardDisciplineHistory ───

test('validateDecisionAttach - valid storageKey returns valid', () => {
  const res = validateDecisionAttach('docs/decisions/dec-123.pdf');
  assert.equal(res.valid, true);
});

test('validateDecisionAttach - empty storageKey returns error', () => {
  const res = validateDecisionAttach('  ');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('storageKey'));
});

test('filterRewardDisciplineHistory - filters decisions by kind and employeeId', () => {
  const items: DecisionHistoryEntry[] = [
    { id: '1', employeeId: 'emp-1', employeeName: 'A', kind: 'Reward', title: 'Khen thưởng 1', decisionDate: '2026-08-01', status: 'Issued' },
    { id: '2', employeeId: 'emp-2', employeeName: 'B', kind: 'Discipline', title: 'Kỷ luật 2', decisionDate: '2026-08-01', status: 'Issued' },
  ];

  const filtered = filterRewardDisciplineHistory(items, 'Reward', 'emp-1');
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].title, 'Khen thưởng 1');
});

// ─── UC_HRM_141: formatRewardDisciplineStatus ───

test('formatRewardDisciplineStatus - returns correct status label', () => {
  assert.ok(formatRewardDisciplineStatus('Issued').includes('ban hành'));
  assert.ok(formatRewardDisciplineStatus('Applied').includes('áp dụng'));
});
