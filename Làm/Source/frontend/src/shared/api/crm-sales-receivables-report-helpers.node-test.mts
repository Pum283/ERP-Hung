import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateReceivableDebtRiskLevel,
  validateReportExportForm,
} from './crm-sales-receivables-report-helpers.ts';

test('evaluateReceivableDebtRiskLevel - evaluates debt risk percentage', () => {
  const high = evaluateReceivableDebtRiskLevel(45);
  assert.equal(high.label.includes('Rủi ro cao'), true);

  const safe = evaluateReceivableDebtRiskLevel(10);
  assert.equal(safe.label.includes('an toàn'), true);
});

test('validateReportExportForm - checks report name and email format', () => {
  assert.equal(validateReportExportForm('', 'test@erphung.vn').isValid, false);
  assert.equal(validateReportExportForm('Báo cáo nợ', 'invalid-email').isValid, false);
  assert.equal(validateReportExportForm('Báo cáo nợ', 'admin@erphung.vn').isValid, true);
});
