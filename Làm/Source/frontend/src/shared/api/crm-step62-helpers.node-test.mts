import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatCampaignStatusBadge,
  validateLeadSourceInput,
  formatLeadSourceChannelType,
  formatLandingPageUrl,
} from './crm-step62-helpers.ts';

// ─── UC_CRM_023: formatCampaignStatusBadge ───

test('formatCampaignStatusBadge - active and closed status return correct badge labels', () => {
  const active = formatCampaignStatusBadge('Active');
  assert.equal(active.isClosed, false);
  assert.ok(active.label.includes('Đang triển khai'));

  const closed = formatCampaignStatusBadge('Closed');
  assert.equal(closed.isClosed, true);
  assert.ok(closed.label.includes('Đã hoàn tất'));
});

// ─── UC_CRM_024: validateLeadSourceInput ───

test('validateLeadSourceInput - valid input returns no errors', () => {
  const res = validateLeadSourceInput({ code: 'SRC_FB', name: 'Facebook Ads', channelType: 'Social' });
  assert.equal(res.isValid, true);
  assert.equal(res.errors.length, 0);
});

test('validateLeadSourceInput - invalid channel type returns error', () => {
  const res = validateLeadSourceInput({ code: 'SRC_BAD', name: 'Nguồn Lỗi', channelType: 'InvalidChannel' });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('Loại kênh nguồn')));
});

test('validateLeadSourceInput - empty code and name returns error', () => {
  const res = validateLeadSourceInput({ code: '', name: '', channelType: 'Website' });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('Mã nguồn lead')));
  assert.ok(res.errors.some(e => e.includes('Tên nguồn lead')));
});

// ─── UC_CRM_024: formatLeadSourceChannelType ───

test('formatLeadSourceChannelType - returns formatted channel icons and labels', () => {
  assert.ok(formatLeadSourceChannelType('Social').includes('Mạng xã hội'));
  assert.ok(formatLeadSourceChannelType('Website').includes('Website'));
});

// ─── UC_CRM_025 & 026: formatLandingPageUrl ───

test('formatLandingPageUrl - truncates long URL properly', () => {
  const longUrl = 'https://erp.vn/landing-page-khuyen-mai-tri-an-khach-hang-2026-sieu-uu-dai-mua-hang';
  const formatted = formatLandingPageUrl(longUrl);
  assert.ok(formatted.endsWith('...'));
  assert.ok(formatted.length <= 60);
});
