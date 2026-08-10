import test from 'node:test';
import assert from 'node:assert/strict';
import { filterAccessibleDepartments, validateOrgUnitForm, buildOrgUnitTree } from './sys-datascope-helpers.ts';
import type { OrgUnitNode, DepartmentNode } from './sys-datascope-helpers.ts';

test('validateOrgUnitForm - Empty code returns error', () => {
  const res = validateOrgUnitForm({ code: '  ', name: 'Chi nhánh 1' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Mã chi nhánh không được để trống.');
});

test('validateOrgUnitForm - Self parent returns error', () => {
  const res = validateOrgUnitForm({ id: 'org-1', parentId: 'org-1', code: 'CN_1', name: 'Chi nhánh 1' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Chi nhánh không thể làm đơn vị cấp trên của chính nó.');
});

test('validateOrgUnitForm - Valid form returns valid true', () => {
  const res = validateOrgUnitForm({ id: 'org-2', parentId: 'org-1', code: 'CN_2', name: 'Chi nhánh 2' });
  assert.equal(res.valid, true);
  assert.equal(res.error, undefined);
});

test('filterAccessibleDepartments - Bypass returns all departments', () => {
  const depts: DepartmentNode[] = [
    { id: 'd1', code: 'D1', name: 'Kế toán', parentId: null, path: '/d1/', isActive: true },
    { id: 'd2', code: 'D2', name: 'Nhân sự', parentId: null, path: '/d2/', isActive: true }
  ];
  const filtered = filterAccessibleDepartments(depts, ['d1'], true);
  assert.equal(filtered.length, 2);
});

test('filterAccessibleDepartments - Restricts to accessible IDs', () => {
  const depts: DepartmentNode[] = [
    { id: 'd1', code: 'D1', name: 'Kế toán', parentId: null, path: '/d1/', isActive: true },
    { id: 'd2', code: 'D2', name: 'Nhân sự', parentId: null, path: '/d2/', isActive: true }
  ];
  const filtered = filterAccessibleDepartments(depts, ['d1'], false);
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].id, 'd1');
});

test('buildOrgUnitTree - Constructs parent-child tree hierarchy', () => {
  const units: OrgUnitNode[] = [
    { id: 'u1', code: 'HO', name: 'Hội sở', parentId: null, unitType: 'Company', isActive: true, path: '/u1/' },
    { id: 'u2', code: 'CN1', name: 'Chi nhánh 1', parentId: 'u1', unitType: 'Branch', isActive: true, path: '/u1/u2/' }
  ];
  const tree = buildOrgUnitTree(units);
  assert.equal(tree.length, 1);
  assert.equal(tree[0].id, 'u1');
  assert.equal(tree[0].children.length, 1);
  assert.equal(tree[0].children[0].id, 'u2');
});
