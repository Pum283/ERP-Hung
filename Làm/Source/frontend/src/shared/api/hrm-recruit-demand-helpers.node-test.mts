import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateRecruitmentRequestForm,
  getRecruitmentStatusBadge,
} from './hrm-recruit-demand-helpers.ts';

// ─── UC_HRM_047 - 049: validateRecruitmentRequestForm ───

test('validateRecruitmentRequestForm - valid form returns true', () => {
  const res = validateRecruitmentRequestForm({
    jobTitleId: 'JT_001',
    orgUnitId: 'ORG_001',
    headcount: 3,
    reason: 'Tuyển thêm nhân sự cho dự án mới',
  });
  assert.equal(res.valid, true);
});

test('validateRecruitmentRequestForm - empty jobTitleId returns error', () => {
  const res = validateRecruitmentRequestForm({
    jobTitleId: '',
    orgUnitId: 'ORG_001',
    headcount: 2,
    reason: 'Lý do hợp lệ',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Chức danh'));
});

test('validateRecruitmentRequestForm - headcount out of range returns error', () => {
  const res1 = validateRecruitmentRequestForm({
    jobTitleId: 'JT_001',
    orgUnitId: 'ORG_001',
    headcount: 0,
    reason: 'Lý do hợp lệ',
  });
  assert.equal(res1.valid, false);
  assert.ok(res1.error?.includes('1 đến 999'));

  const res2 = validateRecruitmentRequestForm({
    jobTitleId: 'JT_001',
    orgUnitId: 'ORG_001',
    headcount: 1000,
    reason: 'Lý do hợp lệ',
  });
  assert.equal(res2.valid, false);
  assert.ok(res2.error?.includes('1 đến 999'));
});

test('validateRecruitmentRequestForm - reason too short returns error', () => {
  const res = validateRecruitmentRequestForm({
    jobTitleId: 'JT_001',
    orgUnitId: 'ORG_001',
    headcount: 2,
    reason: 'Ngắn',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('tối thiểu 5 ký tự'));
});

// ─── UC_HRM_050: getRecruitmentStatusBadge ───

test('getRecruitmentStatusBadge - returns correct badge text and severity', () => {
  assert.deepEqual(getRecruitmentStatusBadge('Draft'), { text: 'Nháp', severity: 'draft' });
  assert.deepEqual(getRecruitmentStatusBadge('Pending'), { text: 'Chờ duyệt', severity: 'pending' });
  assert.deepEqual(getRecruitmentStatusBadge('Approved'), { text: 'Đã duyệt', severity: 'approved' });
  assert.deepEqual(getRecruitmentStatusBadge('Rejected'), { text: 'Từ chối', severity: 'rejected' });
});
