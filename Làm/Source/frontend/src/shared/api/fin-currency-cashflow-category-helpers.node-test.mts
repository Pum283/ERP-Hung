import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatExchangeRate,
  formatCashFlowTypeBadge,
} from './fin-currency-cashflow-category-helpers.ts';

test('formatExchangeRate - formats exchange rate with base currency check', () => {
  assert.equal(formatExchangeRate(1, 'VND'), '1 VNĐ (Đồng Tiền Cơ Sở)');
  assert.equal(formatExchangeRate(25450, 'USD'), '1 USD = 25.450 VNĐ');
});

test('formatCashFlowTypeBadge - returns correct badge classes', () => {
  assert.equal(formatCashFlowTypeBadge('Inflow'), 'bg-emerald-100 text-emerald-800 border-emerald-300');
  assert.equal(formatCashFlowTypeBadge('Outflow'), 'bg-rose-100 text-rose-800 border-rose-300');
});
