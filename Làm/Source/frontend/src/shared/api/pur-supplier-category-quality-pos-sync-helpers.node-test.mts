import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateWeightedQualityScore,
  validateOrderMoqCompliance,
} from './pur-supplier-category-quality-pos-sync-helpers.ts';

test('calculateWeightedQualityScore - calculates overall score and letter grade', () => {
  const s1 = calculateWeightedQualityScore(95, 90, 88);
  assert.equal(s1.overallScore, 91);
  assert.equal(s1.grade, 'A');

  const s2 = calculateWeightedQualityScore(70, 65, 75);
  assert.equal(s2.overallScore, 70);
  assert.equal(s2.grade, 'C');
});

test('validateOrderMoqCompliance - checks minimum order quantity compliance', () => {
  assert.equal(validateOrderMoqCompliance(120, 100).isCompliant, true);
  const v = validateOrderMoqCompliance(80, 100);
  assert.equal(v.isCompliant, false);
  assert.equal(v.deficit, 20);
});
