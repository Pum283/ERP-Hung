import test from 'node:test';
import assert from 'node:assert/strict';
import { validateFileUploadForm, validateNotificationRuleForm, formatFileSize } from './sys-file-helpers.ts';

test('validateFileUploadForm - Empty filename returns error', () => {
  const res = validateFileUploadForm({ fileName: '', sizeBytes: 1024 });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Tên file không được để trống.');
});

test('validateFileUploadForm - Zero byte size returns error', () => {
  const res = validateFileUploadForm({ fileName: 'empty.txt', sizeBytes: 0 });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Kích thước file phải lớn hơn 0 byte.');
});

test('validateFileUploadForm - File size exceeds 50MB returns error', () => {
  const res = validateFileUploadForm({ fileName: 'video.mp4', sizeBytes: 55 * 1024 * 1024 });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Kích thước file không được vượt quá 50MB.');
});

test('validateNotificationRuleForm - Empty event type returns error', () => {
  const res = validateNotificationRuleForm({ eventType: '', titleTemplate: 'Title', bodyTemplate: 'Body' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Mã sự kiện không được để trống.');
});

test('validateNotificationRuleForm - Valid rule returns valid true', () => {
  const res = validateNotificationRuleForm({ eventType: 'ORDER_APPROVED', titleTemplate: 'Đơn hàng đã duyệt', bodyTemplate: 'Nội dung' });
  assert.equal(res.valid, true);
  assert.equal(res.error, undefined);
});

test('formatFileSize - Formats bytes to human readable string', () => {
  assert.equal(formatFileSize(500), '500.00 B');
  assert.equal(formatFileSize(1024 * 1.5), '1.50 KB');
  assert.equal(formatFileSize(1024 * 1024 * 5), '5.00 MB');
});
