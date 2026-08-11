import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateCategoryRequest,
  formatCategoryBadge,
  validateProductRequest,
  formatProductSummary,
  validateBomLineRequest,
  formatBomLineSummary,
  formatProductActiveStatus,
  toggleProductStatusEligibility,
} from './pos-step77-helpers.ts';

// ─── UC_POS_009: Danh mục nhóm sản phẩm ───

test('UC_POS_009 — validateCategoryRequest — valid inputs pass', () => {
  const result = validateCategoryRequest('CAT-01', 'Đồ Uống');
  assert.equal(result.isValid, true);
});

test('UC_POS_009 — formatCategoryBadge — formats badge text', () => {
  const text = formatCategoryBadge('CAT-BEV', 'Giải Khát', 15);
  assert.ok(text.includes('CAT-BEV'));
  assert.ok(text.includes('15 sản phẩm'));
});

// ─── UC_POS_010: Danh mục sản phẩm bán ───

test('UC_POS_010 — validateProductRequest — valid product inputs pass', () => {
  const result = validateProductRequest('PROD-01', 'Cà Phê Muối', 'Ly');
  assert.equal(result.isValid, true);
});

test('UC_POS_010 — formatProductSummary — active status format', () => {
  const summary = formatProductSummary('PROD-01', 'Cà Phê Muối', 'Ly', 'Active');
  assert.ok(summary.includes('Cà Phê Muối'));
  assert.ok(summary.includes('Đang bán'));
});

// ─── UC_POS_012: BOM / định mức nguyên liệu ───

test('UC_POS_012 — validateBomLineRequest — valid qty passes', () => {
  const result = validateBomLineRequest('MAT-01', 'Sữa Đặc', 0.05);
  assert.equal(result.isValid, true);
});

test('UC_POS_012 — validateBomLineRequest — zero or negative qty rejected', () => {
  const result = validateBomLineRequest('MAT-01', 'Sữa Đặc', 0);
  assert.equal(result.isValid, false);
  assert.ok(result.error?.toLowerCase().includes('định mức'));
});

test('UC_POS_012 — formatBomLineSummary — formats line summary', () => {
  const summary = formatBomLineSummary('Bột Cà Phê', 0.02, 'kg');
  assert.ok(summary.includes('Bột Cà Phê'));
  assert.ok(summary.includes('0.02 kg'));
});

// ─── UC_POS_014: Ngưng bán sản phẩm ───

test('UC_POS_014 — formatProductActiveStatus — Suspended status shows action text', () => {
  const info = formatProductActiveStatus('Suspended');
  assert.equal(info.actionText, 'Mở bán lại');
  assert.ok(info.label.includes('Tạm ngưng'));
});

test('UC_POS_014 — toggleProductStatusEligibility — toggles status correctly', () => {
  assert.equal(toggleProductStatusEligibility('Active').newStatus, 'Suspended');
  assert.equal(toggleProductStatusEligibility('Suspended').newStatus, 'Active');
});
