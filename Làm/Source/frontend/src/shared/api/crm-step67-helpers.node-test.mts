import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatLeadScoreBadge,
  formatPipelineStageStep,
  validateLeadTaskInput,
  formatTaskReminderNotice,
} from './crm-step67-helpers.ts';

// ─── UC_CRM_052: formatLeadScoreBadge ───

test('formatLeadScoreBadge - returns Hot, Warm, Cold tiers correctly', () => {
  const hot = formatLeadScoreBadge(85);
  assert.equal(hot.tier, 'Hot');
  assert.ok(hot.label.includes('Hot Lead'));

  const warm = formatLeadScoreBadge(60);
  assert.equal(warm.tier, 'Warm');

  const cold = formatLeadScoreBadge(20);
  assert.equal(cold.tier, 'Cold');
});

// ─── UC_CRM_053: formatPipelineStageStep ───

test('formatPipelineStageStep - returns correct step details for pipeline stages', () => {
  const stepNew = formatPipelineStageStep('New');
  assert.equal(stepNew.stepNumber, 1);

  const stepQualified = formatPipelineStageStep('Qualified');
  assert.equal(stepQualified.stepNumber, 3);
});

// ─── UC_CRM_054: validateLeadTaskInput ───

test('validateLeadTaskInput - valid task title and due date returns isValid true', () => {
  const res = validateLeadTaskInput({ title: 'Tư vấn báo giá', dueAt: new Date(Date.now() + 86400000) });
  assert.equal(res.isValid, true);
});

test('validateLeadTaskInput - missing title returns validation error', () => {
  const res = validateLeadTaskInput({ title: '', dueAt: new Date() });
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Tiêu đề công việc là bắt buộc'));
});

test('validateLeadTaskInput - missing due date returns validation error', () => {
  const res = validateLeadTaskInput({ title: 'Họp với khách hàng' });
  assert.equal(res.isValid, false);
  assert.ok(res.error?.includes('Hạn hoàn thành công việc là bắt buộc'));
});

// ─── UC_CRM_055: formatTaskReminderNotice ───

test('formatTaskReminderNotice - returns overdue warning and reminder notice strings', () => {
  const completedNotice = formatTaskReminderNotice(true, new Date(), 'Completed');
  assert.ok(completedNotice.includes('đã hoàn thành'));

  const overdueNotice = formatTaskReminderNotice(true, new Date(Date.now() - 3600000), 'Open');
  assert.ok(overdueNotice.includes('QUÁ HẠN'));

  const reminderNotice = formatTaskReminderNotice(true, new Date(Date.now() + 7200000), 'Open');
  assert.ok(reminderNotice.includes('Bật nhắc nhở'));
});
