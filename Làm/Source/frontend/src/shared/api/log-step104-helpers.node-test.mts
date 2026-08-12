import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateValuationCsvExport,
  validateCarrierUpsert,
  validateDeliveryOrderCreate,
  validateBatchSplit,
} from './log-step104-helpers.ts';

test('UC_INV_070: validateValuationCsvExport', () => {
  const exportCheck = validateValuationCsvExport('WH-01');
  assert.equal(exportCheck.canExport, true);
});

test('UC_LOG_001: validateCarrierUpsert', () => {
  const valid = validateCarrierUpsert('GHN', 'Giao Hàng Nhanh');
  assert.equal(valid.isValid, true);

  const noName = validateCarrierUpsert('GHN', '');
  assert.equal(noName.isValid, false);
  assert.match(noName.error!, /Tên ĐVVC/);
});

test('UC_LOG_006: validateDeliveryOrderCreate', () => {
  const valid = validateDeliveryOrderCreate('SO-2026-001', 'Nguyễn Văn A');
  assert.equal(valid.canCreate, true);

  const noSo = validateDeliveryOrderCreate('', 'Nguyễn Văn A');
  assert.equal(noSo.canCreate, false);
});

test('UC_LOG_008: validateBatchSplit', () => {
  const valid = validateBatchSplit(3, 10);
  assert.equal(valid.canSplit, true);

  const overflow = validateBatchSplit(10, 10);
  assert.equal(overflow.canSplit, false);
  assert.match(overflow.error!, /nhỏ hơn/);
});
