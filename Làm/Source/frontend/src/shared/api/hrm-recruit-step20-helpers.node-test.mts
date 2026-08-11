import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateCareNote,
  calculateConversionRate,
  validateOnboardingSettingsForm,
} from './hrm-recruit-step20-helpers.ts';

// ─── UC_HRM_064: validateCareNote ───

test('validateCareNote - valid note returns valid', () => {
  const res = validateCareNote('Đã gọi điện tư vấn JD');
  assert.equal(res.valid, true);
});

test('validateCareNote - empty note returns error', () => {
  const res = validateCareNote('   ');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('trống'));
});

test('validateCareNote - note too long returns error', () => {
  const res = validateCareNote('X'.repeat(1001));
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1000 ký tự'));
});

// ─── UC_HRM_065: calculateConversionRate ───

test('calculateConversionRate - 1 accepted out of 2 candidates returns 50%', () => {
  const rate = calculateConversionRate(1, 2);
  assert.equal(rate, 50);
});

test('calculateConversionRate - 0 accepted returns 0%', () => {
  const rate = calculateConversionRate(0, 10);
  assert.equal(rate, 0);
});

test('calculateConversionRate - 0 candidates returns 0%', () => {
  const rate = calculateConversionRate(0, 0);
  assert.equal(rate, 0);
});

test('calculateConversionRate - rounding to 2 decimals', () => {
  const rate = calculateConversionRate(1, 3);
  assert.equal(rate, 33.33);
});

// ─── UC_HRM_066 + UC_HRM_067: validateOnboardingSettingsForm ───

test('validateOnboardingSettingsForm - valid 30 onboarding & 60 trial returns valid', () => {
  const res = validateOnboardingSettingsForm({ onboardingDays: 30, trialDays: 60 });
  assert.equal(res.valid, true);
});

test('validateOnboardingSettingsForm - onboardingDays 0 returns error', () => {
  const res = validateOnboardingSettingsForm({ onboardingDays: 0, trialDays: 60 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('onboarding'));
});

test('validateOnboardingSettingsForm - onboardingDays 400 returns error', () => {
  const res = validateOnboardingSettingsForm({ onboardingDays: 400, trialDays: 60 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('onboarding'));
});

test('validateOnboardingSettingsForm - trialDays 0 returns error', () => {
  const res = validateOnboardingSettingsForm({ onboardingDays: 30, trialDays: 0 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('thử việc'));
});

test('validateOnboardingSettingsForm - trialDays 500 returns error', () => {
  const res = validateOnboardingSettingsForm({ onboardingDays: 30, trialDays: 500 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('thử việc'));
});
