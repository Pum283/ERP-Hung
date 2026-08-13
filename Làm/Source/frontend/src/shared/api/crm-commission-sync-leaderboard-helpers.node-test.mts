import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateCommissionPeriodStatusBadge,
  formatLeaderboardRankBadge,
  validateCommissionPeriodForm,
} from './crm-commission-sync-leaderboard-helpers.ts';

test('evaluateCommissionPeriodStatusBadge - formats status badges correctly', () => {
  const synced = evaluateCommissionPeriodStatusBadge('SyncedToHrmFin');
  assert.equal(synced.label.toLowerCase().includes('đồng bộ'), true);

  const approved = evaluateCommissionPeriodStatusBadge('Approved');
  assert.equal(approved.label.includes('Đã duyệt'), true);
});

test('formatLeaderboardRankBadge - highlights top 3 ranks', () => {
  const r1 = formatLeaderboardRankBadge(1);
  assert.equal(r1.label.includes('Hạng 1'), true);

  const r5 = formatLeaderboardRankBadge(5);
  assert.equal(r5.label, 'Top 5');
});

test('validateCommissionPeriodForm - checks dates and code requirement', () => {
  assert.equal(validateCommissionPeriodForm('', '2026-08-01', '2026-08-31').isValid, false);
  assert.equal(validateCommissionPeriodForm('COMM-01', '2026-08-31', '2026-08-01').isValid, false);
  assert.equal(validateCommissionPeriodForm('COMM-01', '2026-08-01', '2026-08-31').isValid, true);
});
