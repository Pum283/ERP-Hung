import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatExamTimeRemaining,
  calculateExamScorePercentage,
  evaluateExamPassStatus,
  formatCertificateEligibilityMessage,
} from './hrm-step54-helpers.ts';

// ─── UC_LMS_041: formatExamTimeRemaining ───

test('formatExamTimeRemaining - no limit returns unlimited string', () => {
  const text = formatExamTimeRemaining(0);
  assert.ok(text.includes('Không giới hạn'));
});

test('formatExamTimeRemaining - calculates countdown string correctly', () => {
  const recentStart = new Date(Date.now() - 5 * 60 * 1000).toISOString(); // Started 5 mins ago
  const text = formatExamTimeRemaining(45, recentStart);
  assert.ok(text.includes('Còn lại'));
  assert.ok(text.includes('39:') || text.includes('40:'));
});

// ─── UC_LMS_042 & 043: calculateExamScorePercentage & evaluateExamPassStatus ───

test('calculateExamScorePercentage - calculates score percentage correctly', () => {
  const pct = calculateExamScorePercentage(80, 100);
  assert.equal(pct, 80);
});

test('evaluateExamPassStatus - evaluates pass and fail status labels', () => {
  const pass = evaluateExamPassStatus(85, 80);
  assert.equal(pass.isPassed, true);
  assert.ok(pass.label.includes('ĐẠT KẾT QUẢ'));

  const fail = evaluateExamPassStatus(75, 80);
  assert.equal(fail.isPassed, false);
  assert.ok(fail.label.includes('KHÔNG ĐẠT'));
});

// ─── UC_LMS_044: formatCertificateEligibilityMessage ───

test('formatCertificateEligibilityMessage - eligible when exam passed and 100% course completed', () => {
  const res = formatCertificateEligibilityMessage(true, 100);
  assert.equal(res.eligible, true);
  assert.ok(res.message.includes('đủ điều kiện'));
});

test('formatCertificateEligibilityMessage - ineligible when course incomplete', () => {
  const res = formatCertificateEligibilityMessage(true, 80);
  assert.equal(res.eligible, false);
  assert.ok(res.message.includes('chưa hoàn thành 100%'));
});
