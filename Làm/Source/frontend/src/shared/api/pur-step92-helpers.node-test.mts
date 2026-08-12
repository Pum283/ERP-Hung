import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatAgingBucket,
  validatePurCsvExport,
  validateSkuCreate,
  validateItemGroup,
} from './pur-step92-helpers.ts';

test('UC_PUR_051: formatAgingBucket', () => {
  assert.equal(formatAgingBucket(15), '0 - 30 ngày');
  assert.equal(formatAgingBucket(45), '31 - 60 ngày');
  assert.equal(formatAgingBucket(100), 'Trên 90 ngày (> 90)');
});

test('UC_PUR_052: validatePurCsvExport', () => {
  const valid = validatePurCsvExport('by-vendor', 25);
  assert.equal(valid.canExport, true);

  const empty = validatePurCsvExport('by-vendor', 0);
  assert.equal(empty.canExport, false);
  assert.match(empty.reason!, /không có dữ liệu/);
});

test('UC_INV_001: validateSkuCreate', () => {
  const valid = validateSkuCreate('SKU-100', 'Bút Bi', 'CAI');
  assert.equal(valid.isValid, true);

  const noUom = validateSkuCreate('SKU-100', 'Bút Bi', '');
  assert.equal(noUom.isValid, false);
  assert.match(noUom.error!, /đơn vị tính/);
});

test('UC_INV_002: validateItemGroup', () => {
  const valid = validateItemGroup('NH-VPP', 'Văn Phòng Phẩm');
  assert.equal(valid.isValid, true);

  const emptyCode = validateItemGroup('', 'Nhóm A');
  assert.equal(emptyCode.isValid, false);
});
