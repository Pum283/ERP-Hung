import test from 'node:test';
import assert from 'node:assert/strict';
import {
  getLotTraceDirectionLabel,
  getStocktakeLockStatusPill,
} from './inv-lot-trace-stocktake-lock-internal-request-helpers.ts';

test('getLotTraceDirectionLabel - maps forward and backward lot trace direction correctly', () => {
  const fwd = getLotTraceDirectionLabel('Forward');
  assert.equal(fwd.label, 'Truy Vết Xuôi (NCC ➔ SX ➔ Khách Hàng)');
  assert.match(fwd.colorClass, /bg-emerald/);

  const bwd = getLotTraceDirectionLabel('Backward');
  assert.equal(bwd.label, 'Truy Vết Ngược (Khách Hàng ➔ SX ➔ NCC)');
  assert.match(bwd.colorClass, /bg-blue/);
});

test('getStocktakeLockStatusPill - returns lock indicator with proper styling', () => {
  const locked = getStocktakeLockStatusPill(true);
  assert.equal(locked.label, '🔒 ĐANG KHÓA GIAO DỊCH (KIỂM KÊ)');
  assert.match(locked.colorClass, /bg-rose/);

  const unlocked = getStocktakeLockStatusPill(false);
  assert.equal(unlocked.label, '🔓 ĐANG MỞ (GIAO DỊCH BÌNH THƯỜNG)');
  assert.match(unlocked.colorClass, /bg-emerald/);
});
