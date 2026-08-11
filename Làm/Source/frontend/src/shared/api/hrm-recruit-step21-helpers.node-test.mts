import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateHireRequest,
  validateMentorAssignment,
  calculateChecklistProgress,
  validateOnboardingDocument,
} from './hrm-recruit-step21-helpers.ts';

// ─── UC_HRM_068: validateHireRequest ───

test('validateHireRequest - valid candidateId returns valid', () => {
  const res = validateHireRequest({ candidateId: 'cand-123' });
  assert.equal(res.valid, true);
});

test('validateHireRequest - empty candidateId returns error', () => {
  const res = validateHireRequest({ candidateId: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ứng viên'));
});

// ─── UC_HRM_069: validateMentorAssignment ───

test('validateMentorAssignment - valid mentor returns valid', () => {
  const res = validateMentorAssignment('emp-mentor', 'emp-newbie');
  assert.equal(res.valid, true);
});

test('validateMentorAssignment - empty mentorId returns error', () => {
  const res = validateMentorAssignment('   ', 'emp-newbie');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('hướng dẫn'));
});

test('validateMentorAssignment - self assignment returns error', () => {
  const res = validateMentorAssignment('emp-123', 'emp-123');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chính nhân viên mới'));
});

// ─── UC_HRM_070: calculateChecklistProgress ───

test('calculateChecklistProgress - 2 done out of 4 returns 50%', () => {
  const pct = calculateChecklistProgress([
    { done: true },
    { done: true },
    { done: false },
    { done: false },
  ]);
  assert.equal(pct, 50);
});

test('calculateChecklistProgress - all done returns 100%', () => {
  const pct = calculateChecklistProgress([{ done: true }, { done: true }]);
  assert.equal(pct, 100);
});

test('calculateChecklistProgress - empty items returns 0%', () => {
  const pct = calculateChecklistProgress([]);
  assert.equal(pct, 0);
});

test('calculateChecklistProgress - 1 done out of 3 rounds correctly (33%)', () => {
  const pct = calculateChecklistProgress([{ done: true }, { done: false }, { done: false }]);
  assert.equal(pct, 33);
});

// ─── UC_HRM_071: validateOnboardingDocument ───

test('validateOnboardingDocument - valid title & key returns valid', () => {
  const res = validateOnboardingDocument('Bằng Đại học', 'docs/degree.pdf');
  assert.equal(res.valid, true);
});

test('validateOnboardingDocument - empty title returns error', () => {
  const res = validateOnboardingDocument('   ', 'docs/degree.pdf');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('trống'));
});

test('validateOnboardingDocument - title over 200 chars returns error', () => {
  const res = validateOnboardingDocument('A'.repeat(201), 'docs/degree.pdf');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('200 ký tự'));
});

test('validateOnboardingDocument - empty storageKey returns error', () => {
  const res = validateOnboardingDocument('Bằng Cấp', '   ');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chưa được tải lên'));
});
