import test from 'node:test';
import assert from 'node:assert/strict';
import {
  rankQuotationsByLowestPrice,
  validatePoConfirmationStatus,
} from './pur-quotation-compare-award-po-confirm-helpers.ts';

test('rankQuotationsByLowestPrice - ranks quotations by total price ascending', () => {
  const quotations = [
    { id: 'q1', supplierName: 'Mộc Châu Milk', totalAmountVnd: 25500000, leadTimeDays: 5 },
    { id: 'q2', supplierName: 'Vinamilk Co.', totalAmountVnd: 24000000, leadTimeDays: 3 },
  ];

  const ranked = rankQuotationsByLowestPrice(quotations);
  assert.equal(ranked[0].supplierName, 'Vinamilk Co.');
  assert.equal(ranked[0].rank, 1);
  assert.equal(ranked[0].isBestValue, true);
  assert.equal(ranked[1].rank, 2);
});

test('validatePoConfirmationStatus - validates vendor PO confirmation status', () => {
  assert.equal(validatePoConfirmationStatus('Confirmed').isConfirmed, true);
  assert.equal(validatePoConfirmationStatus('ConfirmedWithChanges').requiresReview, true);
  assert.equal(validatePoConfirmationStatus('Rejected').requiresReview, true);
});
