import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateCsatStars,
  formatOnlineOrderCode,
  validateLeadCaptureForm,
} from './crm-chatbot-lead-csat-order-helpers.ts';

test('evaluateCsatStars - formats 5 stars correctly', () => {
  const res = evaluateCsatStars(5);
  assert.equal(res.starsDisplay.includes('⭐⭐⭐⭐⭐'), true);
  assert.equal(res.badgeClass.includes('emerald'), true);
});

test('evaluateCsatStars - formats 2 stars correctly', () => {
  const res = evaluateCsatStars(2);
  assert.equal(res.starsDisplay.includes('⭐⭐'), true);
  assert.equal(res.badgeClass.includes('rose'), true);
});

test('formatOnlineOrderCode - prepends channel prefix', () => {
  const code = formatOnlineOrderCode('Zalo', '9981');
  assert.equal(code, 'ORD-ZALO-9981');
});

test('validateLeadCaptureForm - checks required fields', () => {
  assert.equal(validateLeadCaptureForm('', '0908123456').isValid, false);
  assert.equal(validateLeadCaptureForm('Nguyễn Văn A', '123').isValid, false);
  assert.equal(validateLeadCaptureForm('Nguyễn Văn A', '0908123456').isValid, true);
});
