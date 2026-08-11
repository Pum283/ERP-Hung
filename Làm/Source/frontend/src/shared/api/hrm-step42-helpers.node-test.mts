import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePayrollPolicyInput,
  calculateInsuranceDeductions,
  calculatePersonalTax,
  formatInsuranceRatesSummary,
  type PayrollPolicyInput,
} from './hrm-step42-helpers.ts';

// ─── UC_HRM_160 & 161: validatePayrollPolicyInput ───

test('validatePayrollPolicyInput - valid policy returns valid', () => {
  const input: PayrollPolicyInput = {
    socialInsuranceEmpRate: 0.08,
    healthInsuranceEmpRate: 0.015,
    unemploymentEmpRate: 0.01,
    personalDeduction: 11000000,
    flatTaxRate: 0.05,
    standardWorkDays: 26,
    otMultiplier: 1.5,
  };
  const res = validatePayrollPolicyInput(input);
  assert.equal(res.valid, true);
});

test('validatePayrollPolicyInput - invalid insurance rate returns error', () => {
  const input: PayrollPolicyInput = {
    socialInsuranceEmpRate: 1.5, // > 1.0
    healthInsuranceEmpRate: 0.015,
    unemploymentEmpRate: 0.01,
    personalDeduction: 11000000,
    flatTaxRate: 0.05,
    standardWorkDays: 26,
    otMultiplier: 1.5,
  };
  const res = validatePayrollPolicyInput(input);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('BHXH NLĐ'));
});

// ─── UC_HRM_160: calculateInsuranceDeductions & formatInsuranceRatesSummary ───

test('calculateInsuranceDeductions - calculates insurance components correctly', () => {
  const res = calculateInsuranceDeductions(10000000, 0.08, 0.015, 0.01);
  assert.equal(res.social, 800000);
  assert.equal(res.health, 150000);
  assert.equal(res.unemp, 100000);
  assert.equal(res.totalInsurance, 1050000);
});

test('formatInsuranceRatesSummary - formats total insurance percentage correctly', () => {
  const summary = formatInsuranceRatesSummary(0.08, 0.015, 0.01);
  assert.ok(summary.includes('10.5%'));
});

// ─── UC_HRM_161 & 162: calculatePersonalTax ───

test('calculatePersonalTax - calculates taxable income and tax amount correctly', () => {
  const res = calculatePersonalTax(20000000, 2100000, 11000000, 0.05);
  // Taxable = 20M - 2.1M - 11M = 6.9M
  // Tax = 6.9M * 0.05 = 345,000
  assert.equal(res.taxableIncome, 6900000);
  assert.equal(res.taxAmount, 345000);
});

test('calculatePersonalTax - income below deduction returns 0 tax', () => {
  const res = calculatePersonalTax(10000000, 1050000, 11000000, 0.05);
  assert.equal(res.taxableIncome, 0);
  assert.equal(res.taxAmount, 0);
});
