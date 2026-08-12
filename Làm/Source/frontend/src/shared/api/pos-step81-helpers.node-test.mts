import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateInvoicePrintRequest,
  formatInvoiceHeader,
  validateCancelLineRequest,
  validateCancelBillRequest,
  validateReturnRefundRequest,
  formatRefundSummary,
} from './pos-step81-helpers.ts';

test('UC_POS_037: validateInvoicePrintRequest - valid completed sale', () => {
  const res = validateInvoicePrintRequest('Completed', 3);
  assert.equal(res.canPrint, true);
});

test('UC_POS_037: validateInvoicePrintRequest - invalid status or empty items', () => {
  const res1 = validateInvoicePrintRequest('Open', 3);
  assert.equal(res1.canPrint, false);
  assert.match(res1.reason!, /Chỉ có thể in hóa đơn chính thức/);

  const res2 = validateInvoicePrintRequest('Completed', 0);
  assert.equal(res2.canPrint, false);
  assert.match(res2.reason!, /rỗng/);
});

test('UC_POS_037: formatInvoiceHeader', () => {
  const header = formatInvoiceHeader('Cửa hàng POS 1', 'POS-SALE-001');
  assert.equal(header.includes('CỬA HÀNG POS 1'), true);
  assert.equal(header.includes('POS-SALE-001'), true);
});

test('UC_POS_038: validateCancelLineRequest', () => {
  const valid = validateCancelLineRequest('Open', 2);
  assert.equal(valid.canCancel, true);

  const completed = validateCancelLineRequest('Completed', 1);
  assert.equal(completed.canCancel, false);

  const invalidQty = validateCancelLineRequest('Open', 0);
  assert.equal(invalidQty.canCancel, false);
});

test('UC_POS_039: validateCancelBillRequest', () => {
  const valid = validateCancelBillRequest('Open', 'Khách đổi ý không mua');
  assert.equal(valid.canCancel, true);

  const noReason = validateCancelBillRequest('Open', '   ');
  assert.equal(noReason.canCancel, false);

  const cancelled = validateCancelBillRequest('Cancelled', 'Lý do');
  assert.equal(cancelled.canCancel, false);
});

test('UC_POS_040: validateReturnRefundRequest', () => {
  const valid = validateReturnRefundRequest('Completed', 50000, 100000);
  assert.equal(valid.canRefund, true);

  const overRefund = validateReturnRefundRequest('Completed', 150000, 100000);
  assert.equal(overRefund.canRefund, false);
  assert.match(overRefund.error!, /không được lớn hơn/);

  const uncompleted = validateReturnRefundRequest('Open', 50000, 100000);
  assert.equal(uncompleted.canRefund, false);
});

test('UC_POS_040: formatRefundSummary', () => {
  const summary = formatRefundSummary(200000, 50000);
  assert.equal(summary.includes('200.000'), true);
  assert.equal(summary.includes('50.000'), true);
  assert.equal(summary.includes('150.000'), true);
});
