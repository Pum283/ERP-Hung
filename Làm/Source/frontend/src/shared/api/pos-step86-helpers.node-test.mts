import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePosCsvExport,
  calculateChainTargetAttainment,
  validateMonthlyTarget,
  validateVendorUpsert,
} from './pos-step86-helpers.ts';

test('UC_POS_068: validatePosCsvExport', () => {
  const valid = validatePosCsvExport('RevenueByTime', 10);
  assert.equal(valid.canExport, true);

  const empty = validatePosCsvExport('RevenueByTime', 0);
  assert.equal(empty.canExport, false);
  assert.match(empty.reason!, /không có dữ liệu/);
});

test('UC_POS_069: calculateChainTargetAttainment', () => {
  const ahead = calculateChainTargetAttainment(60000000, 100000000, 50);
  assert.equal(ahead.attainmentPct, 60);
  assert.equal(ahead.isAheadOfSchedule, true);

  const behind = calculateChainTargetAttainment(40000000, 100000000, 50);
  assert.equal(behind.attainmentPct, 40);
  assert.equal(behind.isAheadOfSchedule, false);
});

test('UC_POS_072: validateMonthlyTarget', () => {
  const valid = validateMonthlyTarget(500000000);
  assert.equal(valid.isValid, true);

  const invalid = validateMonthlyTarget(-500);
  assert.equal(invalid.isValid, false);
  assert.match(invalid.error!, /phải là số >= 0/);
});

test('UC_PUR_001: validateVendorUpsert', () => {
  const valid = validateVendorUpsert('NCC01', 'Công ty ABC', '0101234567');
  assert.equal(valid.isValid, true);

  const emptyCode = validateVendorUpsert('', 'Công ty ABC');
  assert.equal(emptyCode.isValid, false);

  const invalidMst = validateVendorUpsert('NCC01', 'Công ty ABC', 'ABC123');
  assert.equal(invalidMst.isValid, false);
  assert.match(invalidMst.error!, /Mã số thuế/);
});
