import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateAiMatchScore,
  formatAiSummaryBullets,
  validateAiQuizStructure,
} from './lms-ai-assist-helpers.ts';

test('calculateAiMatchScore - calculates score based on skill match', () => {
  const userSkills = ['C#', 'SQL', 'DDD'];
  const reqSkills = ['C#', 'DDD', 'Docker'];
  const score = calculateAiMatchScore(userSkills, reqSkills);
  assert.equal(score, 67); // 2/3 matched -> 67%
});

test('formatAiSummaryBullets - splits summary text into bullets', () => {
  const rawText = 'Khái niệm Clean Architecture cốt lõi. Tối ưu hiệu năng truy vấn database SQL. Phân quyền bảo mật theo vai trò.';
  const bullets = formatAiSummaryBullets(rawText);
  assert.equal(bullets.length, 3);
});

test('validateAiQuizStructure - valid quiz questions', () => {
  const questions = [
    { questionText: 'DDD là gì?', options: ['Domain-Driven Design', 'Data Driven Base'], correctOptionIndex: 0 },
  ];
  const res = validateAiQuizStructure(questions);
  assert.equal(res.isValid, true);
});

test('validateAiQuizStructure - invalid option index', () => {
  const questions = [
    { questionText: 'DDD là gì?', options: ['Option A', 'Option B'], correctOptionIndex: 5 },
  ];
  const res = validateAiQuizStructure(questions);
  assert.equal(res.isValid, false);
  assert.equal(res.errorMessage, 'Câu hỏi số 1 có đáp án đúng không hợp lệ.');
});
