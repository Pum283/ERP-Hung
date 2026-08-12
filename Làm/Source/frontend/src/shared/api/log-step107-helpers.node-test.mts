import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateCodHandoverCreate,
  calculateHandoverVariance,
  validateVarianceResolutionNote,
  validateReturnOrderCreate,
} from './log-step107-helpers.ts';

test('UC_LOG_023: validateCodHandoverCreate', () => {
  const valid = validateCodHandoverCreate(['ID-01', 'ID-02']);
  assert.equal(valid.canCreate, true);

  const empty = validateCodHandoverCreate([]);
  assert.equal(empty.canCreate, false);
});

test('UC_LOG_024: calculateHandoverVariance', () => {
  const matched = calculateHandoverVariance(1000000, 1000000);
  assert.equal(matched.isMatched, true);
  assert.equal(matched.varianceAmount, 0);

  const variance = calculateHandoverVariance(1000000, 950000);
  assert.equal(variance.isMatched, false);
  assert.equal(variance.varianceAmount, 50000);
});

test('UC_LOG_026: validateVarianceResolutionNote', () => {
  const valid = validateVarianceResolutionNote('Tài xế bổ sung nộp đủ');
  assert.equal(valid.isValid, true);

  const short = validateVarianceResolutionNote('OK');
  assert.equal(short.isValid, false);
});

test('UC_LOG_027: validateReturnOrderCreate', () => {
  const valid = validateReturnOrderCreate('Failed');
  assert.equal(valid.canReturn, true);

  const draft = validateReturnOrderCreate('Draft');
  assert.equal(draft.canReturn, false);
});
