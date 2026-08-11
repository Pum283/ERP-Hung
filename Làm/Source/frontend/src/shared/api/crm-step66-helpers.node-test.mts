import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatChatChannelBadge,
  validateManualLeadInput,
  formatLeadStatusBadge,
  formatSalesOwnerSummary,
} from './crm-step66-helpers.ts';

// ─── UC_CRM_047: formatChatChannelBadge ───

test('formatChatChannelBadge - inbound and outbound chat return correct labels', () => {
  const inChat = formatChatChannelBadge('Facebook', 'Inbound');
  assert.equal(inChat.isInbound, true);
  assert.ok(inChat.label.includes('Đợi phản hồi'));

  const outChat = formatChatChannelBadge('Zalo', 'Outbound');
  assert.equal(outChat.isInbound, false);
  assert.ok(outChat.label.includes('Đã trả lời'));
});

// ─── UC_CRM_049: validateManualLeadInput ───

test('validateManualLeadInput - valid lead with phone returns isValid true', () => {
  const res = validateManualLeadInput({ name: 'Nguyễn Văn Lead', phone: '0901234567' });
  assert.equal(res.isValid, true);
});

test('validateManualLeadInput - missing both phone and email returns validation error', () => {
  const res = validateManualLeadInput({ name: 'Lead Không Liên Hệ' });
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('ít nhất Số điện thoại hoặc Email'));
});

test('validateManualLeadInput - invalid phone format returns error', () => {
  const res = validateManualLeadInput({ name: 'Lead SĐT Sai', phone: 'abc1234' });
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Số điện thoại'));
});

// ─── UC_CRM_050: formatLeadStatusBadge ───

test('formatLeadStatusBadge - returns correct status badge for New and Qualified', () => {
  assert.ok(formatLeadStatusBadge('New').label.includes('Mới tiếp nhận'));
  assert.ok(formatLeadStatusBadge('Qualified').label.includes('Đủ điều kiện'));
});

// ─── UC_CRM_051: formatSalesOwnerSummary ───

test('formatSalesOwnerSummary - unassigned and assigned lead owner string', () => {
  const unassigned = formatSalesOwnerSummary();
  assert.ok(unassigned.includes('Chưa phân bổ'));

  const assigned = formatSalesOwnerSummary('Trần Sales Rep');
  assert.ok(assigned.includes('Trần Sales Rep'));
});
