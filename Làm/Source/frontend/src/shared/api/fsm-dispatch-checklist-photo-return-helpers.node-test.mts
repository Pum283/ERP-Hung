import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatPhotoTypeLabel,
  formatReturnedPartQuantity,
} from './fsm-dispatch-checklist-photo-return-helpers.ts';

test('formatPhotoTypeLabel - maps photo type correctly', () => {
  assert.equal(formatPhotoTypeLabel('Before'), '📷 Ảnh Trước Khi Sửa');
  assert.equal(formatPhotoTypeLabel('After'), '📸 Ảnh Sau Khi Nghiệm Thu');
  assert.equal(formatPhotoTypeLabel('Other'), '📦 Ảnh Linh Kiện Thay Thế');
});

test('formatReturnedPartQuantity - formats returned spare parts quantity', () => {
  assert.equal(formatReturnedPartQuantity(3), '3 Linh Kiện Hoàn Kho');
});
