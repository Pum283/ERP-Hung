import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatOffsetBalanceSummary,
  formatDunningLevelBadge,
} from './fin-statement-offset-dunning-bad-debt-helpers.ts';

test('formatOffsetBalanceSummary - formats AR and AP offset summary', () => {
  assert.equal(
    formatOffsetBalanceSummary(65000000, 65000000, 0),
    'AR: 65.000.000 đ ↔ AP: 65.000.000 đ | Chênh lệch: 0 đ'
  );
});

test('formatDunningLevelBadge - returns correct badge classes for levels', () => {
  assert.equal(formatDunningLevelBadge('Level1_Reminder'), 'bg-amber-100 text-amber-800 border-amber-300');
  assert.equal(formatDunningLevelBadge('Level2_Warning'), 'bg-orange-100 text-orange-800 border-orange-300');
  assert.equal(formatDunningLevelBadge('Level3_LegalNotice'), 'bg-rose-100 text-rose-800 border-rose-300');
});
