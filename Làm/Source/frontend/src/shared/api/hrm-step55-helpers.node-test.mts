import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLmsInstructorInput,
  formatInstructorStatus,
  formatInstructorRoleSummary,
  formatCertificateVerificationUrl,
  calculateLearnerCompletionRate,
} from './hrm-step55-helpers.ts';

// ─── UC_LMS_049: validateLmsInstructorInput & formatInstructorStatus ───

test('validateLmsInstructorInput - valid instructor input returns valid', () => {
  const res = validateLmsInstructorInput({
    code: 'INS_001',
    displayName: 'ThS. Nguyễn Văn A',
    email: 'teacher@erp.vn',
    phone: '0901234567',
  });
  assert.equal(res.valid, true);
});

test('validateLmsInstructorInput - invalid email returns error', () => {
  const res = validateLmsInstructorInput({
    code: 'INS_001',
    displayName: 'ThS. Nguyễn Văn A',
    email: 'invalid-email',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('email không hợp lệ'));
});

test('formatInstructorStatus - formats instructor status badge text', () => {
  assert.ok(formatInstructorStatus('Active').includes('Đang giảng dạy'));
  assert.ok(formatInstructorStatus('Inactive').includes('Tạm ngưng'));
});

// ─── UC_LMS_050: formatInstructorRoleSummary ───

test('formatInstructorRoleSummary - formats role grant summary', () => {
  const granted = formatInstructorRoleSummary('ThS. Nguyễn Văn A', true);
  assert.ok(granted.includes('Đã được cấp quyền'));

  const notGranted = formatInstructorRoleSummary('ThS. Nguyễn Văn A', false);
  assert.ok(notGranted.includes('Chưa phân quyền'));
});

// ─── UC_LMS_045 & 051: formatCertificateVerificationUrl & calculateLearnerCompletionRate ───

test('formatCertificateVerificationUrl - builds valid verification URL', () => {
  const url = formatCertificateVerificationUrl('CERT-ERP-2026-001');
  assert.equal(url, 'https://lms.erp.vn/verify-cert/CERT-ERP-2026-001');
});

test('calculateLearnerCompletionRate - calculates learner completion rate correctly', () => {
  const res = calculateLearnerCompletionRate(15, 20);
  assert.equal(res.completionRatePct, 75);
  assert.ok(res.summaryText.includes('15/20 học viên'));
  assert.ok(res.summaryText.includes('75%'));
});
