import test from 'node:test';
import assert from 'node:assert/strict';
import {
  getInternalTransferStatusBadge,
  formatGpsCoordinates,
} from './log-realtime-gps-internal-transfer-helpers.ts';

test('getInternalTransferStatusBadge - maps status correctly', () => {
  const received = getInternalTransferStatusBadge('Received');
  assert.equal(received.label, 'Đã Nhận Đủ');
  assert.match(received.colorClass, /bg-emerald/);

  const diff = getInternalTransferStatusBadge('DiscrepancyReported');
  assert.equal(diff.label, 'Lệch Số Lượng (Cần Đối Soát)');
  assert.match(diff.colorClass, /bg-rose/);
});

test('formatGpsCoordinates - formats latitude and longitude to 4 decimal places', () => {
  assert.equal(formatGpsCoordinates(10.776888, 106.700888), '10.7769° N, 106.7009° E');
});
