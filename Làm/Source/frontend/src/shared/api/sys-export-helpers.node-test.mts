import test from 'node:test';
import assert from 'node:assert/strict';
import { validateLinkFileForm, validateExportRequest, getJobStatusLabel, getJobStatusColor } from './sys-export-helpers.ts';

test('validateLinkFileForm - Empty fileId returns error', () => {
  const res = validateLinkFileForm({ fileId: '', entityType: 'Order', entityId: 'abc' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'FileId không được để trống.');
});

test('validateLinkFileForm - Empty entityType returns error', () => {
  const res = validateLinkFileForm({ fileId: 'id-1', entityType: '  ', entityId: 'abc' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Loại đối tượng (EntityType) không được để trống.');
});

test('validateLinkFileForm - Valid input returns valid true', () => {
  const res = validateLinkFileForm({ fileId: 'fid', entityType: 'Customer', entityId: 'eid' });
  assert.equal(res.valid, true);
  assert.equal(res.error, undefined);
});

test('validateExportRequest - Empty entityType returns error', () => {
  const res = validateExportRequest({ entityType: '', format: 'Csv' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Loại đối tượng xuất (EntityType) không được để trống.');
});

test('validateExportRequest - Invalid format returns error', () => {
  const res = validateExportRequest({ entityType: 'Users', format: 'Xlsx' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Csv'));
});

test('validateExportRequest - Valid Csv format returns valid true', () => {
  const res = validateExportRequest({ entityType: 'Users', format: 'Csv' });
  assert.equal(res.valid, true);
});

test('validateExportRequest - Valid Pdf format returns valid true', () => {
  const res = validateExportRequest({ entityType: 'Users', format: 'Pdf' });
  assert.equal(res.valid, true);
});

test('getJobStatusLabel - Maps known statuses', () => {
  assert.equal(getJobStatusLabel('Completed'), '✅ Hoàn thành');
  assert.equal(getJobStatusLabel('Failed'), '❌ Thất bại');
  assert.equal(getJobStatusLabel('Running'), '🔄 Đang xử lý');
});

test('getJobStatusColor - Maps known statuses to colors', () => {
  assert.equal(getJobStatusColor('Completed'), '#10b981');
  assert.equal(getJobStatusColor('Failed'), '#ef4444');
  assert.equal(getJobStatusColor('Unknown'), '#6b7280');
});
