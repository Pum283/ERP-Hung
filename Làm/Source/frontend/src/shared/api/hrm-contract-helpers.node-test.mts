import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateContractForm,
  calculateContractExpiringSeverity,
  generateSuggestedAnnexNo,
} from './hrm-contract-helpers.ts';

// ─── UC_HRM_038: validateContractForm ───

test('validateContractForm - valid definite contract returns true', () => {
  const res = validateContractForm({
    contractNo: 'HD-001',
    contractType: 'Definite',
    startDate: '2026-01-01',
    endDate: '2027-01-01',
    baseSalary: 15000000,
  });
  assert.equal(res.valid, true);
});

test('validateContractForm - definite contract without endDate returns error', () => {
  const res = validateContractForm({
    contractNo: 'HD-002',
    contractType: 'Definite',
    startDate: '2026-01-01',
    endDate: null,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Ngày kết thúc'));
});

test('validateContractForm - endDate <= startDate returns error', () => {
  const res = validateContractForm({
    contractNo: 'HD-003',
    contractType: 'Definite',
    startDate: '2026-01-01',
    endDate: '2025-12-31',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('sau ngày bắt đầu'));
});

test('validateContractForm - negative baseSalary returns error', () => {
  const res = validateContractForm({
    contractNo: 'HD-004',
    contractType: 'Indefinite',
    startDate: '2026-01-01',
    baseSalary: -500,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Lương cơ bản'));
});

// ─── UC_HRM_039: generateSuggestedAnnexNo ───

test('generateSuggestedAnnexNo - formats parent-PL1 correctly', () => {
  assert.equal(generateSuggestedAnnexNo('HD-2026-001', 0), 'HD-2026-001-PL1');
  assert.equal(generateSuggestedAnnexNo('HD-2026-001', 1), 'HD-2026-001-PL2');
});

// ─── UC_HRM_043: calculateContractExpiringSeverity ───

test('calculateContractExpiringSeverity - categorizes severity correctly', () => {
  assert.equal(calculateContractExpiringSeverity(5), 'critical');
  assert.equal(calculateContractExpiringSeverity(12), 'warning');
  assert.equal(calculateContractExpiringSeverity(25), 'info');
});
