import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatChannelTypeBadge,
  formatAttributionSummary,
  calculateMarketingFunnelRates,
  formatFinancialMetrics,
} from './crm-step63-helpers.ts';

// ─── UC_CRM_027: formatChannelTypeBadge ───

test('formatChannelTypeBadge - returns partner, event and referral badges correctly', () => {
  assert.ok(formatChannelTypeBadge('partner_api').label.includes('Đối tác'));
  assert.ok(formatChannelTypeBadge('event_workshop').label.includes('Sự kiện'));
  assert.ok(formatChannelTypeBadge('referral').icon.includes('💬'));
});

// ─── UC_CRM_028: formatAttributionSummary ───

test('formatAttributionSummary - formats full attribution details', () => {
  const summary = formatAttributionSummary('facebook', 'cpc', 'CAMP_FB_TET');
  assert.ok(summary.includes('Nguồn: facebook'));
  assert.ok(summary.includes('Kênh: cpc'));
  assert.ok(summary.includes('Chiến dịch: CAMP_FB_TET'));
});

test('formatAttributionSummary - handles missing parameters with defaults', () => {
  const summary = formatAttributionSummary();
  assert.ok(summary.includes('Nguồn: Trực tiếp'));
  assert.ok(summary.includes('Chiến dịch: Không gắn chiến dịch'));
});

// ─── UC_CRM_029 & 030: calculateMarketingFunnelRates ───

test('calculateMarketingFunnelRates - calculates conversion rate, ROAS, CPL and CAC', () => {
  const res = calculateMarketingFunnelRates(100, 10, 20000000, 60000000);
  assert.equal(res.conversionRatePct, 10);
  assert.equal(res.roas, 3);
  assert.equal(res.cpl, 200000); // 20M / 100 leads
  assert.equal(res.cac, 2000000); // 20M / 10 customers
  assert.ok(res.funnelStatus.includes('hiệu quả cao'));
});

test('calculateMarketingFunnelRates - zero leads and zero spent returns 0 without crashing', () => {
  const res = calculateMarketingFunnelRates(0, 0, 0, 0);
  assert.equal(res.conversionRatePct, 0);
  assert.equal(res.roas, 0);
  assert.equal(res.cpl, 0);
  assert.equal(res.cac, 0);
});

// ─── UC_CRM_029: formatFinancialMetrics ───

test('formatFinancialMetrics - formats financial summary metrics string', () => {
  const formatted = formatFinancialMetrics(150000, 1500000, 4.2, 320);
  assert.ok(formatted.includes('CPL: 150.000'));
  assert.ok(formatted.includes('CAC: 1.500.000'));
  assert.ok(formatted.includes('ROAS: 4.2x'));
  assert.ok(formatted.includes('ROI: +320%'));
});
