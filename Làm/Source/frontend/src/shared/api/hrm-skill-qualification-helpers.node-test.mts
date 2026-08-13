import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateSkillProficiency,
  calculateMovementStats,
  renderContractTemplate,
  validateBulkCandidateRow,
} from './hrm-skill-qualification-helpers.ts';
import type {
  EmployeeMovementItem,
  CandidateImportRow,
} from './hrm-skill-qualification-helpers.ts';

test('validateSkillProficiency - valid proficiency levels return normalized string', () => {
  assert.deepEqual(validateSkillProficiency('expert'), { isValid: true, normalized: 'Expert' });
  assert.deepEqual(validateSkillProficiency('BASIC'), { isValid: true, normalized: 'Basic' });
  assert.deepEqual(validateSkillProficiency('Advanced'), { isValid: true, normalized: 'Advanced' });
});

test('validateSkillProficiency - invalid level falls back to Intermediate', () => {
  const res = validateSkillProficiency('UnknownLevel');
  assert.equal(res.isValid, false);
  assert.equal(res.normalized, 'Intermediate');
});

test('calculateMovementStats - correctly calculates joiners, leavers and turnover rate', () => {
  const employees: EmployeeMovementItem[] = [
    { id: '1', status: 'Active', hireDate: '2026-08-01', terminateDate: null },
    { id: '2', status: 'Active', hireDate: '2026-07-01', terminateDate: null },
    { id: '3', status: 'Terminated', hireDate: '2025-01-01', terminateDate: '2026-08-05' },
    { id: '4', status: 'OnLeave', hireDate: '2026-01-01', terminateDate: null },
  ];

  const stats = calculateMovementStats(employees, '2026-08-01', '2026-08-31');
  assert.equal(stats.total, 4);
  assert.equal(stats.active, 2);
  assert.equal(stats.onLeave, 1);
  assert.equal(stats.terminated, 1);
  assert.equal(stats.joiners, 1);
  assert.equal(stats.leavers, 1);
  assert.equal(stats.turnoverRate, 25); // 1 / 4 * 100
});

test('calculateMovementStats - empty employees list returns zeroes', () => {
  const stats = calculateMovementStats([], '2026-08-01', '2026-08-31');
  assert.equal(stats.total, 0);
  assert.equal(stats.turnoverRate, 0);
});

test('renderContractTemplate - renders contract text correctly with formatting', () => {
  const text = renderContractTemplate({
    contractNo: 'HD-2026-999',
    employeeName: 'Trần Văn X',
    employeeCode: 'EMP999',
    contractType: 'Indefinite',
    startDate: '2026-08-01',
    baseSalary: 15000000,
  });

  assert.match(text, /HỢP ĐỒNG LAO ĐỘNG \(INDEFINITE\)/);
  assert.match(text, /HD-2026-999/);
  assert.match(text, /Trần Văn X/);
  assert.match(text, /15\.000\.000 VNĐ/);
});

test('renderContractTemplate - renders open-ended contract without salary correctly', () => {
  const text = renderContractTemplate({
    contractNo: 'HD-2026-000',
    employeeName: 'Nguyễn Văn Y',
    employeeCode: 'EMP000',
    contractType: 'Probation',
    startDate: '2026-08-01',
  });

  assert.match(text, /Vô thời hạn/);
  assert.match(text, /Thỏa thuận/);
});

test('validateBulkCandidateRow - valid row passes', () => {
  const row: CandidateImportRow = {
    fullName: 'Lê Thị Z',
    email: 'z.le@example.com',
    phone: '0912345678',
    jobPostingId: 'post-1',
  };
  const res = validateBulkCandidateRow(row);
  assert.equal(res.isValid, true);
});

test('validateBulkCandidateRow - missing fullName fails', () => {
  const row: CandidateImportRow = {
    fullName: '   ',
    email: 'z.le@example.com',
    jobPostingId: 'post-1',
  };
  const res = validateBulkCandidateRow(row);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Họ tên ứng viên không được để trống.');
});

test('validateBulkCandidateRow - missing jobPostingId fails', () => {
  const row: CandidateImportRow = {
    fullName: 'Lê Thị Z',
    jobPostingId: '',
  };
  const res = validateBulkCandidateRow(row);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Chưa chọn vị trí tuyển dụng.');
});

test('validateBulkCandidateRow - invalid email fails', () => {
  const row: CandidateImportRow = {
    fullName: 'Lê Thị Z',
    email: 'invalid-email',
    jobPostingId: 'post-1',
  };
  const res = validateBulkCandidateRow(row);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Email không đúng định dạng.');
});
