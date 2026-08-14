import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatAdvanceRefundSummary,
  formatFrequencyLabel,
} from './fin-recurring-voucher-advance-vault-helpers.ts';

test('formatAdvanceRefundSummary - formats advance settlement summary', () => {
  assert.equal(
    formatAdvanceRefundSummary(15000000, 14200000, 800000),
    'Tạm ứng: 15.000.000 đ | Thực chi: 14.200.000 đ | Hoàn quỹ: 800.000 đ'
  );
});

test('formatFrequencyLabel - converts frequency code to Vietnamese label', () => {
  assert.equal(formatFrequencyLabel('Monthly'), 'Hàng Tháng');
  assert.equal(formatFrequencyLabel('Quarterly'), 'Hàng Quý');
  assert.equal(formatFrequencyLabel('Annual'), 'Hàng Năm');
});
