import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateContractValidityStatus,
  formatContractFileSize,
  validateContractForm,
} from './crm-sales-contract-admin-helpers.ts';

test('evaluateContractValidityStatus - evaluates active and expiring dates', () => {
  const future = new Date(Date.now() + 60 * 86400 * 1000).toISOString();
  const resFuture = evaluateContractValidityStatus(future);
  assert.equal(resFuture.label, 'Còn hiệu lực');

  const past = new Date(Date.now() - 5 * 86400 * 1000).toISOString();
  const resPast = evaluateContractValidityStatus(past);
  assert.equal(resPast.label, 'Đã hết hạn');
});

test('formatContractFileSize - formats bytes into readable units', () => {
  assert.equal(formatContractFileSize(1024), '1 KB');
  assert.equal(formatContractFileSize(2450000), '2.3 MB');
});

test('validateContractForm - validates contract inputs', () => {
  assert.equal(validateContractForm('', 500000, 'Khách A').isValid, false);
  assert.equal(validateContractForm('HD-001', 0, 'Khách A').isValid, false);
  assert.equal(validateContractForm('HD-001', 500000, 'Khách A').isValid, true);
});
