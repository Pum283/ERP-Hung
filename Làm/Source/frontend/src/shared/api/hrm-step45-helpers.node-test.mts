import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatPayslipDetail,
  validateBankExportRow,
  calculateCostByOrgTotals,
  formatCostByOrgRow,
  type CostByOrgRow,
} from './hrm-step45-helpers.ts';

// ─── UC_HRM_171: formatPayslipDetail ───

test('formatPayslipDetail - returns correct number of items with proper types', () => {
  const items = formatPayslipDetail(16000000, 16000000, 1500000, 800000, 500000, 1680000, 250000, 0, 18800000, 16870000);
  assert.equal(items.length, 10);
  assert.equal(items.filter(i => i.type === 'income').length, 5);
  assert.equal(items.filter(i => i.type === 'deduction').length, 3);
  assert.equal(items.filter(i => i.type === 'summary').length, 2);
});

test('formatPayslipDetail - net pay item has correct label', () => {
  const items = formatPayslipDetail(16000000, 16000000, 0, 0, 0, 0, 0, 0, 16000000, 16000000);
  const netItem = items.find(i => i.label.includes('Net'));
  assert.ok(netItem);
  assert.equal(netItem.amount, 16000000);
});

// ─── UC_HRM_173: validateBankExportRow ───

test('validateBankExportRow - valid row returns valid', () => {
  const res = validateBankExportRow({
    employeeCode: 'EMP001',
    employeeName: 'Nguyễn Văn A',
    amount: 15000000,
    content: 'Chi luong EMP001',
  });
  assert.equal(res.valid, true);
});

test('validateBankExportRow - zero amount returns error', () => {
  const res = validateBankExportRow({
    employeeCode: 'EMP001',
    employeeName: 'Nguyễn Văn A',
    amount: 0,
    content: 'Chi luong EMP001',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn 0'));
});

// ─── UC_HRM_175: calculateCostByOrgTotals & formatCostByOrgRow ───

test('calculateCostByOrgTotals - sums all org rows correctly', () => {
  const rows: CostByOrgRow[] = [
    { orgUnitName: 'Phòng A', headCount: 10, totalGross: 200000000, totalNet: 170000000, totalInsurance: 20000000 },
    { orgUnitName: 'Phòng B', headCount: 5, totalGross: 100000000, totalNet: 85000000, totalInsurance: 10000000 },
  ];
  const totals = calculateCostByOrgTotals(rows);
  assert.equal(totals.totalGross, 300000000);
  assert.equal(totals.totalNet, 255000000);
  assert.equal(totals.totalHeadCount, 15);
});

test('formatCostByOrgRow - formats org cost row correctly', () => {
  const row: CostByOrgRow = { orgUnitName: 'Phòng IT', headCount: 8, totalGross: 160000000, totalNet: 136000000, totalInsurance: 16000000 };
  const text = formatCostByOrgRow(row);
  assert.ok(text.includes('Phòng IT'));
  assert.ok(text.includes('8 người'));
});
