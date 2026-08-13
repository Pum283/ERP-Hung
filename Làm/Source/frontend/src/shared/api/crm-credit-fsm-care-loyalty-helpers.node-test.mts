import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateCreditLimitStatus,
  formatLoyaltyPointsDisplay,
  validateFsmTicketHandoff,
} from './crm-credit-fsm-care-loyalty-helpers.ts';

test('evaluateCreditLimitStatus - blocks orders exceeding credit limit', () => {
  const blocked = evaluateCreditLimitStatus(85000000, 100000000, 20000000);
  assert.equal(blocked.isBlocked, true);
  assert.equal(blocked.label.includes('CHẶN'), true);

  const approved = evaluateCreditLimitStatus(85000000, 100000000, 10000000);
  assert.equal(approved.isBlocked, false);
  assert.equal(approved.label.includes('DUYỆT'), true);
});

test('formatLoyaltyPointsDisplay - formats points with locale string', () => {
  assert.equal(formatLoyaltyPointsDisplay(0), '0 điểm');
  assert.equal(formatLoyaltyPointsDisplay(1250), '1.250 pts');
});

test('validateFsmTicketHandoff - checks ticket and technician IDs', () => {
  assert.equal(validateFsmTicketHandoff('', 'TECH-01').isValid, false);
  assert.equal(validateFsmTicketHandoff('TCK-100', '').isValid, false);
  assert.equal(validateFsmTicketHandoff('TCK-100', 'TECH-01').isValid, true);
});
