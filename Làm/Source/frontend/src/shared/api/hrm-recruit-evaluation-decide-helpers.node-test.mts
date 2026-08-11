import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateAssignEvalOrgForm,
  validateEvaluationForm,
  validateCandidateDecisionForm,
  isValidPipelineTransition,
} from './hrm-recruit-evaluation-decide-helpers.ts';

// ─── UC_HRM_060: validateAssignEvalOrgForm ───

test('validateAssignEvalOrgForm - valid org unit ID returns valid', () => {
  const res = validateAssignEvalOrgForm({ evalOrgUnitId: 'ORG_123' });
  assert.equal(res.valid, true);
});

test('validateAssignEvalOrgForm - empty org unit ID returns error', () => {
  const res = validateAssignEvalOrgForm({ evalOrgUnitId: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('đơn vị đánh giá'));
});

// ─── UC_HRM_061: validateEvaluationForm ───

test('validateEvaluationForm - valid evaluation returns valid', () => {
  const res = validateEvaluationForm({
    evalScore: 85,
    evalResult: 'Pass',
    evalComment: 'Ứng viên tốt',
  });
  assert.equal(res.valid, true);
});

test('validateEvaluationForm - negative score returns error', () => {
  const res = validateEvaluationForm({
    evalScore: -10,
    evalResult: 'Pass',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 100'));
});

test('validateEvaluationForm - score over 100 returns error', () => {
  const res = validateEvaluationForm({
    evalScore: 101,
    evalResult: 'Pass',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 100'));
});

test('validateEvaluationForm - invalid evalResult returns error', () => {
  const res = validateEvaluationForm({
    evalScore: 90,
    evalResult: 'Excellent',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('kết quả đánh giá'));
});

test('validateEvaluationForm - comment too long returns error', () => {
  const res = validateEvaluationForm({
    evalScore: 80,
    evalResult: 'Pass',
    evalComment: 'A'.repeat(1001),
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1000 ký tự'));
});

// ─── UC_HRM_062: validateCandidateDecisionForm ───

test('validateCandidateDecisionForm - valid Accept with note returns valid', () => {
  const res = validateCandidateDecisionForm({
    action: 'Accept',
    decisionNote: 'Offer Gross 30M/tháng',
  });
  assert.equal(res.valid, true);
});

test('validateCandidateDecisionForm - valid Reject with note returns valid', () => {
  const res = validateCandidateDecisionForm({
    action: 'Reject',
    decisionNote: 'Kinh nghiệm chưa đáp ứng vị trí Senior',
  });
  assert.equal(res.valid, true);
});

test('validateCandidateDecisionForm - Accept without note returns error', () => {
  const res = validateCandidateDecisionForm({
    action: 'Accept',
    decisionNote: '   ',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('thư mời làm việc'));
});

test('validateCandidateDecisionForm - Reject without note returns error', () => {
  const res = validateCandidateDecisionForm({
    action: 'Reject',
    decisionNote: '',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('từ chối'));
});

// ─── UC_HRM_063: isValidPipelineTransition ───

test('isValidPipelineTransition - New to Screening is valid', () => {
  assert.equal(isValidPipelineTransition('New', 'Screening'), true);
});

test('isValidPipelineTransition - New to Accepted is invalid', () => {
  assert.equal(isValidPipelineTransition('New', 'Accepted'), false);
});

test('isValidPipelineTransition - Evaluating to Accepted is valid', () => {
  assert.equal(isValidPipelineTransition('Evaluating', 'Accepted'), true);
});

test('isValidPipelineTransition - Accepted to Screening is invalid', () => {
  assert.equal(isValidPipelineTransition('Accepted', 'Screening'), false);
});
