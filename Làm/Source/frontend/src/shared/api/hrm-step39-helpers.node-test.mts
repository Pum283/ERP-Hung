import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateHandoverProgress,
  validateFinalSettlementInput,
  validateExitInterviewNotes,
  formatOffboardingStatus,
  type HandoverChecklistItem,
} from './hrm-step39-helpers.ts';

// ─── UC_HRM_147: calculateHandoverProgress ───

test('calculateHandoverProgress - calculates correct percentage and count', () => {
  const items: HandoverChecklistItem[] = [
    { key: 'assets', label: 'Tài sản', done: true },
    { key: 'docs', label: 'Tài liệu', done: true },
    { key: 'access', label: 'Quyền', done: false },
    { key: 'finance', label: 'Tạm ứng', done: false },
  ];

  const res = calculateHandoverProgress(items);
  assert.equal(res.completed, 2);
  assert.equal(res.total, 4);
  assert.equal(res.percentage, 50);
});

// ─── UC_HRM_149: validateFinalSettlementInput ───

test('validateFinalSettlementInput - valid settlement returns valid', () => {
  const res = validateFinalSettlementInput({
    leaveDaysRemaining: 5,
    leaveSettlementAmount: 2000000,
    finalPayEstimate: 12000000,
  });
  assert.equal(res.valid, true);
});

test('validateFinalSettlementInput - negative amount returns error', () => {
  const res = validateFinalSettlementInput({
    leaveDaysRemaining: 5,
    leaveSettlementAmount: -500000,
    finalPayEstimate: 12000000,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không được âm'));
});

// ─── UC_HRM_150: validateExitInterviewNotes ───

test('validateExitInterviewNotes - valid notes returns valid', () => {
  const res = validateExitInterviewNotes('Nhân viên phỏng vấn có nguyện vọng nghỉ việc cá nhân.');
  assert.equal(res.valid, true);
});

test('validateExitInterviewNotes - too short notes returns error', () => {
  const res = validateExitInterviewNotes('OK');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('5 đến 1000'));
});

// ─── UC_HRM_150: formatOffboardingStatus ───

test('formatOffboardingStatus - returns correct status label', () => {
  assert.ok(formatOffboardingStatus('InProgress').includes('bàn giao'));
  assert.ok(formatOffboardingStatus('Completed').includes('hoàn tất'));
});
