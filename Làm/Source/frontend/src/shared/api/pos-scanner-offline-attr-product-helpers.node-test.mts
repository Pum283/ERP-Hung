import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateOfflineSyncBadgeStatus,
  validateScannerConfigForm,
} from './pos-scanner-offline-attr-product-helpers.ts';

test('evaluateOfflineSyncBadgeStatus - formats offline sync badge states', () => {
  const synced = evaluateOfflineSyncBadgeStatus('Synced');
  assert.equal(synced.label.includes('hoàn tất'), true);

  const pending = evaluateOfflineSyncBadgeStatus('Pending');
  assert.equal(pending.label.includes('Chờ đồng bộ'), true);
});

test('validateScannerConfigForm - checks scanner name requirement', () => {
  assert.equal(validateScannerConfigForm('').isValid, false);
  assert.equal(validateScannerConfigForm('Honeywell 1950g').isValid, true);
});
