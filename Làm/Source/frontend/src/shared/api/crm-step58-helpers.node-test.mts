import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatSegmentBadge,
  validateMergeInput,
  formatOwnerStatus,
  formatHandoverNote,
} from './crm-step58-helpers.ts';

// ─── UC_CRM_006: formatSegmentBadge ───

test('formatSegmentBadge - returns correct labels and badge classes', () => {
  assert.equal(formatSegmentBadge('Lead').label.includes('Lead'), true);
  assert.equal(formatSegmentBadge('Customer').label.includes('Khách hàng'), true);
  assert.equal(formatSegmentBadge('Partner').label.includes('Đối tác'), true);
  assert.equal(formatSegmentBadge('Unknown').label.includes('Chưa phân loại'), true);
});

// ─── UC_CRM_005: validateMergeInput ───

test('validateMergeInput - valid distinct customer IDs returns valid', () => {
  const res = validateMergeInput('cust-id-1', 'cust-id-2');
  assert.equal(res.isValid, true);
  assert.equal(res.error, undefined);
});

test('validateMergeInput - same customer ID returns error', () => {
  const res = validateMergeInput('cust-id-1', 'cust-id-1');
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('không được trùng nhau'));
});

test('validateMergeInput - empty customer ID returns error', () => {
  const res = validateMergeInput('cust-id-1', '');
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Vui lòng chọn cả'));
});

// ─── UC_CRM_008: formatOwnerStatus ───

test('formatOwnerStatus - unassigned owner returns warning label', () => {
  assert.ok(formatOwnerStatus().includes('Chưa gán'));
  assert.ok(formatOwnerStatus('  ').includes('Chưa gán'));
});

test('formatOwnerStatus - assigned owner returns formatted name', () => {
  assert.ok(formatOwnerStatus('Nguyễn Văn A').includes('Phụ trách: Nguyễn Văn A'));
});

// ─── UC_CRM_009: formatHandoverNote ───

test('formatHandoverNote - formats handover message with reason note', () => {
  const note = formatHandoverNote('Sales 1', 'Sales 2', 'Chuyển vùng phủ sóng');
  assert.ok(note.includes('Sales 1'));
  assert.ok(note.includes('Sales 2'));
  assert.ok(note.includes('Chuyển vùng phủ sóng'));
});
