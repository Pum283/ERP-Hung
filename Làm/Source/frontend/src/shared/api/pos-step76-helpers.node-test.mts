import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateStoreRequest,
  formatStoreBadge,
  validateTerminalRequest,
  formatTerminalBadge,
  validatePrinterRequest,
  formatPrinterStatus,
  validateCashierAssignment,
  formatCashierRoleBadge,
} from './pos-step76-helpers.ts';

// ─── UC_POS_001: Khai báo điểm bán POS ───

test('UC_POS_001 — validateStoreRequest — valid inputs pass', () => {
  const result = validateStoreRequest('STORE-01', 'Cửa hàng Q1', 50000000);
  assert.equal(result.isValid, true);
});

test('UC_POS_001 — validateStoreRequest — missing code rejected', () => {
  const result = validateStoreRequest('', 'Cửa hàng Q1');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('Mã điểm bán'));
});

test('UC_POS_001 — formatStoreBadge — Active status returns green badge', () => {
  const badge = formatStoreBadge('Active');
  assert.equal(badge.style, 'active');
  assert.ok(badge.label.includes('Hoạt động'));
});

// ─── UC_POS_002: Khai báo quầy / máy POS ───

test('UC_POS_002 — validateTerminalRequest — valid terminal request', () => {
  const result = validateTerminalRequest('POS-01', 'Quầy 1');
  assert.equal(result.isValid, true);
});

test('UC_POS_002 — formatTerminalBadge — formats with code & name', () => {
  const badge = formatTerminalBadge('POS-01', 'Quầy Thu Ngân', 'Active');
  assert.ok(badge.includes('POS-01'));
  assert.ok(badge.includes('Quầy Thu Ngân'));
  assert.ok(badge.includes('💻'));
});

// ─── UC_POS_003: Cấu hình máy in hóa đơn ───

test('UC_POS_003 — validatePrinterRequest — valid printer request', () => {
  const result = validatePrinterRequest('PRT-01', 'Máy In K80', 'Receipt');
  assert.equal(result.isValid, true);
});

test('UC_POS_003 — validatePrinterRequest — invalid printer type rejected', () => {
  const result = validatePrinterRequest('PRT-02', 'Máy In 3D', 'OtherType');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('Receipt'));
});

test('UC_POS_003 — formatPrinterStatus — formats printer description', () => {
  const status = formatPrinterStatus('Máy Bếp 1', 'Kitchen', '192.168.1.100');
  assert.ok(status.includes('Máy Bếp 1'));
  assert.ok(status.includes('Bếp'));
  assert.ok(status.includes('192.168.1.100'));
});

// ─── UC_POS_007: Phân quyền thu ngân ───

test('UC_POS_007 — validateCashierAssignment — valid cashier role passes', () => {
  const result = validateCashierAssignment('user-guid-123', 'Cashier');
  assert.equal(result.isValid, true);
});

test('UC_POS_007 — validateCashierAssignment — invalid role rejected', () => {
  const result = validateCashierAssignment('user-guid-123', 'Admin');
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('Cashier hoặc Supervisor'));
});

test('UC_POS_007 — formatCashierRoleBadge — Supervisor role returns star icon', () => {
  const badge = formatCashierRoleBadge('Supervisor');
  assert.equal(badge.icon, '⭐');
  assert.ok(badge.label.includes('Supervisor'));
});
