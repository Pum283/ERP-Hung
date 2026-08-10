import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateWorkCalendarForm,
  formatWeekMaskLabel,
  validateJobLevelForm,
  previewGeneratedEmployeeCode,
  validateEmployeeDocumentForm,
} from './hrm-work-level-code-doc-helpers.ts';

// ─── UC_HRM_006: validateWorkCalendarForm & formatWeekMaskLabel ───

test('validateWorkCalendarForm - valid 7-bit mask returns true', () => {
  const res = validateWorkCalendarForm({ code: 'OFFICE', name: 'Hành chính', weekMask: '1111100' });
  assert.equal(res.valid, true);
});

test('validateWorkCalendarForm - invalid mask length returns error', () => {
  const res = validateWorkCalendarForm({ code: 'OFFICE', name: 'Hành chính', weekMask: '1111' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('7 ký tự'));
});

test('formatWeekMaskLabel - formats active days correctly', () => {
  assert.equal(formatWeekMaskLabel('1111100'), 'T2, T3, T4, T5, T6');
  assert.equal(formatWeekMaskLabel('0000000'), 'Không có ngày làm việc');
});

// ─── UC_HRM_010: validateJobLevelForm ───

test('validateJobLevelForm - valid JobLevel returns true', () => {
  const res = validateJobLevelForm({ code: 'L1', name: 'Nhân viên', levelOrder: 1, defaultScopeType: 'Own' });
  assert.equal(res.valid, true);
});

test('validateJobLevelForm - negative levelOrder returns error', () => {
  const res = validateJobLevelForm({ code: 'L1', name: 'Nhân viên', levelOrder: -1, defaultScopeType: 'Own' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('LevelOrder'));
});

// ─── UC_HRM_012: previewGeneratedEmployeeCode ───

test('previewGeneratedEmployeeCode - formats EMP-{SEQ:4} correctly', () => {
  const code = previewGeneratedEmployeeCode('EMP-{SEQ:4}', 5);
  assert.equal(code, 'EMP-0005');
});

test('previewGeneratedEmployeeCode - replaces YYYY token correctly', () => {
  const code = previewGeneratedEmployeeCode('NV-{YYYY}-{SEQ:3}', 12);
  const currentYear = new Date().getFullYear().toString();
  assert.equal(code, `NV-${currentYear}-012`);
});

// ─── UC_HRM_017: validateEmployeeDocumentForm ───

test('validateEmployeeDocumentForm - valid IdCard returns true', () => {
  const res = validateEmployeeDocumentForm({
    docType: 'IdCard',
    title: 'CCCD',
    storageKey: 'key123',
    issuedOn: '2020-01-01',
    expiresOn: '2030-01-01',
  });
  assert.equal(res.valid, true);
});

test('validateEmployeeDocumentForm - expiresOn <= issuedOn returns error', () => {
  const res = validateEmployeeDocumentForm({
    docType: 'Passport',
    title: 'Hộ chiếu',
    storageKey: 'key123',
    issuedOn: '2025-01-01',
    expiresOn: '2024-01-01',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ExpiresOn'));
});

test('validateEmployeeDocumentForm - missing storageKey returns error', () => {
  const res = validateEmployeeDocumentForm({
    docType: 'Degree',
    title: 'Bằng ĐH',
    storageKey: '',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('StorageKey'));
});
