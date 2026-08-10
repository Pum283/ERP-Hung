import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateApprovalForm,
  validateJobPostingForm,
} from './hrm-recruit-approval-posting-helpers.ts';

// ─── UC_HRM_051: validateApprovalForm ───

test('validateApprovalForm - valid Approve returns true', () => {
  const res = validateApprovalForm({ action: 'Approve', comment: 'Đồng ý' });
  assert.equal(res.valid, true);
});

test('validateApprovalForm - Reject without comment returns error', () => {
  const res = validateApprovalForm({ action: 'Reject', comment: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lý do khi từ chối'));
});

// ─── UC_HRM_054: validateJobPostingForm ───

test('validateJobPostingForm - valid job posting returns true', () => {
  const res = validateJobPostingForm({
    recruitmentRequestId: 'RR_001',
    title: 'Tuyển Lập Trình Viên React Native',
    channel: 'LinkedIn',
  });
  assert.equal(res.valid, true);
});

test('validateJobPostingForm - empty title returns error', () => {
  const res = validateJobPostingForm({
    recruitmentRequestId: 'RR_001',
    title: '',
    channel: 'Website',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Tiêu đề'));
});

test('validateJobPostingForm - short title returns error', () => {
  const res = validateJobPostingForm({
    recruitmentRequestId: 'RR_001',
    title: 'DEV',
    channel: 'Website',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('tối thiểu 5 ký tự'));
});
