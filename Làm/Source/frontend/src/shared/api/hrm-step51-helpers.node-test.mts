import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLmsMentorAssignment,
  formatMentorAssignmentSummary,
  validateLearnerRegistration,
  validateLearnerLogin,
  formatCourseLessonProgressSummary,
} from './hrm-step51-helpers.ts';

// ─── UC_LMS_023: validateLmsMentorAssignment & formatMentorAssignmentSummary ───

test('validateLmsMentorAssignment - valid mentor assignment returns valid', () => {
  const res = validateLmsMentorAssignment('mentee-1', 'mentor-2');
  assert.equal(res.valid, true);
});

test('validateLmsMentorAssignment - same mentee and mentor returns error', () => {
  const res = validateLmsMentorAssignment('emp-1', 'emp-1');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không được là cùng một người'));
});

test('formatMentorAssignmentSummary - formats summary string correctly', () => {
  const text = formatMentorAssignmentSummary('Nguyễn Văn A', 'Trần Văn B');
  assert.ok(text.includes('Mentor: Trần Văn B'));
  assert.ok(text.includes('Nguyễn Văn A'));
});

// ─── UC_LMS_028: validateLearnerRegistration ───

test('validateLearnerRegistration - valid registration input returns valid', () => {
  const res = validateLearnerRegistration({
    username: 'learner01',
    email: 'learner@erp.vn',
    password: 'password123',
    confirmPassword: 'password123',
  });
  assert.equal(res.valid, true);
});

test('validateLearnerRegistration - mismatched passwords returns error', () => {
  const res = validateLearnerRegistration({
    username: 'learner01',
    email: 'learner@erp.vn',
    password: 'password123',
    confirmPassword: 'wrongpassword',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('trùng khớp'));
});

// ─── UC_LMS_029 & 030: validateLearnerLogin & formatCourseLessonProgressSummary ───

test('validateLearnerLogin - empty password returns error', () => {
  const res = validateLearnerLogin('learner01', '');
  assert.equal(res.valid, false);
});

test('formatCourseLessonProgressSummary - formats chapters and lessons text', () => {
  const text = formatCourseLessonProgressSummary(5, 20);
  assert.ok(text.includes('5 chương'));
  assert.ok(text.includes('20 bài học'));
});
