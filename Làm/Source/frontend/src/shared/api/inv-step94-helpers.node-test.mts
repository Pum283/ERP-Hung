import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateCsvImportFile,
  validateWarehouseCreate,
  validateKeeperAssignment,
  validatePickingStrategy,
} from './inv-step94-helpers.ts';

test('UC_INV_008: validateCsvImportFile', () => {
  const valid = validateCsvImportFile('skus.csv', 1024);
  assert.equal(valid.canImport, true);

  const txtFile = validateCsvImportFile('skus.txt', 1024);
  assert.equal(txtFile.canImport, false);
  assert.match(txtFile.reason!, /\.csv/);

  const emptyFile = validateCsvImportFile('skus.csv', 0);
  assert.equal(emptyFile.canImport, false);
});

test('UC_INV_011: validateWarehouseCreate', () => {
  const valid = validateWarehouseCreate('KHO-TONG', 'Kho Tổng Miền Bắc');
  assert.equal(valid.isValid, true);

  const emptyCode = validateWarehouseCreate('', 'Kho A');
  assert.equal(emptyCode.isValid, false);
});

test('UC_INV_014: validateKeeperAssignment', () => {
  const valid = validateKeeperAssignment('WH-01', 'USER-01');
  assert.equal(valid.isValid, true);

  const noUser = validateKeeperAssignment('WH-01', '');
  assert.equal(noUser.isValid, false);
});

test('UC_INV_015: validatePickingStrategy', () => {
  const fefo = validatePickingStrategy('FEFO');
  assert.equal(fefo.isValid, true);

  const invalid = validatePickingStrategy('UNKNOWN');
  assert.equal(invalid.isValid, false);
  assert.match(invalid.error!, /FEFO, FIFO/);
});
