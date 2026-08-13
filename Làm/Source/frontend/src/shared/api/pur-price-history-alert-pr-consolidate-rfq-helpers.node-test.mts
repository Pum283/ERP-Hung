import test from 'node:test';
import assert from 'node:assert/strict';
import {
  detectAbnormalPriceSpike,
  consolidateDemandsByProduct,
} from './pur-price-history-alert-pr-consolidate-rfq-helpers.ts';

test('detectAbnormalPriceSpike - detects abnormal purchase price increase percentage', () => {
  const s1 = detectAbnormalPriceSpike(24000, 28000, 10);
  assert.equal(s1.changePercent, 16.67);
  assert.equal(s1.isSpike, true);

  const s2 = detectAbnormalPriceSpike(215000, 220000, 10);
  assert.equal(s2.changePercent, 2.33);
  assert.equal(s2.isSpike, false);
});

test('consolidateDemandsByProduct - sums quantities of demands for the same product', () => {
  const demands = [
    { productId: 'p1', productCode: 'SKU-PAPER', productName: 'Giấy A4', qty: 10 },
    { productId: 'p1', productCode: 'SKU-PAPER', productName: 'Giấy A4', qty: 15 },
    { productId: 'p2', productCode: 'SKU-PEN', productName: 'Bút Bi', qty: 5 },
  ];

  const consolidated = consolidateDemandsByProduct(demands);
  assert.equal(consolidated.length, 2);
  assert.equal(consolidated[0].totalQty, 25);
  assert.equal(consolidated[1].totalQty, 5);
});
