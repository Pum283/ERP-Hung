import test from 'node:test';
import assert from 'node:assert/strict';
import {
  generateCustomerCsvExportFileName,
  validateCsvImportLine,
  formatCampaignChannelBadge,
  formatUtmSummary,
} from './crm-step60-helpers.ts';

// ─── UC_CRM_014: generateCustomerCsvExportFileName ───

test('generateCustomerCsvExportFileName - generates valid filename with tenant code', () => {
  const name = generateCustomerCsvExportFileName('ERP_HUNG');
  assert.ok(name.startsWith('ERP_HUNG_DanhSach_KhachHang_'));
  assert.ok(name.endsWith('.csv'));
});

// ─── UC_CRM_014: validateCsvImportLine ───

test('validateCsvImportLine - valid csv row returns parsed columns', () => {
  const line = 'CUST_IMP01, Person, Nguyễn Văn A, 0901234567';
  const res = validateCsvImportLine(line, 2);
  assert.equal(res.isValid, true);
  assert.equal(res.parsedCols?.length, 4);
  assert.equal(res.parsedCols?.[0], 'CUST_IMP01');
});

test('validateCsvImportLine - line with insufficient columns returns error', () => {
  const line = 'CUST_IMP01, Person';
  const res = validateCsvImportLine(line, 3);
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Thiếu cột bắt buộc'));
});

test('validateCsvImportLine - empty line returns error', () => {
  const res = validateCsvImportLine('   ', 1);
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Dòng trống'));
});

// ─── UC_CRM_016: formatCampaignChannelBadge ───

test('formatCampaignChannelBadge - returns correct icons and channel labels', () => {
  assert.ok(formatCampaignChannelBadge('Social').label.includes('Social'));
  assert.ok(formatCampaignChannelBadge('SEM').label.includes('SEM'));
  assert.ok(formatCampaignChannelBadge('Email').icon.includes('📧'));
});

// ─── UC_CRM_017: formatUtmSummary ───

test('formatUtmSummary - formats UTM source, medium, campaign parameters', () => {
  const summary = formatUtmSummary('google', 'cpc', 'tet_2026');
  assert.ok(summary.includes('google / cpc'));
  assert.ok(summary.includes('tet_2026'));
});
