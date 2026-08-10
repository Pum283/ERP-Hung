import test from 'node:test';
import assert from 'node:assert/strict';
import { validateDepartmentForm, validateJobLevelForm, formatLocaleDate } from './sys-orgstruct-helpers.ts';

test('validateDepartmentForm - Empty code returns error', () => {
  const res = validateDepartmentForm({ code: '', name: 'Phòng Kế Toán', orgUnitId: 'org-1' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Mã phòng ban không được để trống.');
});

test('validateDepartmentForm - Missing OrgUnit returns error', () => {
  const res = validateDepartmentForm({ code: 'D_KT', name: 'Phòng Kế Toán', orgUnitId: '' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Chi nhánh gán vào phòng ban không được để trống.');
});

test('validateDepartmentForm - Self parent returns error', () => {
  const res = validateDepartmentForm({ id: 'dept-1', parentId: 'dept-1', code: 'D_KT', name: 'Phòng Kế Toán', orgUnitId: 'org-1' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Phòng ban không thể chọn chính nó làm đơn vị cấp trên.');
});

test('validateJobLevelForm - Negative order returns error', () => {
  const res = validateJobLevelForm({ code: 'JL_1', name: 'Nhân viên', levelOrder: -1 });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Thứ tự cấp bậc phải lớn hơn hoặc bằng 0.');
});

test('validateJobLevelForm - Valid data returns valid true', () => {
  const res = validateJobLevelForm({ code: 'JL_1', name: 'Trưởng phòng', levelOrder: 2 });
  assert.equal(res.valid, true);
  assert.equal(res.error, undefined);
});

test('formatLocaleDate - Formats ISO string into dd/MM/yyyy pattern', () => {
  const formatted = formatLocaleDate('2026-08-10T00:00:00Z', 'dd/MM/yyyy');
  assert.equal(formatted, '10/08/2026');
});
