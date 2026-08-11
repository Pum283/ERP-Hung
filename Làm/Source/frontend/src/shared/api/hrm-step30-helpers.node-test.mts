import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateForgotCheckoutConfig,
  validateAdjustDeadlineConfig,
  validateOvertimeConfig,
  validateNightShiftHolidayConfig,
} from './hrm-step30-helpers.ts';

// ─── UC_HRM_105: validateForgotCheckoutConfig ───

test('validateForgotCheckoutConfig - valid hours returns valid', () => {
  const res = validateForgotCheckoutConfig(14);
  assert.equal(res.valid, true);
});

test('validateForgotCheckoutConfig - zero hours returns error', () => {
  const res = validateForgotCheckoutConfig(0);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 48'));
});

test('validateForgotCheckoutConfig - hours over 48 returns error', () => {
  const res = validateForgotCheckoutConfig(50);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 48'));
});

// ─── UC_HRM_106: validateAdjustDeadlineConfig ───

test('validateAdjustDeadlineConfig - valid deadline days returns valid', () => {
  const res = validateAdjustDeadlineConfig(7);
  assert.equal(res.valid, true);
});

test('validateAdjustDeadlineConfig - negative days returns error', () => {
  const res = validateAdjustDeadlineConfig(-1);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 60'));
});

test('validateAdjustDeadlineConfig - days over 60 returns error', () => {
  const res = validateAdjustDeadlineConfig(90);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 60'));
});

// ─── UC_HRM_107: validateOvertimeConfig ───

test('validateOvertimeConfig - OT enabled with valid minutes returns valid', () => {
  const res = validateOvertimeConfig({ enableOt: true, otAfterMinutes: 30 });
  assert.equal(res.valid, true);
});

test('validateOvertimeConfig - OT disabled returns valid', () => {
  const res = validateOvertimeConfig({ enableOt: false, otAfterMinutes: 0 });
  assert.equal(res.valid, true);
});

test('validateOvertimeConfig - otAfterMinutes over 480 returns error', () => {
  const res = validateOvertimeConfig({ enableOt: true, otAfterMinutes: 600 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 480'));
});

// ─── UC_HRM_108: validateNightShiftHolidayConfig ───

test('validateNightShiftHolidayConfig - valid booleans returns valid', () => {
  const res = validateNightShiftHolidayConfig({
    enableNightShiftRule: true,
    enableHolidayRule: true,
  });
  assert.equal(res.valid, true);
});
