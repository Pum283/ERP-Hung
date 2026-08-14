import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatFrequencyLabel,
  formatCompletionRate,
} from './fsm-equipment-maintenance-helpers.ts';

test('formatFrequencyLabel - maps maintenance schedule frequency', () => {
  assert.equal(formatFrequencyLabel('Monthly'), 'Hàng Tháng');
  assert.equal(formatFrequencyLabel('Quarterly'), 'Hàng Quý');
  assert.equal(formatFrequencyLabel('Annual'), 'Hàng Năm');
});

test('formatCompletionRate - formats on-time rate with suffix', () => {
  assert.equal(formatCompletionRate(95.83), '95.8% Đúng Hạn');
});
