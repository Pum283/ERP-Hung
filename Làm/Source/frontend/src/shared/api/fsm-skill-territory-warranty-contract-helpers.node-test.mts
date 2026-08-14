import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatWarrantyDaysRemaining,
  formatContractValue,
} from './fsm-skill-territory-warranty-contract-helpers.ts';

test('formatWarrantyDaysRemaining - formats days remaining string', () => {
  assert.equal(formatWarrantyDaysRemaining(15), 'Còn 15 ngày bảo hành');
  assert.equal(formatWarrantyDaysRemaining(0), 'Đã Hết Hạn Bảo Hành');
});

test('formatContractValue - formats contract value in VND per year', () => {
  assert.equal(formatContractValue(120000000), '120.000.000 đ / Năm');
});
