import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateFinSyncRequest,
  formatHourlyRevenueRow,
  formatProductRevenueRow,
  formatCashierRevenueRow,
} from './pos-step84-helpers.ts';

test('UC_POS_059: validateFinSyncRequest', () => {
  const valid = validateFinSyncRequest('Closed', 5);
  assert.equal(valid.canSync, true);

  const openShift = validateFinSyncRequest('Open', 5);
  assert.equal(openShift.canSync, false);
  assert.match(openShift.reason!, /Chỉ có thể đồng bộ/);

  const zeroPaid = validateFinSyncRequest('Closed', 0);
  assert.equal(zeroPaid.canSync, false);
  assert.match(zeroPaid.reason!, /không có đơn/);
});

test('UC_POS_061: formatHourlyRevenueRow', () => {
  const row = formatHourlyRevenueRow('09:00 - 10:00', 8, 1200000);
  assert.match(row, /09:00 - 10:00/);
  assert.match(row, /8 đơn/);
  assert.match(row, /1.200.000/);
});

test('UC_POS_062: formatProductRevenueRow', () => {
  const row = formatProductRevenueRow('P01', 'Cà phê đá', 25, 750000);
  assert.match(row, /P01/);
  assert.match(row, /Cà phê đá/);
  assert.match(row, /25/);
  assert.match(row, /750.000/);
});

test('UC_POS_063: formatCashierRevenueRow', () => {
  const row = formatCashierRevenueRow('Nguyễn Văn A', 10, 2000000);
  assert.match(row, /Nguyễn Văn A/);
  assert.match(row, /10/);
  assert.match(row, /2.000.000/);
  assert.match(row, /200.000/);
});
