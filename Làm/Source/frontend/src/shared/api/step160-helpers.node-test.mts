import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateGradeDistribution,
  validateCourseTag,
  parseSemanticVersion,
  generateRandomExamQuestions,
  QuestionBankItem,
} from './step160-helpers.js';

test('calculateGradeDistribution - empty array returns zero distribution', () => {
  const res = calculateGradeDistribution([]);
  assert.equal(res.total, 0);
  assert.equal(res.distributions.A.count, 0);
});

test('calculateGradeDistribution - correctly calculates grade percentages', () => {
  const grades = ['A', 'A', 'B', 'C'];
  const res = calculateGradeDistribution(grades);
  assert.equal(res.total, 4);
  assert.equal(res.distributions.A.count, 2);
  assert.equal(res.distributions.A.percentage, 50);
  assert.equal(res.distributions.B.count, 1);
  assert.equal(res.distributions.B.percentage, 25);
});

test('validateCourseTag - valid tag passes', () => {
  const res = validateCourseTag('ReactJS', 'skill');
  assert.equal(res.isValid, true);
  assert.equal(res.normalizedType, 'Skill');
});

test('validateCourseTag - missing tagName fails', () => {
  const res = validateCourseTag('   ', 'Skill');
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Tên tag không được để trống.');
});

test('validateCourseTag - invalid tagType defaults to Skill', () => {
  const res = validateCourseTag('CustomTag', 'UnknownType');
  assert.equal(res.isValid, true);
  assert.equal(res.normalizedType, 'Skill');
});

test('parseSemanticVersion - valid version passes', () => {
  const res = parseSemanticVersion('2.1.0');
  assert.equal(res.isValid, true);
  assert.equal(res.normalized, '2.1.0');
});

test('parseSemanticVersion - invalid version falls back to 1.0', () => {
  const res = parseSemanticVersion('invalid-ver');
  assert.equal(res.isValid, false);
  assert.equal(res.normalized, '1.0');
});

test('generateRandomExamQuestions - empty pool returns empty selected', () => {
  const res = generateRandomExamQuestions([], 5);
  assert.equal(res.count, 0);
  assert.equal(res.selected.length, 0);
});

test('generateRandomExamQuestions - requested count larger than pool returns full pool', () => {
  const pool: QuestionBankItem[] = [
    { id: 'q1', content: 'Câu 1' },
    { id: 'q2', content: 'Câu 2' },
  ];
  const res = generateRandomExamQuestions(pool, 10);
  assert.equal(res.count, 2);
});

test('generateRandomExamQuestions - selects requested count correctly', () => {
  const pool: QuestionBankItem[] = [
    { id: 'q1', content: 'Câu 1' },
    { id: 'q2', content: 'Câu 2' },
    { id: 'q3', content: 'Câu 3' },
    { id: 'q4', content: 'Câu 4' },
  ];
  const res = generateRandomExamQuestions(pool, 2);
  assert.equal(res.count, 2);
});
