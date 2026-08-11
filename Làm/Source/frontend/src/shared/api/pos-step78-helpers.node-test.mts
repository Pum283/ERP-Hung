import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatCatalogSyncNotice,
  validatePriceListRequest,
  formatPriceListBadge,
  validateTaxRateRequest,
  formatTaxRateBadge,
  validatePromotionRequest,
  formatPromotionSummary,
} from './pos-step78-helpers.ts';

// ─── UC_POS_015: Đồng bộ catalog ───

test('UC_POS_015 — formatCatalogSyncNotice — formats sync notice text', () => {
  const notice = formatCatalogSyncNotice(100, 10, 5, 2);
  assert.ok(notice.includes('100 SP tổng cộng'));
  assert.ok(notice.includes('10 mới'));
});

// ─── UC_POS_016: Bảng giá theo điểm bán ───

test('UC_POS_016 — validatePriceListRequest — valid price list request passes', () => {
  const result = validatePriceListRequest('PL-Q1', 'Bảng Giá Q1', 'store-guid-1');
  assert.equal(result.isValid, true);
});

test('UC_POS_016 — formatPriceListBadge — formats price list description', () => {
  const text = formatPriceListBadge('PL-Q1', 'Bảng Giá Tết', 25);
  assert.ok(text.includes('PL-Q1'));
  assert.ok(text.includes('25 sản phẩm'));
});

// ─── UC_POS_019: Cấu hình thuế GTGT ───

test('UC_POS_019 — validateTaxRateRequest — valid rate passes', () => {
  const result = validateTaxRateRequest('VAT8', 'Thuế 8%', 8);
  assert.equal(result.isValid, true);
});

test('UC_POS_019 — validateTaxRateRequest — rate > 100 rejected', () => {
  const result = validateTaxRateRequest('VAT150', 'Thuế Lỗi', 150);
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('0% đến 100%'));
});

test('UC_POS_019 — formatTaxRateBadge — default tax rate shows star icon', () => {
  const badge = formatTaxRateBadge('Thuế GTGT 10%', 10, true);
  assert.ok(badge.includes('⭐'));
  assert.ok(badge.includes('10%'));
});

// ─── UC_POS_021: Áp dụng chương trình khuyến mại ───

test('UC_POS_021 — validatePromotionRequest — valid percent promo passes', () => {
  const result = validatePromotionRequest('PROMO-10', 'Giảm 10%', 'Percent', 10);
  assert.equal(result.isValid, true);
});

test('UC_POS_021 — validatePromotionRequest — percent > 100 rejected', () => {
  const result = validatePromotionRequest('PROMO-ERR', 'Giảm 200%', 'Percent', 200);
  assert.equal(result.isValid, false);
  assert.ok(result.error?.includes('tối đa 100%'));
});

test('UC_POS_021 — formatPromotionSummary — formats promotion string', () => {
  const summary = formatPromotionSummary('KM Hè', 'Amount', 50000, 200000);
  assert.ok(summary.includes('KM Hè'));
  assert.ok(summary.includes('50.000 VNĐ'));
  assert.ok(summary.includes('Đơn từ 200.000 VNĐ'));
});
