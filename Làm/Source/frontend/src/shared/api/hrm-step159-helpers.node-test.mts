import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateFinalKpiGrade,
  validateTemplateWeights,
  validateSelfEvaluationScore,
  calculateCycleCompletionStats,
  ManagerEvaluationItem,
} from './hrm-step159-helpers.js';

test('calculateFinalKpiGrade - score >= 85 assigns Grade A', () => {
  const res = calculateFinalKpiGrade(90, 85);
  assert.equal(res.finalScore, 87.5);
  assert.equal(res.grade, 'A');
});

test('calculateFinalKpiGrade - score between 70 and 84 assigns Grade B', () => {
  const res = calculateFinalKpiGrade(75, 70);
  assert.equal(res.finalScore, 72.5);
  assert.equal(res.grade, 'B');
});

test('calculateFinalKpiGrade - score between 50 and 69 assigns Grade C', () => {
  const res = calculateFinalKpiGrade(60, 50);
  assert.equal(res.finalScore, 55);
  assert.equal(res.grade, 'C');
});

test('calculateFinalKpiGrade - score < 50 assigns Grade D', () => {
  const res = calculateFinalKpiGrade(40, 30);
  assert.equal(res.finalScore, 35);
  assert.equal(res.grade, 'D');
});

test('validateTemplateWeights - valid maxScore and weightPercentage pass', () => {
  const res = validateTemplateWeights(100, 50);
  assert.equal(res.isValid, true);
});

test('validateTemplateWeights - invalid maxScore <= 0 fails', () => {
  const res = validateTemplateWeights(0, 50);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Điểm tối đa phải lớn hơn 0.');
});

test('validateTemplateWeights - invalid weightPercentage > 100 fails', () => {
  const res = validateTemplateWeights(100, 150);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Tỷ trọng % phải nằm trong khoảng (0, 100].');
});

test('validateSelfEvaluationScore - rating between 1 and 5 is valid', () => {
  const res = validateSelfEvaluationScore(4);
  assert.equal(res.isValid, true);
  assert.equal(res.clampedRating, 4);
});

test('validateSelfEvaluationScore - rating out of bounds is clamped', () => {
  const res = validateSelfEvaluationScore(7);
  assert.equal(res.isValid, false);
  assert.equal(res.clampedRating, 5);
});

test('calculateCycleCompletionStats - calculates completed rate correctly', () => {
  const items: ManagerEvaluationItem[] = [
    { id: '1', employeeId: 'emp1', kpiScore: 90, competencyScore: 85, finalGrade: 'A', status: 'Completed' },
    { id: '2', employeeId: 'emp2', kpiScore: 80, competencyScore: 75, finalGrade: 'B', status: 'Completed' },
    { id: '3', employeeId: 'emp3', kpiScore: 0, competencyScore: 0, finalGrade: 'D', status: 'Pending' },
  ];

  const stats = calculateCycleCompletionStats(items);
  assert.equal(stats.total, 3);
  assert.equal(stats.completed, 2);
  assert.equal(stats.pending, 1);
  assert.equal(stats.completionRate, 66.7);
});
