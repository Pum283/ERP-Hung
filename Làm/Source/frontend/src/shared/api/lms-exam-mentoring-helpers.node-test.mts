import test from 'node:test';
import assert from 'node:assert/strict';
import {
  checkAntiCheatViolation,
  calculateMentoringProgress,
  validateRatingScore,
  summarizeMentoringEffectiveness
} from './lms-exam-mentoring-helpers.ts';
import type {
  ChecklistItem
} from './lms-exam-mentoring-helpers.ts';

test('checkAntiCheatViolation - no violation when within limits', () => {
  const res = checkAntiCheatViolation(0, 0, 1800);
  assert.equal(res.isViolated, false);
  assert.equal(res.shouldForceSubmit, false);
});

test('checkAntiCheatViolation - time expired forces submit', () => {
  const res = checkAntiCheatViolation(0, 0, 0);
  assert.equal(res.isViolated, true);
  assert.equal(res.shouldForceSubmit, true);
  assert.equal(res.reason, 'Hết thời gian làm bài.');
});

test('checkAntiCheatViolation - tab switch threshold forces submit', () => {
  const res = checkAntiCheatViolation(1, 3, 1000);
  assert.equal(res.isViolated, true);
  assert.equal(res.shouldForceSubmit, true);
});

test('checkAntiCheatViolation - single focus loss warns without submit', () => {
  const res = checkAntiCheatViolation(1, 0, 1000);
  assert.equal(res.isViolated, true);
  assert.equal(res.shouldForceSubmit, false);
});

test('calculateMentoringProgress - empty tasks returns zero', () => {
  const res = calculateMentoringProgress([]);
  assert.equal(res.total, 0);
  assert.equal(res.percentage, 0);
});

test('calculateMentoringProgress - correctly calculates percentage', () => {
  const tasks: ChecklistItem[] = [
    { id: 't1', taskName: 'Task 1', isCompleted: true },
    { id: 't2', taskName: 'Task 2', isCompleted: true },
    { id: 't3', taskName: 'Task 3', isCompleted: false },
    { id: 't4', taskName: 'Task 4', isCompleted: false },
  ];
  const res = calculateMentoringProgress(tasks);
  assert.equal(res.total, 4);
  assert.equal(res.completed, 2);
  assert.equal(res.percentage, 50);
});

test('validateRatingScore - valid rating 1-5 passes', () => {
  const res = validateRatingScore(4);
  assert.equal(res.isValid, true);
  assert.equal(res.normalizedRating, 4);
});

test('validateRatingScore - out of range rating gets clamped', () => {
  const res = validateRatingScore(10);
  assert.equal(res.isValid, false);
  assert.equal(res.normalizedRating, 5);
});

test('summarizeMentoringEffectiveness - empty ratings returns zero averages', () => {
  const res = summarizeMentoringEffectiveness(5, 0, 0, [], []);
  assert.equal(res.completionRatePct, 0);
  assert.equal(res.avgMentorRating, 0);
});

test('summarizeMentoringEffectiveness - correctly averages mentor and mentee ratings', () => {
  const res = summarizeMentoringEffectiveness(5, 8, 10, [5, 4, 5], [4, 4, 4]);
  assert.equal(res.completionRatePct, 80);
  assert.equal(res.avgMentorRating, 4.67);
  assert.equal(res.avgMenteeRating, 4);
});
