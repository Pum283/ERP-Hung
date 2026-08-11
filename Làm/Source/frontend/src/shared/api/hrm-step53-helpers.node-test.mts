import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatLessonCompletionStatus,
  formatResumeLessonText,
  calculateCourseCompletionPercentage,
  validateChapterQuizSubmission,
  formatQuizResultSummary,
} from './hrm-step53-helpers.ts';

// ─── UC_LMS_035: formatLessonCompletionStatus ───

test('formatLessonCompletionStatus - returns status text with icon', () => {
  assert.ok(formatLessonCompletionStatus(true).includes('Đã hoàn thành'));
  assert.ok(formatLessonCompletionStatus(false).includes('Chưa hoàn thành'));
});

// ─── UC_LMS_036: formatResumeLessonText ───

test('formatResumeLessonText - formats resume button label correctly', () => {
  const text1 = formatResumeLessonText('Bài 2: Thực Hành', 'Chương 1');
  assert.ok(text1.includes('Tiếp tục học: Chương 1 — Bài 2: Thực Hành'));

  const text2 = formatResumeLessonText();
  assert.ok(text2.includes('Bắt đầu học bài đầu tiên'));
});

// ─── UC_LMS_037: calculateCourseCompletionPercentage ───

test('calculateCourseCompletionPercentage - calculates completion percentage correctly', () => {
  const res = calculateCourseCompletionPercentage(3, 4);
  assert.equal(res.completionPct, 75);
  assert.equal(res.isFullyCompleted, false);
});

// ─── UC_LMS_040: validateChapterQuizSubmission & formatQuizResultSummary ───

test('validateChapterQuizSubmission - detects unanswered questions', () => {
  const answers = [{ questionId: 'q1', selectedAnswers: ['A'] }];
  const res = validateChapterQuizSubmission(answers, 3);
  assert.equal(res.valid, true);
  assert.equal(res.answeredCount, 1);
  assert.equal(res.unAnsweredCount, 2);
  assert.ok(res.warningMsg?.includes('2 câu chưa trả lời'));
});

test('formatQuizResultSummary - formats score and pass status', () => {
  const text = formatQuizResultSummary(90, 100, 80, true);
  assert.ok(text.includes('ĐẠT'));
  assert.ok(text.includes('90/100 điểm'));
  assert.ok(text.includes('80%'));
});
