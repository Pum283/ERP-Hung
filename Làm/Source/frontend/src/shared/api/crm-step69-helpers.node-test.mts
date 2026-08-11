import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLeadCsvImport,
  formatLeadImportSummary,
  validateOpportunityInput,
  formatOpportunityStageBadge,
} from './crm-step69-helpers.ts';

// ─── UC_CRM_060: validateLeadCsvImport ───

test('validateLeadCsvImport - valid csv content returns isValid true and correct line count', () => {
  const csv = 'Name,Phone,Email\nNguyễn Văn A,0901234567,a@erp.vn\nTrần Văn B,0907654321,b@erp.vn';
  const res = validateLeadCsvImport(csv);
  assert.equal(res.isValid, true);
  assert.equal(res.lineCount, 2);
});

test('validateLeadCsvImport - empty csv content returns validation error', () => {
  const res = validateLeadCsvImport('');
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('không được để trống'));
});

test('validateLeadCsvImport - csv missing name header returns error', () => {
  const csv = 'Header1,Header2\nValue1,Value2';
  const res = validateLeadCsvImport(csv);
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('thiếu cột bắt buộc'));
});

// ─── UC_CRM_060: formatLeadImportSummary ───

test('formatLeadImportSummary - formats success summary string', () => {
  const summary = formatLeadImportSummary(10, 2, ['Line 3 invalid email']);
  assert.ok(summary.includes('Import thành công 10/12'));
  assert.ok(summary.includes('1 lỗi'));
});

// ─── UC_CRM_062: validateOpportunityInput ───

test('validateOpportunityInput - valid input returns isValid true', () => {
  const res = validateOpportunityInput({ name: 'ERP Cloud Implementation', estimatedValue: 500000000, probabilityPercent: 75 });
  assert.equal(res.isValid, true);
});

// ─── UC_CRM_063: formatOpportunityStageBadge ───

test('formatOpportunityStageBadge - returns correct badge labels for stages', () => {
  const proposal = formatOpportunityStageBadge('Proposal');
  assert.equal(proposal.color, 'orange');
  assert.equal(proposal.probability, 75);

  const won = formatOpportunityStageBadge('ClosedWon');
  assert.equal(won.color, 'green');
  assert.equal(won.probability, 100);
});
