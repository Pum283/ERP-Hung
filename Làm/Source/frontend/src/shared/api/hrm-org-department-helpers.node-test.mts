import test from 'node:test';
import assert from 'node:assert/strict';
import {
  buildDepartmentTree,
  validateCostCenterAllocation,
  filterEmergencyContacts,
  validateDepartmentForm,
  validateRelativeForm
} from './hrm-org-department-helpers.ts';
import type {
  FlatDepartment,
  CostCenterItem,
  RelativeItem
} from './hrm-org-department-helpers.ts';

test('buildDepartmentTree - correctly builds hierarchy and sorts nodes', () => {
  const departments: FlatDepartment[] = [
    { id: '1', code: 'HQ', name: 'Ban Giám Đốc', orgUnitId: 'ou1', sortOrder: 1, isActive: true },
    { id: '2', code: 'HR', name: 'Phòng Nhân Sự', parentId: '1', orgUnitId: 'ou1', sortOrder: 2, isActive: true },
    { id: '3', code: 'IT', name: 'Phòng IT', parentId: '1', orgUnitId: 'ou1', sortOrder: 1, isActive: true },
    { id: '4', code: 'DEV', name: 'Tổ Lập Trình', parentId: '3', orgUnitId: 'ou1', sortOrder: 1, isActive: true },
  ];

  const tree = buildDepartmentTree(departments);
  assert.equal(tree.length, 1);
  assert.equal(tree[0].code, 'HQ');
  assert.equal(tree[0].children.length, 2);
  assert.equal(tree[0].children[0].code, 'IT'); // sortOrder 1 comes before 2
  assert.equal(tree[0].children[0].children[0].code, 'DEV');
});

test('buildDepartmentTree - handles empty input gracefully', () => {
  const tree = buildDepartmentTree([]);
  assert.deepEqual(tree, []);
});

test('validateCostCenterAllocation - valid allocation total <= 100%', () => {
  const items: CostCenterItem[] = [
    { id: '1', code: 'CC1', name: 'CC 1', allocationPercentage: 50, isActive: true },
    { id: '2', code: 'CC2', name: 'CC 2', allocationPercentage: 30, isActive: true },
  ];
  const res = validateCostCenterAllocation(items);
  assert.equal(res.isValid, true);
  assert.equal(res.totalPercentage, 80);
});

test('validateCostCenterAllocation - invalid allocation total > 100%', () => {
  const items: CostCenterItem[] = [
    { id: '1', code: 'CC1', name: 'CC 1', allocationPercentage: 70, isActive: true },
    { id: '2', code: 'CC2', name: 'CC 2', allocationPercentage: 40, isActive: true },
  ];
  const res = validateCostCenterAllocation(items);
  assert.equal(res.isValid, false);
  assert.equal(res.totalPercentage, 110);
  assert.match(res.errorMessage || '', /vượt quá 100%/);
});

test('validateCostCenterAllocation - ignores inactive cost centers', () => {
  const items: CostCenterItem[] = [
    { id: '1', code: 'CC1', name: 'CC 1', allocationPercentage: 70, isActive: true },
    { id: '2', code: 'CC2', name: 'CC 2', allocationPercentage: 50, isActive: false },
  ];
  const res = validateCostCenterAllocation(items);
  assert.equal(res.isValid, true);
  assert.equal(res.totalPercentage, 70);
});

test('filterEmergencyContacts - filters and sorts emergency contacts only', () => {
  const relatives: RelativeItem[] = [
    { id: '1', employeeId: 'emp1', fullName: 'Bà C', relationship: 'Mother', isEmergencyContact: true, isTaxDependent: false },
    { id: '2', employeeId: 'emp1', fullName: 'Anh A', relationship: 'Spouse', isEmergencyContact: true, isTaxDependent: true },
    { id: '3', employeeId: 'emp1', fullName: 'Ông D', relationship: 'Father', isEmergencyContact: false, isTaxDependent: false },
  ];
  const list = filterEmergencyContacts(relatives);
  assert.equal(list.length, 2);
  assert.equal(list[0].fullName, 'Anh A'); // sorted alphabetically
  assert.equal(list[1].fullName, 'Bà C');
});

test('validateDepartmentForm - valid input passes', () => {
  const res = validateDepartmentForm({ code: 'DEPT_ACC', name: 'Phòng Kế Toán' });
  assert.equal(res.isValid, true);
});

test('validateDepartmentForm - empty code or name fails', () => {
  const res1 = validateDepartmentForm({ code: '', name: 'Phòng Kế Toán' });
  assert.equal(res1.isValid, false);
  assert.equal(res1.error, 'Mã bộ phận không được để trống.');

  const res2 = validateDepartmentForm({ code: 'DEPT_ACC', name: ' ' });
  assert.equal(res2.isValid, false);
  assert.equal(res2.error, 'Tên bộ phận không được để trống.');
});

test('validateDepartmentForm - self parent fails', () => {
  const res = validateDepartmentForm({ code: 'D1', name: 'Dept 1', id: 'd1', parentId: 'd1' });
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Bộ phận cha không thể là chính nó.');
});

test('validateRelativeForm - phone and relationship validation', () => {
  const valid = validateRelativeForm({ employeeId: 'emp1', fullName: 'Nguyễn Văn B', relationship: 'Spouse', phone: '0912345678' });
  assert.equal(valid.isValid, true);

  const invalidPhone = validateRelativeForm({ employeeId: 'emp1', fullName: 'Nguyễn Văn B', relationship: 'Spouse', phone: 'abc' });
  assert.equal(invalidPhone.isValid, false);
  assert.equal(invalidPhone.error, 'Số điện thoại không đúng định dạng.');

  const invalidRel = validateRelativeForm({ employeeId: 'emp1', fullName: 'Nguyễn Văn B', relationship: 'Unknown' });
  assert.equal(invalidRel.isValid, false);
  assert.equal(invalidRel.error, 'Mối quan hệ không hợp lệ.');
});
