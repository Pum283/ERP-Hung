import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatPolicyAckSummary,
  calculateOverallPassRate,
  formatOrgCompletionRow,
  generateTrainingReportFileName,
} from './hrm-step56-helpers.ts';

// ─── UC_LMS_058: formatPolicyAckSummary ───

test('formatPolicyAckSummary - unacknowledged returns warning status', () => {
  const res = formatPolicyAckSummary('Nội quy đào tạo 2026');
  assert.equal(res.isAcknowledged, false);
  assert.ok(res.statusText.includes('Chưa xác nhận'));
});

test('formatPolicyAckSummary - acknowledged returns success status with date', () => {
  const res = formatPolicyAckSummary('Nội quy đào tạo 2026', '2026-08-10T10:00:00Z');
  assert.equal(res.isAcknowledged, true);
  assert.ok(res.statusText.includes('Đã xác nhận'));
});

// ─── UC_LMS_065: calculateOverallPassRate ───

test('calculateOverallPassRate - calculates pass rate percentage correctly', () => {
  const res = calculateOverallPassRate(80, 100);
  assert.equal(res.passRatePct, 80);
  assert.ok(res.label.includes('80%'));
  assert.ok(res.label.includes('80/100'));
});

// ─── UC_LMS_066: formatOrgCompletionRow ───

test('formatOrgCompletionRow - formats org unit training completion row', () => {
  const row = formatOrgCompletionRow('Phòng Đào Tạo', 10, 12, 5, 8);
  assert.ok(row.includes('Phòng Đào Tạo'));
  assert.ok(row.includes('Hoàn thành 15/20'));
  assert.ok(row.includes('75%'));
});

// ─── UC_LMS_070: generateTrainingReportFileName ───

test('generateTrainingReportFileName - generates valid CSV filename for report type', () => {
  const fileName = generateTrainingReportFileName('OnlineEnrollments', 'ERP_HUNG');
  assert.ok(fileName.startsWith('ERP_HUNG_BaoCao_HocVien_Online_'));
  assert.ok(fileName.endsWith('.csv'));
});

test('generateTrainingReportFileName - fallback report type uses default prefix', () => {
  const fileName = generateTrainingReportFileName('CustomType');
  assert.ok(fileName.includes('BaoCao_DaoTao'));
});
