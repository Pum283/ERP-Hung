import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatQuoteExpirationStatus,
  isQuoteExpiringSoon,
  validateQuoteForConversion,
  formatOrderConversionNotice,
  generateQuotePrintTemplateHtml,
  formatSalesOrderSummary,
  formatOrderLineItemRow,
} from './crm-step73-helpers.ts';

// ─── UC_CRM_076: Hết hạn báo giá tự động ───

test('UC_CRM_076 — formatQuoteExpirationStatus — expired status returns ĐÃ HẾT HẠN', () => {
  const result = formatQuoteExpirationStatus(undefined, 'Expired');
  assert.equal(result.isExpired, true);
  assert.ok(result.statusLabel.includes('ĐÃ HẾT HẠN'));
  assert.equal(result.daysRemaining, 0);
});

test('UC_CRM_076 — formatQuoteExpirationStatus — future validUntil shows days remaining', () => {
  const future = new Date(Date.now() + 5 * 24 * 3600 * 1000);
  const result = formatQuoteExpirationStatus(future, 'Draft');
  assert.equal(result.isExpired, false);
  assert.ok(result.statusLabel.includes('ngày hiệu lực'));
  assert.ok(result.daysRemaining > 0);
});

test('UC_CRM_076 — formatQuoteExpirationStatus — past date returns QUÁ HẠN HIỆU LỰC', () => {
  const past = new Date(Date.now() - 2 * 24 * 3600 * 1000);
  const result = formatQuoteExpirationStatus(past, 'Draft');
  assert.equal(result.isExpired, true);
  assert.ok(result.statusLabel.includes('QUÁ HẠN'));
});

test('UC_CRM_076 — isQuoteExpiringSoon — quote expiring within threshold returns true', () => {
  const soonDate = new Date(Date.now() + 2 * 24 * 3600 * 1000);
  assert.equal(isQuoteExpiringSoon(soonDate, 3), true);
  const farDate = new Date(Date.now() + 10 * 24 * 3600 * 1000);
  assert.equal(isQuoteExpiringSoon(farDate, 3), false);
});

// ─── UC_CRM_077: Chuyển báo giá thành đơn ───

test('UC_CRM_077 — validateQuoteForConversion — valid Draft quote can convert', () => {
  const result = validateQuoteForConversion('Draft', 'None', 2, 50000000);
  assert.equal(result.canConvert, true);
  assert.equal(result.reason, undefined);
});

test('UC_CRM_077 — validateQuoteForConversion — Expired quote cannot convert', () => {
  const result = validateQuoteForConversion('Expired', 'None', 1, 10000000);
  assert.equal(result.canConvert, false);
  assert.ok(result.reason?.includes('Expired'));
});

test('UC_CRM_077 — validateQuoteForConversion — PendingDiscount blocks conversion', () => {
  const result = validateQuoteForConversion('Draft', 'Pending', 1, 10000000);
  assert.equal(result.canConvert, false);
  assert.ok(result.reason?.includes('chiết khấu'));
});

test('UC_CRM_077 — formatOrderConversionNotice — formats success text', () => {
  const notice = formatOrderConversionNotice('SO-2026-001', 'QT-2026-001');
  assert.ok(notice.includes('SO-2026-001'));
  assert.ok(notice.includes('QT-2026-001'));
  assert.ok(notice.includes('🛒'));
});

// ─── UC_CRM_078: In mẫu báo giá ───

test('UC_CRM_078 — generateQuotePrintTemplateHtml — generates complete HTML', () => {
  const html = generateQuotePrintTemplateHtml({
    code: 'QT-999',
    customerName: 'Công ty ABC',
    totalAmount: 150000000,
    validUntil: '2026-12-31',
  });
  assert.ok(html.includes('BÁO GIÁ SẢN PHẨM / DỊCH VỤ - QT-999'));
  assert.ok(html.includes('Công ty ABC'));
  assert.ok(html.includes('VNĐ'));
  assert.ok(html.includes('quote-print-template'));
});

// ─── UC_CRM_079: Tạo đơn hàng từ báo giá ───

test('UC_CRM_079 — formatSalesOrderSummary — formats order summary with status icon', () => {
  const summary = formatSalesOrderSummary('SO-005', 200000000, 'Confirmed');
  assert.ok(summary.includes('SO-005'));
  assert.ok(summary.includes('VNĐ'));
  assert.ok(summary.includes('✅'));
});

test('UC_CRM_079 — formatOrderLineItemRow — formats line item row correctly', () => {
  const row = formatOrderLineItemRow(1, 'SKU-001', 'Gói ERP Pro', 2, 25000000);
  assert.ok(row.includes('SKU-001'));
  assert.ok(row.includes('Gói ERP Pro'));
  assert.ok(row.includes('SL: 2'));
  assert.ok(row.includes('VNĐ'));
});
