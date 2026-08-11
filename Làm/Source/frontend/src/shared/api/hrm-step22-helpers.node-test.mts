import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateTrialEvaluation,
  validateConvertOfficial,
  validateHeadcountPlan,
} from './hrm-step22-helpers.ts';

// ─── UC_HRM_072: validateTrialEvaluation ───

test('validateTrialEvaluation - valid score 85 returns valid', () => {
  const res = validateTrialEvaluation(85, 'Đạt thử việc xuất sắc');
  assert.equal(res.valid, true);
});

test('validateTrialEvaluation - score below 0 returns error', () => {
  const res = validateTrialEvaluation(-5);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 100'));
});

test('validateTrialEvaluation - score above 100 returns error', () => {
  const res = validateTrialEvaluation(105);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 100'));
});

test('validateTrialEvaluation - comment over 1000 chars returns error', () => {
  const res = validateTrialEvaluation(80, 'C'.repeat(1001));
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1000 ký tự'));
});

// ─── UC_HRM_073: validateConvertOfficial ───

test('validateConvertOfficial - valid trialScore returns valid', () => {
  const res = validateConvertOfficial(75);
  assert.equal(res.valid, true);
});

test('validateConvertOfficial - null trialScore returns error', () => {
  const res = validateConvertOfficial(null);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Cần đánh giá thử việc'));
});

test('validateConvertOfficial - undefined trialScore returns error', () => {
  const res = validateConvertOfficial(undefined);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Cần đánh giá thử việc'));
});

// ─── UC_HRM_075: validateHeadcountPlan ───

test('validateHeadcountPlan - valid OrgUnit plan returns valid', () => {
  const res = validateHeadcountPlan({ scopeType: 'OrgUnit', orgUnitId: 'org-123', plannedHeadcount: 20 });
  assert.equal(res.valid, true);
});

test('validateHeadcountPlan - invalid scopeType returns error', () => {
  const res = validateHeadcountPlan({ scopeType: 'InvalidScope', orgUnitId: 'org-123', plannedHeadcount: 20 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Loại định biên'));
});

test('validateHeadcountPlan - empty orgUnitId returns error', () => {
  const res = validateHeadcountPlan({ scopeType: 'OrgUnit', orgUnitId: '   ', plannedHeadcount: 20 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Đơn vị'));
});

test('validateHeadcountPlan - negative plannedHeadcount returns error', () => {
  const res = validateHeadcountPlan({ scopeType: 'OrgUnit', orgUnitId: 'org-123', plannedHeadcount: -1 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không âm'));
});
