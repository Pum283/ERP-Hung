import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateSlaStatus,
  validateRoutingRule,
  parseBotFlowSteps,
} from './crm-omnichannel-routing-sla-helpers.ts';

test('evaluateSlaStatus - returns Vi phạm SLA when actual > max', () => {
  const res = evaluateSlaStatus(5, 8);
  assert.equal(res.isBreached, true);
  assert.equal(res.statusText, 'Vi phạm SLA');
  assert.equal(res.badgeClass.includes('rose'), true);
});

test('evaluateSlaStatus - returns Cảnh báo sắp trễ when actual near max', () => {
  const res = evaluateSlaStatus(5, 4);
  assert.equal(res.isBreached, false);
  assert.equal(res.statusText, 'Cảnh báo sắp trễ');
});

test('evaluateSlaStatus - returns Phản hồi tốt when actual is fast', () => {
  const res = evaluateSlaStatus(5, 1);
  assert.equal(res.isBreached, false);
  assert.equal(res.statusText, 'Phản hồi tốt');
});

test('validateRoutingRule - validates rule name and strategy', () => {
  assert.equal(validateRoutingRule('', 'RoundRobin').isValid, false);
  assert.equal(validateRoutingRule('Rule 1', 'InvalidStrategy').isValid, false);
  assert.equal(validateRoutingRule('Rule 1', 'SkillBased').isValid, true);
});

test('parseBotFlowSteps - parses JSON flow steps safely', () => {
  const json = '[{"step":1,"action":"send_msg","text":"Xin chào"}]';
  const steps = parseBotFlowSteps(json);
  assert.equal(steps.length, 1);
  assert.equal(steps[0].text, 'Xin chào');
});
