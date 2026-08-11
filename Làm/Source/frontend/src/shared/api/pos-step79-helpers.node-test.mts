import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateVoucherCode,
  formatVoucherNotice,
  validateManualDiscount,
  formatManualDiscountStatus,
  validateOpenSaleRequest,
  formatAreaDisplay,
  validateSaleLineInput,
  formatSaleLineRow,
} from './pos-step79-helpers.ts';

// ─── UC_POS_022: Nhập mã voucher ───

test('UC_POS_022 — validateVoucherCode — valid voucher passes', () => {
  const result = validateVoucherCode('VOUCHER-50K');
  assert.equal(result.isValid, true);
});

test('UC_POS_022 — formatVoucherNotice — formats notice', () => {
  const text = formatVoucherNotice('VOUCHER-50K', 50000);
  assert.ok(text.includes('VOUCHER-50K'));
  assert.ok(text.includes('50.000 VNĐ'));
});

// ─── UC_POS_024: Giảm giá tay ───

test('UC_POS_024 — validateManualDiscount — valid discount passes', () => {
  const result = validateManualDiscount('Percent', 15);
  assert.equal(result.isValid, true);
});

test('UC_POS_024 — formatManualDiscountStatus — Pending status formats correctly', () => {
  const status = formatManualDiscountStatus('Pending', 10, 'Percent');
  assert.ok(status.label.includes('Chờ quản lý duyệt'));
  assert.equal(status.icon, 'clock');
});

// ─── UC_POS_026: Mở đơn / chọn khu vực ───

test('UC_POS_026 — validateOpenSaleRequest — valid shift passes', () => {
  const result = validateOpenSaleRequest('shift-guid-1');
  assert.equal(result.isValid, true);
});

test('UC_POS_026 — formatAreaDisplay — formats area name or takeaway', () => {
  assert.ok(formatAreaDisplay('Bàn 05').includes('Bàn 05'));
  assert.ok(formatAreaDisplay(undefined).includes('mang đi'));
});

// ─── UC_POS_027: Thêm / sửa / xóa sản phẩm ───

test('UC_POS_027 — validateSaleLineInput — valid input passes', () => {
  const result = validateSaleLineInput(2, 35000);
  assert.equal(result.isValid, true);
});

test('UC_POS_027 — formatSaleLineRow — formats sale line row text', () => {
  const row = formatSaleLineRow(1, 'CF-01', 'Cà Phê Sữa', 2, 25000);
  assert.ok(row.includes('1. [CF-01] Cà Phê Sữa x2'));
  assert.ok(row.includes('50.000 VNĐ'));
});
