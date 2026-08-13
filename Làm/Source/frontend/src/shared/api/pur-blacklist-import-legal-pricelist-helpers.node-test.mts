import test from 'node:test';
import assert from 'node:assert/strict';
import {
  checkPricelistActiveValidity,
  validateBatchImportSupplierRows,
} from './pur-blacklist-import-legal-pricelist-helpers.ts';

test('checkPricelistActiveValidity - checks effective date range validity', () => {
  const now = new Date();
  const past = new Date(now.getTime() - 1000 * 60 * 60 * 24 * 30);
  const future = new Date(now.getTime() + 1000 * 60 * 60 * 24 * 30);

  const active = checkPricelistActiveValidity(past, future);
  assert.equal(active.isActive, true);
  assert.equal(active.isExpired, false);

  const expired = checkPricelistActiveValidity(past, past);
  assert.equal(expired.isActive, false);
  assert.equal(expired.isExpired, true);
});

test('validateBatchImportSupplierRows - checks valid and invalid rows count', () => {
  const rows = [
    { supplierCode: 'SUP-01', supplierName: 'Supplier 1' },
    { supplierCode: '', supplierName: 'Invalid' },
  ];
  const res = validateBatchImportSupplierRows(rows);
  assert.equal(res.totalCount, 2);
  assert.equal(res.validCount, 1);
  assert.equal(res.invalidCount, 1);
});
