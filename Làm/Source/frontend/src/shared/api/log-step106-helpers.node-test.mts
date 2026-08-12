import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateStatusUpdate,
  formatDeliveryFailureReason,
  validateCodAmount,
  validateCodCollectConfirmation,
} from './log-step106-helpers.ts';

test('UC_LOG_014: validateStatusUpdate', () => {
  const valid = validateStatusUpdate('InTransit');
  assert.equal(valid.isValid, true);

  const invalid = validateStatusUpdate('UnknownStatus');
  assert.equal(invalid.isValid, false);
});

test('UC_LOG_017: formatDeliveryFailureReason', () => {
  const reason = formatDeliveryFailureReason('Sai địa chỉ');
  assert.match(reason.formattedReason, /Sai địa chỉ/);
});

test('UC_LOG_021: validateCodAmount', () => {
  const valid = validateCodAmount(500000);
  assert.equal(valid.isValid, true);

  const zero = validateCodAmount(0);
  assert.equal(zero.isValid, false);
});

test('UC_LOG_022: validateCodCollectConfirmation', () => {
  const valid = validateCodCollectConfirmation('Pending');
  assert.equal(valid.canCollect, true);

  const collected = validateCodCollectConfirmation('Collected');
  assert.equal(collected.canCollect, false);
});
