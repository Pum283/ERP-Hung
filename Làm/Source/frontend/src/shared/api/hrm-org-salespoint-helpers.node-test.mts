import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateOrgUnitForm,
  getUnitTypeLabel,
  getUnitTypeBadgeColor,
  validateSalesPointForm,
} from './hrm-org-salespoint-helpers.ts';

// ─── UC_HRM_001 / 002 / 003: validateOrgUnitForm ───

test('validateOrgUnitForm - valid Company form returns valid', () => {
  const res = validateOrgUnitForm({ code: 'HQ', name: 'Tập đoàn', unitType: 'Company' });
  assert.equal(res.valid, true);
});

test('validateOrgUnitForm - valid OperationsBlock form returns valid', () => {
  const res = validateOrgUnitForm({ code: 'OPS', name: 'Khối vận hành', unitType: 'OperationsBlock' });
  assert.equal(res.valid, true);
});

test('validateOrgUnitForm - valid ProductionBlock form returns valid', () => {
  const res = validateOrgUnitForm({ code: 'MFG', name: 'Khối sản xuất', unitType: 'ProductionBlock' });
  assert.equal(res.valid, true);
});

test('validateOrgUnitForm - empty code returns error', () => {
  const res = validateOrgUnitForm({ code: '', name: 'Chi nhánh', unitType: 'Branch' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Code'));
});

test('validateOrgUnitForm - empty name returns error', () => {
  const res = validateOrgUnitForm({ code: 'CN_HN', name: '   ', unitType: 'Branch' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Name'));
});

test('validateOrgUnitForm - invalid unitType returns error', () => {
  const res = validateOrgUnitForm({ code: 'CN_HN', name: 'Chi nhánh', unitType: 'InvalidType' as any });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không hợp lệ'));
});

// ─── Display Helpers ───

test('getUnitTypeLabel - returns correct Vietnamese labels', () => {
  assert.ok(getUnitTypeLabel('Company').includes('Công ty'));
  assert.ok(getUnitTypeLabel('OperationsBlock').includes('Khối Vận Hành'));
  assert.ok(getUnitTypeLabel('ProductionBlock').includes('Khối Sản Xuất'));
  assert.ok(getUnitTypeLabel('Branch').includes('Chi nhánh'));
});

test('getUnitTypeBadgeColor - returns hex colors', () => {
  assert.equal(getUnitTypeBadgeColor('Company'), '#4f46e5');
  assert.equal(getUnitTypeBadgeColor('Branch'), '#10b981');
  assert.equal(getUnitTypeBadgeColor('OperationsBlock'), '#f59e0b');
});

// ─── UC_HRM_004: validateSalesPointForm ───

test('validateSalesPointForm - valid form returns valid', () => {
  const res = validateSalesPointForm({ code: 'SP01', name: 'Cửa hàng 1' });
  assert.equal(res.valid, true);
});

test('validateSalesPointForm - empty code returns error', () => {
  const res = validateSalesPointForm({ code: '', name: 'Cửa hàng 1' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Code'));
});

test('validateSalesPointForm - empty name returns error', () => {
  const res = validateSalesPointForm({ code: 'SP01', name: '  ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Name'));
});
