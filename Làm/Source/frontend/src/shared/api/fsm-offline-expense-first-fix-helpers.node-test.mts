import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatFtfrPercentage,
  formatSettlementNet,
} from './fsm-offline-expense-first-fix-helpers.ts';

test('formatFtfrPercentage - formats first-time fix rate', () => {
  assert.equal(formatFtfrPercentage(90.0), '90.0% FTFR');
});

test('formatSettlementNet - formats net settlement amount', () => {
  assert.equal(formatSettlementNet(2150000), '2.150.000 đ Quyết Toán');
});
