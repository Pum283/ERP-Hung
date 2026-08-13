import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateLeadPriorityTier,
  generateClonedCampaignName,
  filterConversationsByChannel,
} from './crm-lead-campaign-inbox-helpers.ts';

test('evaluateLeadPriorityTier - returns Hot tier for score >= 80', () => {
  const res = evaluateLeadPriorityTier(85);
  assert.equal(res.priorityTier, 'Hot');
  assert.equal(res.badgeColorClass.includes('rose'), true);
});

test('evaluateLeadPriorityTier - returns Warm tier for score 50-79', () => {
  const res = evaluateLeadPriorityTier(65);
  assert.equal(res.priorityTier, 'Warm');
});

test('evaluateLeadPriorityTier - returns Cold tier for score < 50', () => {
  const res = evaluateLeadPriorityTier(30);
  assert.equal(res.priorityTier, 'Cold');
});

test('generateClonedCampaignName - appends (Bản sao) tag', () => {
  const name = generateClonedCampaignName('Chiến dịch Khuyến mại Q3');
  assert.equal(name, 'Chiến dịch Khuyến mại Q3 (Bản sao)');
});

test('filterConversationsByChannel - filters correctly by channel', () => {
  const list = [
    { channel: 'Zalo', customerName: 'Khách 1' },
    { channel: 'Facebook', customerName: 'Khách 2' },
  ];
  const filtered = filterConversationsByChannel(list, 'Zalo');
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].customerName, 'Khách 1');
});
